
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRRealTimeChatSimulation
{
    // Represents a user in the chat system.
    // In a real SignalR application, this would be tied to a ConnectionId.
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    // Represents a chat message.
    public class ChatMessage
    {
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // Simulates the SignalR Hub.
    // In ASP.NET Core, this class would inherit from Microsoft.AspNetCore.SignalR.Hub.
    // Here, we simulate the core logic: managing groups and broadcasting messages.
    public class ChatHub
    {
        // Simulates the connected clients dictionary (ConnectionId -> User).
        // In a real scenario, SignalR manages this internally.
        private Dictionary<string, User> _connectedClients = new Dictionary<string, User>();

        // Simulates the groups (GroupName -> List of ConnectionIds).
        private Dictionary<string, List<string>> _groups = new Dictionary<string, List<string>>();

        // Simulates the client proxy (the interface to send messages back to the client).
        // We will implement a simple console output for this simulation.
        private IClientProxy _clientProxy;

        public ChatHub(IClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        // Simulates a client connecting to the hub.
        public async Task OnConnectedAsync(string connectionId, string userName)
        {
            var user = new User { Id = connectionId, Name = userName };
            _connectedClients[connectionId] = user;
            Console.WriteLine($"[Hub] User '{userName}' connected (ID: {connectionId}).");
            
            // Notify the specific client they are connected.
            await _clientProxy.SendToClient(connectionId, "Welcome to the AI Chat Hub!");
        }

        // Simulates a client joining a specific group (e.g., "AI-Assistants").
        public async Task AddToGroup(string connectionId, string groupName)
        {
            if (!_groups.ContainsKey(groupName))
            {
                _groups[groupName] = new List<string>();
            }

            if (!_groups[groupName].Contains(connectionId))
            {
                _groups[groupName].Add(connectionId);
                Console.WriteLine($"[Hub] User '{_connectedClients[connectionId].Name}' joined group '{groupName}'.");
                
                // Notify the group (excluding the sender usually, but here we notify all for simplicity).
                await _clientProxy.SendToGroup(groupName, $"{_connectedClients[connectionId].Name} has joined the group.");
            }
        }

        // Simulates sending a message to a specific group.
        public async Task SendGroupMessage(string connectionId, string groupName, string message)
        {
            if (_groups.ContainsKey(groupName) && _groups[groupName].Contains(connectionId))
            {
                var sender = _connectedClients[connectionId];
                var chatMessage = new ChatMessage
                {
                    SenderName = sender.Name,
                    Content = message,
                    Timestamp = DateTime.Now
                };

                Console.WriteLine($"[Hub] Broadcasting message from '{sender.Name}' to group '{groupName}': '{message}'");
                
                // Broadcast to the group.
                await _clientProxy.SendToGroup(groupName, $"[{chatMessage.Timestamp:HH:mm:ss}] {chatMessage.SenderName}: {chatMessage.Content}");
            }
            else
            {
                Console.WriteLine($"[Hub] Error: User not in group '{groupName}'.");
            }
        }

        // Simulates streaming AI tokens to a specific client.
        // This mimics the IAsyncEnumerable pattern for responsive UI.
        public async Task StreamAiResponse(string connectionId, string prompt)
        {
            Console.WriteLine($"[Hub] Starting AI token stream for user: {_connectedClients[connectionId].Name}");
            
            // Simulate the AI generating tokens one by one.
            string[] tokens = { "Here", " ", "is", " ", "the", " ", "streaming", " ", "response", "." };
            
            foreach (var token in tokens)
            {
                // Send the token immediately to the client.
                await _clientProxy.SendToClient(connectionId, $"AI_Token: {token}");
                
                // Simulate network latency or processing time.
                await Task.Delay(150); 
            }
            
            // Signal the end of the stream.
            await _clientProxy.SendToClient(connectionId, "AI_Token: [END_STREAM]");
        }

        // Simulates a client disconnecting.
        public async Task OnDisconnectedAsync(string connectionId)
        {
            if (_connectedClients.ContainsKey(connectionId))
            {
                var user = _connectedClients[connectionId];
                Console.WriteLine($"[Hub] User '{user.Name}' disconnected.");
                _connectedClients.Remove(connectionId);
                
                // Cleanup from groups (simplified logic)
                foreach (var group in _groups.Values)
                {
                    group.Remove(connectionId);
                }
            }
        }
    }

    // Interface for the Client Proxy (abstraction for sending messages).
    public interface IClientProxy
    {
        Task SendToClient(string connectionId, string message);
        Task SendToGroup(string groupName, string message);
    }

    // Implementation of the Client Proxy for the Console Application.
    // In a real web app, this would push data over WebSocket connections.
    public class ConsoleClientProxy : IClientProxy
    {
        public Task SendToClient(string connectionId, string message)
        {
            // Simulate receiving a message on a specific client connection.
            Console.WriteLine($"    [Client {connectionId} received]: {message}");
            return Task.CompletedTask;
        }

        public Task SendToGroup(string groupName, string message)
        {
            // Simulate broadcasting to a group.
            Console.WriteLine($"    [Group '{groupName}' broadcast]: {message}");
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== SignalR Real-Time Chat Simulation ===\n");

            // 1. Initialize the Hub and Client Proxy.
            IClientProxy proxy = new ConsoleClientProxy();
            ChatHub hub = new ChatHub(proxy);

            // 2. Simulate Client Connections.
            string connId1 = "conn-abc-123";
            string connId2 = "conn-xyz-789";
            
            await hub.OnConnectedAsync(connId1, "Alice");
            await hub.OnConnectedAsync(connId2, "Bob");

            Console.WriteLine();

            // 3. Simulate Group Management (Multi-User Chat).
            // Alice joins the "Developers" group.
            await hub.AddToGroup(connId1, "Developers");
            // Bob joins the "Developers" group.
            await hub.AddToGroup(connId2, "Developers");

            Console.WriteLine();

            // 4. Simulate Real-Time Messaging within the Group.
            await hub.SendGroupMessage(connId1, "Developers", "Hey team, check out this new SignalR feature!");
            await hub.SendGroupMessage(connId2, "Developers", "Looks great! Is it secure?");

            Console.WriteLine();

            // 5. Simulate AI Model Token Streaming (Async IAsyncEnumerable logic).
            // Alice requests an AI response.
            await hub.StreamAiResponse(connId1, "Explain SignalR security");

            Console.WriteLine();

            // 6. Simulate Disconnection.
            await hub.OnDisconnectedAsync(connId2);

            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}
