
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

// Program.cs
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// 2. Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("gpu_model", () => 
    {
        // Check if the model is loaded (simulated via a static flag)
        return AppState.IsModelLoaded 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Unhealthy("Model not loaded");
    });

// 4. Request Tracing Middleware (Registered early)
// 5. Concurrency Limiter (Singleton Semaphore)
builder.Services.AddSingleton<SemaphoreSlim>(new SemaphoreSlim(10, 10)); // Limit to 10 concurrent requests

var app = builder.Build();

// 4. Request Tracing Middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Request-ID"))
    {
        context.Request.Headers["X-Request-ID"] = Guid.NewGuid().ToString();
    }
    await next();
});

// 5. Concurrency Limiter Middleware
app.Use(async (context, next) =>
{
    var semaphore = context.RequestServices.GetRequiredService<SemaphoreSlim>();
    
    // Try to enter the semaphore immediately (non-blocking check)
    if (!semaphore.Wait(0))
    {
        context.Response.StatusCode = 503; // Service Unavailable
        context.Response.Headers["Retry-After"] = "10"; // Seconds
        await context.Response.WriteAsync("Server busy. Please retry later.");
        return;
    }

    try
    {
        await next();
    }
    finally
    {
        semaphore.Release();
    }
});

// 1. Hardware Awareness (Simulated)
app.MapGet("/hardware", (ILogger<Program> logger) =>
{
    // In a real app, use System.Device.Gpu to query CUDA/OpenCL devices
    var hasGpu = true; // Simulation
    if (hasGpu)
    {
        logger.LogInformation("GPU Detected: NVIDIA A100 (Simulated), Memory: 40GB");
        return Results.Ok(new { Device = "NVIDIA A100", Memory = "40GB" });
    }
    return Results.Ok(new { Device = "CPU Only" });
});

// 2. Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

// 3. Readiness Probe Simulation
app.MapGet("/ready", () =>
{
    if (AppState.StartupTime.AddSeconds(5) > DateTime.UtcNow)
    {
        // Simulate "Loading Model..."
        return Results.StatusCode(503); // Not Ready
    }
    return Results.Ok("Ready");
});

// Simulation of App State
app.Run();

static class AppState
{
    public static DateTime StartupTime { get; } = DateTime.UtcNow;
    public static bool IsModelLoaded => DateTime.UtcNow > StartupTime.AddSeconds(5);
}
