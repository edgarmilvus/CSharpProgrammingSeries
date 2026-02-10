
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Health Checks
// We register a custom health check that simulates verifying the AI model is loaded.
builder.Services.AddHealthChecks()
    .AddCheck("ai_model_ready", new AiModelHealthCheck());

var app = builder.Build();

// 2. Graceful Shutdown Configuration
// We need to handle SIGTERM (Docker stop) to finish processing requests.
// In ASP.NET Core, this is handled by the host, but we can hook into the stopping token.
// We simulate a long-running background process (e.g., processing a request).
app.Lifetime.ApplicationStopping.Register(() =>
{
    // This runs when SIGTERM is received.
    Console.WriteLine("SIGTERM received. Waiting for current request to finish...");
    // In a real scenario, we would wait for a semaphore or flag indicating request completion.
    Thread.Sleep(2000); // Simulate finishing current work
    Console.WriteLine("Cleanup complete. Exiting.");
});

// 3. Health Check Endpoint
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Mock endpoint to simulate processing
app.MapPost("/analyze", async (HttpContext context) =>
{
    // Simulate processing time
    await Task.Delay(500);
    return Results.Ok(new { sentiment = "Positive", confidence = 0.95 });
});

app.Run();

// Custom Health Check Implementation
public class AiModelHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Logic to verify model is loaded (e.g., check if model file exists in memory)
        bool isModelLoaded = true; // In reality, check a singleton service holding the model

        if (isModelLoaded)
        {
            return Task.FromResult(HealthCheckResult.Healthy("AI Model is loaded and ready."));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("AI Model is not loaded."));
        }
    }
}
