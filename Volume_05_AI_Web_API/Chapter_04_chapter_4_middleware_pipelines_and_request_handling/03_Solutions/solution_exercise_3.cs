
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class AiTrafficLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AiTrafficLoggingMiddleware> _logger;

    public AiTrafficLoggingMiddleware(RequestDelegate next, ILogger<AiTrafficLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process AI traffic
        if (!context.Request.Path.StartsWithSegments("/api/ai"))
        {
            await _next(context);
            return;
        }

        // 1. Enable buffering to allow multiple reads of the request body
        context.Request.EnableBuffering();

        // 2. Read the request body
        string requestBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
        }
        
        // 3. Reset the request stream position so downstream can read it
        context.Request.Body.Position = 0;

        // Log the prompt
        _logger.LogInformation("AI Prompt Received: {RequestBody}", requestBody);

        // 4. Intercept the response stream
        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // 5. Call the next middleware
            await _next(context);

            // 6. Read the response body from our memory stream
            responseBody.Position = 0;
            using var responseReader = new StreamReader(responseBody);
            var responseBodyContent = await responseReader.ReadToEndAsync();

            // Log the AI generation
            _logger.LogInformation("AI Generation Response: {ResponseBody}", responseBodyContent);

            // 7. Copy the content back to the original stream for the client
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalResponseBody);
        }
        finally
        {
            // Restore the original body to avoid memory leaks or disposal issues
            context.Response.Body = originalResponseBody;
        }
    }
}
