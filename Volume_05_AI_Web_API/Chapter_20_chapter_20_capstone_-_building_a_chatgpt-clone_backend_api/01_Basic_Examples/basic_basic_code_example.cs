
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

using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Channels;

// 1. Define the request DTO (Data Transfer Object)
public record ChatRequest(string Message);

// 2. Define the response DTO
public record ChatResponse(string Content, bool IsComplete);

// 3. Define the AI Service Interface
public interface IChatService
{
    IAsyncEnumerable<string> GetResponseStreamAsync(string userMessage);
}

// 4. Implement the Mock AI Service
// This simulates a real AI model generating text token-by-token.
public class MockChatService : IChatService
{
    public async IAsyncEnumerable<string> GetResponseStreamAsync(string userMessage)
    {
        // Simulate processing delay (like network latency or model inference time)
        await Task.Delay(200);

        // Simulate a simple "echo" logic for the AI
        string responseText = $"Echoing your message: '{userMessage}'";

        // Break the response into "tokens" (words) to simulate streaming
        string[] tokens = responseText.Split(' ');

        foreach (var token in tokens)
        {
            yield return token + " "; // Yield each token individually
            
            // Simulate the time it takes to generate the next token
            await Task.Delay(100); 
        }
    }
}

// 5. The API Controller
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("stream")]
    public async Task StreamChat([FromBody] ChatRequest request)
    {
        // Set headers for streaming
        Response.ContentType = "text/plain";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        // Get the stream from the service
        await foreach (var token in _chatService.GetResponseStreamAsync(request.Message))
        {
            // Write token to the response body
            await Response.WriteAsync(token);
            
            // Flush the stream immediately to ensure the client receives data in real-time
            await Response.Body.FlushAsync();
        }
    }
}

// 6. Program.cs (Entry Point)
var builder = WebApplication.CreateBuilder(args);

// Register dependencies
builder.Services.AddControllers();
builder.Services.AddSingleton<IChatService, MockChatService>(); // Register the mock service

var app = builder.Build();

// Middleware pipeline
app.UseRouting();
app.MapControllers();

// Run the application
app.Run("http://localhost:5000");
