
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// Minimal API for an AI Chat Endpoint with Middleware
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Custom Middleware: Request Logging
// Logs every incoming request to the console for observability.
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    Console.WriteLine($"[INFO] Request received: {context.Request.Method} {context.Request.Path}");
    
    // Capture the request body for logging (must be done before reading it in the endpoint)
    context.Request.EnableBuffering();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    context.Request.Body.Position = 0;
    
    if (!string.IsNullOrEmpty(body))
    {
        Console.WriteLine($"[DEBUG] Request Body: {body}");
    }

    // Call the next middleware in the pipeline
    await next(context);

    var elapsed = DateTime.UtcNow - start;
    Console.WriteLine($"[INFO] Request completed in {elapsed.TotalMilliseconds}ms with status {context.Response.StatusCode}");
});

// 2. Custom Middleware: API Key Authentication
// Simulates checking a header for a valid API key before allowing access to the AI model.
app.Use(async (context, next) =>
{
    // Only protect the /chat endpoint
    if (context.Request.Path.StartsWithSegments("/chat"))
    {
        // Check for the header "X-API-KEY"
        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Error: Missing API Key.");
            return;
        }

        // Simulate a valid key check (In production, validate against a database or secrets manager)
        if (apiKey != "sk-1234567890")
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Error: Invalid API Key.");
            return;
        }
    }

    await next(context);
});

// 3. AI Chat Endpoint
// Simulates generating an AI response based on a user prompt.
app.MapPost("/chat", async (HttpContext context, ChatRequest request) =>
{
    // Input Validation
    if (string.IsNullOrWhiteSpace(request.Prompt))
    {
        return Results.BadRequest("Prompt cannot be empty.");
    }

    // Simulate AI Model Processing (High Latency Workload)
    // We use a delay to mimic the time an LLM takes to generate text.
    await Task.Delay(2000); 

    // Simulate AI Response Generation
    var responseText = $"AI Response to '{request.Prompt}': Hello! I am processing your request asynchronously.";
    
    // Return JSON response
    var response = new ChatResponse { Response = responseText, Timestamp = DateTime.UtcNow };
    return Results.Json(response);
});

// 4. Global Exception Handling Middleware
// Catches any unhandled exceptions in the pipeline to prevent crashing the app.
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CRITICAL] Unhandled Exception: {ex.Message}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An internal server error occurred.");
    }
});

// Run the application
app.Run();

// Record definitions for JSON serialization
public record ChatRequest(string Prompt);
public record ChatResponse
{
    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }
}
