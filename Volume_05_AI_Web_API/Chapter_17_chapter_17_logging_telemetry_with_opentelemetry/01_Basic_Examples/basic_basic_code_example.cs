
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure OpenTelemetry Resources
// Define the service name and version to identify this application in telemetry backends.
var serviceName = "AI-Chat-API";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    
    // 2. Add Tracing (Distributed Tracing)
    // Tracks the lifecycle of a request across services.
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Automatically traces incoming HTTP requests
        .AddConsoleExporter()) // Export traces to the console for this demo (replace with Jaeger/OTLP in prod)
    
    // 3. Add Metrics
    // Collects quantitative data like request counts and latency histograms.
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation() // Collect HTTP request metrics
        .AddConsoleExporter());

// 4. Configure Logging
// We need to hook OpenTelemetry into the standard ILogger system.
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true; // Include scope information (e.g., request ID)
    options.ParseStateValues = true; // Parse log state into structured attributes
    options.AddConsoleExporter(); // Export logs to console
});

var app = builder.Build();

// 5. Define a Custom Activity Source for Manual Tracing
// This allows us to create spans for specific operations (e.g., model inference).
static class TelemetryConstants
{
    public static readonly ActivitySource ActivitySource = new("AI.Chat.API");
}

// 6. Create the Chat Endpoint
app.MapPost("/chat", async (HttpContext context) =>
{
    // Read the prompt from the request body
    var reader = new StreamReader(context.Request.Body);
    var prompt = await reader.ReadToEndAsync();
    
    // Start a manual span for the model inference process
    using var activity = TelemetryConstants.ActivitySource.StartActivity("Model.Inference");
    
    // Add tags (attributes) to the span for better filtering in observability tools
    activity?.SetTag("model.version", "v1.2");
    activity?.SetTag("prompt.length", prompt.Length);
    
    // Simulate AI Model Inference
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    // Structured Logging: Log the inference start with context
    logger.LogInformation("Starting model inference for prompt length: {PromptLength}", prompt.Length);
    
    // Simulate latency
    await Task.Delay(100); 
    
    // Simulate an error scenario for demonstration
    if (prompt.Contains("error"))
    {
        // Record an exception event on the span
        activity?.SetStatus(ActivityStatusCode.Error, "Simulated inference failure");
        
        // Structured Logging: Log the error
        logger.LogError("Model inference failed for prompt: {Prompt}", prompt);
        
        return Results.Problem("Model inference failed.");
    }

    // Record success
    activity?.SetStatus(ActivityStatusCode.Ok);
    logger.LogInformation("Model inference completed successfully.");
    
    return Results.Ok(new { response = "This is a generated AI response." });
});

app.Run();
