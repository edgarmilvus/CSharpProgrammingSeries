
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
#
# MIT License
# Copyright (c) 2026 Edgar Milvus
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
*/

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AudioStreamingApp.Exercises
{
    public class ConnectionMetadata
    {
        public WebSocket Socket { get; set; }
        public DateTime LastPingSent { get; set; }
        public double LastRTT { get; set; } // in ms
        public int KeepAliveInterval { get; set; } = 30; // Default seconds

        public ConnectionMetadata(WebSocket socket)
        {
            Socket = socket;
        }
    }

    // 1 & 2. Connection Manager
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, ConnectionMetadata> _connections = new();

        public string AddSocket(WebSocket socket)
        {
            var id = Guid.NewGuid().ToString();
            _connections.TryAdd(id, new ConnectionMetadata(socket));
            return id;
        }

        public void RemoveSocket(string id)
        {
            _connections.TryRemove(id, out _);
        }

        public ConnectionMetadata? GetSocketById(string id)
        {
            _connections.TryGetValue(id, out var metadata);
            return metadata;
        }

        public IEnumerable<string> GetAllIds() => _connections.Keys;

        public void UpdateMetadata(string id, Action<ConnectionMetadata> updateAction)
        {
            if (_connections.TryGetValue(id, out var metadata))
            {
                updateAction(metadata);
            }
        }
    }

    // 4. Keep-Alive Background Service
    public class KeepAliveService : BackgroundService
    {
        private readonly WebSocketConnectionManager _manager;
        private readonly ILogger<KeepAliveService> _logger;
        private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5)); // Check every 5s

        public KeepAliveService(WebSocketConnectionManager manager, ILogger<KeepAliveService> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                foreach (var id in _manager.GetAllIds())
                {
                    var metadata = _manager.GetSocketById(id);
                    if (metadata == null || metadata.Socket.State != WebSocketState.Open) continue;

                    // Adaptive Logic
                    // If RTT > 200ms, interval = 60s. If < 50ms, interval = 15s.
                    int interval = metadata.LastRTT switch
                    {
                        > 200 => 60,
                        < 50 => 15,
                        _ => 30
                    };

                    // Check if we need to ping based on the calculated interval
                    if ((DateTime.UtcNow - metadata.LastPingSent).TotalSeconds >= interval)
                    {
                        await SendPing(metadata.Socket, id);
                        metadata.LastPingSent = DateTime.UtcNow;
                    }
                }
            }
        }

        private async Task SendPing(WebSocket socket, string id)
        {
            try
            {
                var pingPayload = Encoding.UTF8.GetBytes($"PING|{DateTime.UtcNow.Ticks}");
                await socket.SendAsync(
                    new ArraySegment<byte>(pingPayload),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending ping to {id}");
                _manager.RemoveSocket(id);
            }
        }
    }

    // Interactive Challenge: Handling PONG to calculate RTT
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketConnectionManager _manager;

        public WebSocketMiddleware(RequestDelegate next, WebSocketConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                var id = _manager.AddSocket(socket);

                try
                {
                    var buffer = new byte[1024];
                    while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                        
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            
                            // Handle PONG response to calculate RTT
                            if (message.StartsWith("PONG|"))
                            {
                                var sentTicks = long.Parse(message.Split('|')[1]);
                                var nowTicks = DateTime.UtcNow.Ticks;
                                var rtt = TimeSpan.FromTicks(nowTicks - sentTicks).TotalMilliseconds;
                                
                                _manager.UpdateMetadata(id, m => m.LastRTT = rtt);
                            }
                        }
                    }
                }
                finally
                {
                    _manager.RemoveSocket(id);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

    /*
     * SCALING DISCUSSION:
     * 
     * The ConcurrentDictionary approach fails in a load-balanced scenario because 
     * it is scoped to a single server instance's memory. If a client connects to Server A,
     * but the next request (or WebSocket frame) is routed to Server B, Server B has no 
     * knowledge of the connection ID or metadata stored on Server A.
     * 
     * SOLUTION:
     * 1. Use a Distributed Cache (Redis) to store connection metadata (IDs, RTT, etc.).
     * 2. Use a Backplane (like SignalR's Redis backplane or a custom Pub/Sub) to broadcast 
     *    messages (like Pings) to the correct server instance holding the socket.
     * 3. Alternatively, implement Sticky Sessions at the Load Balancer level to ensure 
     *    a client always hits the same server instance, though this limits scaling flexibility.
     */
}
