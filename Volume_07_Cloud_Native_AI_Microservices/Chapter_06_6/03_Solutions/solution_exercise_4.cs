
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

// File: Program.cs (OTel & Baggage)
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. OpenTelemetry Setup
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("sentiment-agent"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation() // Auto-instrument HttpClient
        .AddOtlpExporter(options => 
        {
            // In K8s, this URL points to the OTel Collector
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
        }));

var app = builder.Build();

// 2. Baggage Propagation Helper
var textMapPropagator = new TraceContextPropagator();

app.MapPost("/analyze", async (AnalysisRequest request, HttpContext httpContext) =>
{
    // Extract existing context/headers
    var parentContext = textMapPropagator.Extract(default, httpContext.Request.Headers, (headers, key) => headers[key].ToArray());
    
    // Inject the extracted context into Activity.Current (if using ActivitySource) or Baggage
    Baggage.Current = parentContext.Baggage;

    // 3. Read Correlation ID from Header and add to Baggage
    if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
    {
        Baggage.Set("correlation.id", correlationId!);
    }

    // 4. Custom Span for Batching Logic
    using var batchActivity = Activity.Current?.Source.StartActivity("Inference/BatchProcessing", ActivityKind.Internal);
    
    if (batchActivity != null)
    {
        // Add Attributes (Tags)
        batchActivity.SetTag("batch.size", 8); // Example size
        batchActivity.SetTag("model.version", "v1.2");
        
        var sw = Stopwatch.StartNew();
        
        // Simulate Work
        await Task.Delay(50); 
        
        sw.Stop();
        batchActivity.SetTag("processing.duration_ms", sw.ElapsedMilliseconds);
    }

    // 5. Downstream Call (Baggage automatically propagated via Instrumentation)
    // If we create an HttpClient here, the configured instrumentation should propagate the Baggage
    // (Note: Standard instrumentation propagates TraceContext; Baggage requires explicit handling in some versions, 
    // but modern OTel SDK handles it via the Activity.Current Baggage property).

    return Results.Ok(new { Sentiment = "Analyzed", CorrelationId = Baggage.Get("correlation.id")?.Value });
});

app.Run();

public record AnalysisRequest(string Text);
