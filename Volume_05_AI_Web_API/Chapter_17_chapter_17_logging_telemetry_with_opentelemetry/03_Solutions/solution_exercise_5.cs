
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Custom ActivitySource
public static class AIActivitySource
{
    public static readonly ActivitySource Source = new("MyCompany.AI.Inference");
}

// 2. Custom Processor for Enrichment
public class ModelVersionProcessor : BaseProcessor<Activity>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ModelVersionProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnStart(Activity data)
    {
        // Check if this activity is for an HTTP request
        if (data.Kind == ActivityKind.Server)
        {
            // 4. Dynamic Feature Flag Logic
            var context = _httpContextAccessor.HttpContext;
            var version = context?.Request.Headers["X-Model-Version"].ToString() ?? "v1.2.0";

            // Sanitize label to prevent cardinality explosion (e.g., limit to known versions)
            if (version != "v1.3.0") version = "v1.2.0"; 

            // Set tag on the activity
            data.SetTag("model.version", version);
        }
    }
}

// Setup OpenTelemetry
builder.Services.AddHttpContextAccessor(); // Needed to access request headers in processor
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("AI-Service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter()
        .AddProcessor<ModelVersionProcessor>()); // Register custom processor

var app = builder.Build();

app.MapPost("/api/classify", async (ClassificationRequest request) =>
{
    // 5. Dynamic Override for specific request scope
    // This ensures the specific inference span gets the correct tag even if 
    // the processor hasn't run on it yet or we want to override.
    var currentActivity = Activity.Current;
    var headerVersion = currentActivity?.GetTagItem("model.version")?.ToString() 
                        ?? "v1.2.0";
    
    // Start a custom span
    using var activity = AIActivitySource.Source.StartActivity("ModelInference");
    
    // Explicitly set the version on this specific child span
    activity?.SetTag("model.version", headerVersion);
    activity?.SetTag("model.name", "Transformer-V2");

    // Simulate async work
    await Task.Delay(100);

    return Results.Ok(new { Version = headerVersion });
});

app.Run();

public record ClassificationRequest(string Text);
