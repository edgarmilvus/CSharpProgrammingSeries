
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
using System.Text.Json;

namespace SseStreamingDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    /// <summary>
    /// Simulates a streaming LLM response using Server-Sent Events (SSE).
    /// </summary>
    /// <param name="prompt">The user input (not used in this simulation).</param>
    /// <returns>A stream of text/event-stream formatted data.</returns>
    [HttpGet("stream")]
    public async Task StreamChat([FromQuery] string prompt = "Hello")
    {
        // 1. Set the standard SSE content type.
        // This tells the browser to treat the connection as an event stream.
        Response.ContentType = "text/event-stream";

        // 2. Create a simulated sequence of tokens.
        // In a real app, this would come from an LLM library (e.g., Azure OpenAI SDK).
        var tokens = new List<string> { "Hello", " ", "World", "!", " This", " is", " a", " streaming", " demo." };

        // 3. Iterate over the tokens asynchronously.
        foreach (var token in tokens)
        {
            // 4. Format the token according to the SSE specification.
            // Format: "data: {json_payload}\n\n"
            // We wrap the token in JSON to send metadata (e.g., timestamp, role) if needed.
            var jsonPayload = JsonSerializer.Serialize(new { token = token });
            var sseMessage = $"data: {jsonPayload}\n\n";

            // 5. Convert to bytes and write to the response body.
            var bytes = Encoding.UTF8.GetBytes(sseMessage);
            await Response.Body.WriteAsync(bytes, 0, bytes.Length);

            // 6. Flush the stream immediately.
            // Without this, the buffer might hold the data until the connection closes.
            await Response.Body.FlushAsync();

            // 7. Simulate network/processing delay.
            await Task.Delay(100); 
        }
    }
}
