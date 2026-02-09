
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

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// 1. Define Metrics
public static class AIMetrics
{
    private static readonly Meter Meter = new("MyCompany.AI", "1.0.0");
    
    // Counter with ModelVersion tag
    public static readonly Counter<long> ModelRequests = Meter.CreateCounter<long>(
        "ai_model_requests_total", 
        description: "Total number of model inference requests");

    // Histogram with specific buckets
    public static readonly Histogram<double> InferenceDuration = Meter.CreateHistogram<double>(
        "ai_model_inference_duration_seconds",
        unit: "s",
        description: "Model inference latency distribution");
}

// 2. Configure OpenTelemetry Metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("AI-Service"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("MyCompany.AI") // Listen to our custom meter
        .AddView("ai_model_inference_duration_seconds", new ExplicitBucketHistogramConfiguration 
        { 
            Boundaries = new[] { 0.1, 0.5, 1.0, 2.0 } 
        })
        .AddPrometheusExporter()); // Export to Prometheus format

var app = builder.Build();

// 3. Expose /metrics endpoint
app.MapGet("/metrics", async context =>
{
    // OpenTelemetry Prometheus exporter writes directly to the response stream
    var exporter = context.RequestServices.GetRequiredService<PrometheusExporter>();
    await exporter.WriteMetricsResponse(context.Response);
});

app.MapPost("/api/classify", (ClassificationRequest request) =>
{
    // 4. Increment Counter with Tags
    var modelVersion = "v1.2.0";
    AIMetrics.ModelRequests.Add(1, 
        new KeyValuePair<string, object?>("model_version", modelVersion));

    // 5. Record Histogram with Tags
    var startTime = DateTime.UtcNow;
    
    // Simulate work
    System.Threading.Thread.Sleep(new Random().Next(50, 200)); 
    
    var duration = (DateTime.UtcNow - startTime).TotalSeconds;
    
    AIMetrics.InferenceDuration.Record(duration,
        new KeyValuePair<string, object?>("model_version", modelVersion));

    return Results.Ok(new { Result = "Classified" });
});

app.Run();

public record ClassificationRequest(string Text);
