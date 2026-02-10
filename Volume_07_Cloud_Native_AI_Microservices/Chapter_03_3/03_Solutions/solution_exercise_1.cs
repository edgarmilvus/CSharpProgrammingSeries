
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// 1. Custom ActivitySource
public static class InferenceAgentSource
{
    public static readonly ActivitySource Source = new("InferenceAgentSource", "1.0.0");
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 2. Telemetry Setup: Configure OpenTelemetry SDK
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Orchestrator"))
            .WithTracing(tracing => 
            {
                tracing
                    // Instrument standard libraries (HttpClient, ASP.NET Core)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // 5. Filter: Exclude health checks from tracing
                        options.Filter = (context) => 
                        {
                            return !context.Request.Path.StartsWith("/health") 
                                && !context.Request.Path.StartsWith("/ready");
                        };
                        // Enrich spans with metadata
                        options.Enrich = (activity, eventName, rawObject) =>
                        {
                            if (eventName == "Microsoft.AspNetCore.Http.RequestInStart")
                            {
                                if (rawObject is HttpContext httpContext)
                                {
                                    activity.SetTag("http.user_agent", httpContext.Request.Headers["User-Agent"]);
                                }
                            }
                        };
                    })
                    .AddHttpClientInstrumentation()
                    // Export to OTLP (e.g., Jaeger/Zipkin)
                    .AddOtlpExporter()
                    .AddSource(InferenceAgentSource.Source.Name); // Add custom source
            })
            .WithMetrics(metrics => metrics.AddPrometheusExporter()); // Optional: for metrics

        var app = builder.Build();

        // 3. Custom Middleware for Activity Enrichment
        app.Use(async (context, next) =>
        {
            // Start a custom Activity for the request processing logic
            using var activity = InferenceAgentSource.Source.StartActivity("AgentProcessing");
            
            if (activity != null)
            {
                activity.SetTag("http.method", context.Request.Method);
                activity.SetTag("http.path", context.Request.Path);
                
                // 4. Baggage: Add correlation ID to Baggage
                // This propagates down automatically via headers
                var correlationId = context.Request.Headers["X-Correlation-ID"].ToString() ?? Guid.NewGuid().ToString();
                Baggage.SetBaggage("correlationId", correlationId);
            }

            await next();
        });

        // 5. Custom Span in Orchestrator Logic
        app.MapPost("/orchestrate", async (HttpContext ctx) =>
        {
            // This logic represents the AgentDecisionLoop
            using var activity = InferenceAgentSource.Source.StartActivity("AgentDecisionLoop");
            
            // Simulate decision logic
            var decision = "classify"; 
            
            // Call downstream service (HttpClient automatically injects traceparent)
            var client = ctx.RequestServices.GetRequiredService<HttpClient>();
            await client.PostAsync($"http://classifier-service/api/{decision}", new StringContent(""));

            return Results.Ok();
        });

        app.Run();
    }
}
