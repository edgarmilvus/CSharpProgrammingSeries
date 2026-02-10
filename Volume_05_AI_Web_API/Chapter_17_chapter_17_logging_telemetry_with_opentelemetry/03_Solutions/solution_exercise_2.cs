
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

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("AI-Model-Service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Auto-instrumentation for HTTP requests
        .AddHttpClientInstrumentation() // Auto-instrumentation for HttpClient
        .AddConsoleExporter()); // Export to console

// 2. Register HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// 3. Custom ActivitySource
public static class AIActivitySource
{
    public static readonly ActivitySource Source = new("MyCompany.AI.Inference");
}

app.MapPost("/api/classify", async (ClassificationRequest request, HttpClient httpClient) =>
{
    // Simulate calling an external dependency (e.g., DB or Auth service)
    // This span is created automatically by AddHttpClientInstrumentation
    await httpClient.GetStringAsync("https://httpbin.org/get");

    // 4. Manual Span Creation
    using var activity = AIActivitySource.Source.StartActivity("ModelInference");
    
    if (activity != null)
    {
        activity.SetTag("model.name", "SentimentBERT");
        activity.SetTag("model.latency", "45ms");
    }

    // 5. Async Boundary Context Propagation
    // The 'activity' variable is automatically tracked by OpenTelemetry's 
    // AsyncLocal mechanism. Even if we await, the context flows.
    await Task.Delay(50); 
    
    // Simulate another internal operation
    await PerformInternalCalculation();

    return Results.Ok(new { Status = "Processed" });
});

// Demonstrating context propagation across methods
async Task PerformInternalCalculation()
{
    // Activity.Current is automatically populated here because we awaited
    // inside the parent activity scope.
    using var childActivity = AIActivitySource.Source.StartActivity("InternalCalculation");
    childActivity?.SetTag("calculation.type", "Normalization");
    await Task.Delay(20);
}

app.Run();

public record ClassificationRequest(string Text);
