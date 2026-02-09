
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Define the Hub Interface
// This interface defines the methods that clients can call on the Hub.
public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
}

// 2. Define the Hub
// The Hub is the server-side class that handles communication.
// It inherits from Hub<T> to provide strong typing for client methods.
public class ChatHub : Hub<IChatClient>
{
    // This method is called by the client to send a message to all connected users.
    public async Task SendMessage(string user, string message)
    {
        // Broadcast the message to all clients connected to this hub.
        await Clients.All.ReceiveMessage(user, message);
    }

    // This method is called when a client connects.
    public override async Task OnConnectedAsync()
    {
        // Send a welcome message to the newly connected client.
        await Clients.Caller.ReceiveMessage("System", "Welcome to the chat!");
        await base.OnConnectedAsync();
    }
}

// 3. Define the Program
// This sets up the web application host and configures services and middleware.
public class Program
{
    public static void Main(string[] args)
    {
        // Create a web application builder.
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // SignalR requires the SignalR service to be registered.
        builder.Services.AddSignalR();

        // Build the application.
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // Map the SignalR Hub to the "/chatHub" endpoint.
        // This is where the WebSocket connection is established.
        app.MapHub<ChatHub>("/chatHub");

        // Run the application.
        app.Run();
    }
}
