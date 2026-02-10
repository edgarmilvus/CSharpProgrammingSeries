
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AudioStreamingApp.Exercises
{
    // Data structure for the jitter buffer simulation
    public record struct AudioFrame(byte[] Payload, DateTime Timestamp);

    public class WebSocketEchoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketEchoMiddleware> _logger;

        public WebSocketEchoMiddleware(RequestDelegate next, ILogger<WebSocketEchoMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                // Configure options for large audio frames (64KB)
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync(new WebSocketAcceptContext
                {
                    // Note: In .NET 8, MaxMessageSize can be configured here or globally
                });

                _logger.LogInformation("WebSocket connection established.");

                // Interactive Challenge: Jitter Buffer Simulation
                // Use an unbounded channel to decouple receiving from sending
                var channel = Channel.CreateUnbounded<AudioFrame>();

                // Start the sender task (consumer)
                var senderTask = SendFramesWithJitter(webSocket, channel.Reader);

                // Start the receiver loop (producer)
                await ReceiveFrames(webSocket, channel.Writer);
                
                // Wait for the sender to finish (handles client close)
                await senderTask;
            }
            else
            {
                await _next(context);
            }
        }

        private async Task ReceiveFrames(WebSocket webSocket, ChannelWriter<AudioFrame> writer)
        {
            var buffer = new byte[1024 * 64]; // 64KB buffer
            var stopwatch = new Stopwatch();
            DateTime lastFrameTime = DateTime.UtcNow;
            long totalBytesReceived = 0;

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        writer.Complete(); // Signal sender to stop
                        break;
                    }

                    // Calculate throughput immediately (Constraint: No buffering)
                    stopwatch.Restart();
                    var receivedBytes = result.Count;
                    totalBytesReceived += receivedBytes;
                    
                    var timeSinceLastFrame = (DateTime.UtcNow - lastFrameTime).TotalSeconds;
                    if (timeSinceLastFrame > 0)
                    {
                        var throughputKB = (receivedBytes / 1024.0) / timeSinceLastFrame;
                        // _logger.LogDebug($"Throughput: {throughputKB:F2} KB/s");
                    }
                    lastFrameTime = DateTime.UtcNow;

                    // Handle Text (Control) vs Binary (Audio)
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        _logger.LogInformation($"Control Message: {message}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Copy data to a new array to send to the channel
                        // In a high-performance scenario, we might use MemoryPool, 
                        // but for clarity, we copy here.
                        var payload = new byte[receivedBytes];
                        Buffer.BlockCopy(buffer, 0, payload, 0, receivedBytes);

                        // Write to channel for the Jitter simulation
                        await writer.WriteAsync(new AudioFrame(payload, DateTime.UtcNow));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving WebSocket data");
                writer.Complete(ex);
            }
        }

        private async Task SendFramesWithJitter(WebSocket webSocket, ChannelReader<AudioFrame> reader)
        {
            var random = new Random();
            try
            {
                // Read from channel until it completes
                await foreach (var frame in reader.ReadAllAsync())
                {
                    // Simulate Jitter (10ms to 50ms delay)
                    var delay = random.Next(10, 50);
                    await Task.Delay(delay);

                    // Echo back to client
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(frame.Payload),
                        WebSocketMessageType.Binary,
                        true, // EndOfMessage
                        CancellationToken.None
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WebSocket data");
            }
        }
    }

    // Extension method to map the middleware
    public static class WebSocketEchoExtensions
    {
        public static IApplicationBuilder UseWebSocketEcho(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketEchoMiddleware>();
        }
    }
}
