
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

// File: Middleware/AiInputValidationMiddleware.cs
using System.Text;

namespace AiApi.Middleware
{
    public class AiInputValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public AiInputValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only validate specific paths
            if (context.Request.Path.StartsWithSegments("/api/inference"))
            {
                // Ensure the request body can be read multiple times
                context.Request.EnableBuffering();

                // Read the stream asynchronously
                using var reader = new StreamReader(
                    context.Request.Body, 
                    encoding: Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: false, 
                    leaveOpen: true); // Important: Leave the stream open so the controller can read it later

                var body = await reader.ReadToEndAsync();

                // Basic validation: Check if body is empty
                if (string.IsNullOrWhiteSpace(body))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Input data cannot be empty for AI inference.\"}");
                    return; // Stop execution here
                }

                // Reset the stream position so the controller can read the body
                context.Request.Body.Position = 0;
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}

// File: Program.cs
using AiApi.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

// 1. Error handling middleware (generic)
app.UseExceptionHandler();

// 2. Custom AI Input Validation Middleware
app.UseMiddleware<AiInputValidationMiddleware>();

// 3. Routing and endpoints
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
