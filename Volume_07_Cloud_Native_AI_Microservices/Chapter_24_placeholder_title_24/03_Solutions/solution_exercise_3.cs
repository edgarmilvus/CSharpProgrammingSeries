
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

// Add NuGet packages: OpenTelemetry, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Exporter.Jaeger

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// 1. OpenTelemetry Tracing & Metrics
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("AiAgentService") // Custom Activity Source
        .AddJaegerExporter(o => 
        {
            o.AgentHost = "jaeger-agent.istio-system.svc.cluster.local";
            o.AgentPort = 6831;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("AiAgentService") // Custom Meter
        .AddPrometheusExporter()); // Exposes metrics at /metrics for Prometheus scraping

// 2. Custom Metric Definition
public static class AiMetrics
{
    public static readonly Meter AiAgentMeter = new("AiAgentService");
    public static readonly Counter<long> InferenceRequests = AiAgentMeter.CreateCounter<long>("inference_requests_total", "requests", "Total inference requests");
}

var app = builder.Build();

// Expose metrics endpoint for Prometheus (if not using the exporter middleware)
app.MapGet("/metrics", async () => 
{
    // In a real setup, the Prometheus exporter middleware handles this.
    return "Metrics endpoint handled by OpenTelemetry Prometheus Exporter";
});

app.MapPost("/analyze", async (HttpContext context) =>
{
    // Start an Activity for distributed tracing
    using var activity = Activity.Current?.Source.StartActivity("AnalyzeSentiment");
    
    // Increment custom metric
    AiMetrics.InferenceRequests.Add(1);

    // Simulate processing
    await Task.Delay(100);
    
    // Simulate calling Redis (would be instrumented automatically if using StackExchange.Redis with OpenTelemetry)
    using var redisActivity = Activity.Current?.Source.StartActivity("RedisQuery");
    await Task.Delay(20);

    return Results.Ok(new { sentiment = "Positive" });
});

app.Run();
