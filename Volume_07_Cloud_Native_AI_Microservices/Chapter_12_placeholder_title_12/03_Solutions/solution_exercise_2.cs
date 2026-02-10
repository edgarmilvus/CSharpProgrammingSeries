
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

// SentimentAnalysis.Api/Program.cs
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SentimentAnalysis.Core;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration (appsettings.json + Env Vars)
// Kestrel and Logging levels are configured via builder.Configuration

// 2. Dependency Injection
builder.Services.AddSingleton<ISentimentAnalyzer, SentimentAnalyzer>();

// 3. Health Checks
// Live: Just checks if the app process is running.
// Ready: Checks dependencies (e.g., mock external registry).
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck("external_model_registry", () => 
    {
        // Simulate a check to an external dependency
        // In a real scenario, check DB connection or external API
        var random = new Random();
        return random.Next(100) > 5 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Degraded("Model registry connection is slow");
    });

var app = builder.Build();

// 4. Structured Logging Middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var correlationId = Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    
    logger.LogInformation("Started processing request {Method} {Path} with Correlation ID {CorrelationId}", 
        context.Request.Method, context.Request.Path, correlationId);

    await next();

    logger.LogInformation("Finished processing request {StatusCode} for {Path}", 
        context.Response.StatusCode, context.Request.Path);
});

// 5. Endpoints
app.MapPost("/api/analyze", async (ISentimentAnalyzer analyzer, AnalyzeRequest request, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        logger.LogWarning("Received empty text for analysis");
        return Results.BadRequest(new { error = "Text is required." });
    }

    var result = analyzer.Analyze(request.Text);
    
    logger.LogInformation("Analysis complete: {Sentiment} with confidence {Confidence}", 
        result.Sentiment, result.Confidence);

    return Results.Ok(result);
});

// 6. Health Check Endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = "Alive" }));
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("external"), // Only check dependencies
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.Run();

// DTOs
public record AnalyzeRequest(string Text);
