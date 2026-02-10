
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class InferenceController : ControllerBase
{
    [HttpGet("stream")]
    public async Task StreamCompletion(string prompt)
    {
        // Set SSE headers
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        // Simulate generating tokens
        var tokens = new[] { "The", " answer", " is", " probably", " correct", "." };
        
        try
        {
            foreach (var token in tokens)
            {
                // Check if client disconnected
                if (HttpContext.RequestAborted.IsCancellationRequested)
                    break;

                // Format as SSE: "data: {json}\n\n"
                var sseMessage = $"data: {{\"token\": \"{token}\"}}\n\n";
                await Response.WriteAsync(sseMessage);
                
                // Flush immediately to ensure client receives data
                await Response.Body.FlushAsync();

                // Simulate model latency
                await Task.Delay(100);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected, exit gracefully
        }
        
        // End of stream
        await Response.WriteAsync("data: [DONE]\n\n");
        await Response.Body.FlushAsync();
    }
}
