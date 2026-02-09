
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

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure the ProblemDetails service
// This enables RFC 7807 compliant error responses.
builder.Services.AddProblemDetails();

// 2. Register the Exception Handler
// This middleware intercepts unhandled exceptions before they reach the response stage.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// 3. Enable the Exception Handling Middleware
// This activates the pipeline defined in AddExceptionHandler.
app.UseExceptionHandler();

// A simple endpoint that simulates an AI processing failure
app.MapGet("/generate-image", (string prompt) =>
{
    // Simulate a logic error: The AI model requires a non-empty prompt
    if (string.IsNullOrWhiteSpace(prompt))
    {
        throw new InvalidOperationException("The AI prompt cannot be empty.");
    }

    // Simulate a critical infrastructure failure (e.g., GPU memory error)
    if (prompt.Contains("complex"))
    {
        throw new OutOfMemoryException("GPU memory exhausted processing complex prompt.");
    }

    return Results.Ok(new { ImageUrl = $"https://ai.service/image/{Guid.NewGuid()}" });
});

app.Run();

// ---------------------------------------------------------
// 4. Custom Exception Handler Implementation
// ---------------------------------------------------------
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // A. Logging the error (Crucial for debugging)
        _logger.LogError(exception, "An unhandled exception occurred.");

        // B. Determine the ProblemDetails instance based on exception type
        var problemDetails = CreateProblemDetails(httpContext, exception);

        // C. Write the standardized response
        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that the exception was handled
        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        // Map specific exceptions to HTTP status codes and AI-specific error types
        return exception switch
        {
            // Simulating an AI Content Filter violation
            UnauthorizedAccessException => new ProblemDetails
            {
                Title = "Access Denied",
                Detail = "The requested AI model is restricted.",
                Status = StatusCodes.Status403Forbidden,
                Type = "https://api.ai/errors/content-filter",
                Instance = httpContext.Request.Path,
                Extensions = { { "errorCode", "AI_403" } }
            },
            
            // Simulating a generic logic error (e.g., invalid input)
            InvalidOperationException => new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://api.ai/errors/invalid-input",
                Instance = httpContext.Request.Path,
                Extensions = { { "errorCode", "AI_400" } }
            },

            // Simulating a critical infrastructure failure
            OutOfMemoryException => new ProblemDetails
            {
                Title = "Service Unavailable",
                Detail = "The AI processing engine is currently overloaded.",
                Status = StatusCodes.Status503ServiceUnavailable,
                Type = "https://api.ai/errors/service-overload",
                Instance = httpContext.Request.Path,
                Extensions = { { "retryAfter", 30 } } // Custom extension for AI retry logic
            },

            // Fallback for all other unhandled exceptions
            _ => new ProblemDetails
            {
                Title = "An unexpected error occurred",
                Detail = "An internal error prevented the request from completing.",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://api.ai/errors/internal",
                Instance = httpContext.Request.Path,
                Extensions = { { "traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier } }
            }
        };
    }
}
