
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

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// 1. Register a Singleton to track metrics
builder.Services.AddSingleton<InferenceMetrics>();
builder.Services.AddHostedService<AdaptiveLimitAdjuster>();

// 2. Configure Concurrency Limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status503ServiceUnavailable; // 503 for busy

    options.AddPolicy("HeavyInference", context =>
    {
        // Get the current limit from the metrics service
        var metrics = context.RequestServices.GetRequiredService<InferenceMetrics>();
        int currentLimit = metrics.GetCurrentLimit();

        return RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: "GlobalInferencePool", // Shared across all users for this model
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = currentLimit, // Dynamic limit
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});

var app = builder.Build();

app.UseRateLimiter();

app.MapPost("/api/inference/heavy", async (HttpContext context) =>
{
    var metrics = context.RequestServices.GetRequiredService<InferenceMetrics>();
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    // Simulate heavy work (10-15 seconds)
    await Task.Delay(new Random().Next(10000, 15000));
    
    sw.Stop();
    metrics.RecordLatency(sw.ElapsedMilliseconds);

    return Results.Ok($"Generated in {sw.ElapsedMilliseconds}ms");
})
.RequireRateLimiting("HeavyInference");

app.Run();

// Supporting Classes
public class InferenceMetrics
{
    private readonly ConcurrentQueue<long> _latencies = new();
    private int _currentLimit = 5; // Starting limit
    private readonly object _lock = new();

    public void RecordLatency(long ms)
    {
        _latencies.Enqueue(ms);
        while (_latencies.Count > 20) _latencies.TryDequeue(out _); // Keep last 20 samples
    }

    public double GetAverageLatency()
    {
        if (_latencies.IsEmpty) return 0;
        return _latencies.Average();
    }

    public int GetCurrentLimit() => _currentLimit;

    public void UpdateLimit(int newLimit)
    {
        lock (_lock)
        {
            _currentLimit = newLimit;
        }
    }
}

public class AdaptiveLimitAdjuster : BackgroundService
{
    private readonly InferenceMetrics _metrics;
    private readonly ILogger<AdaptiveLimitAdjuster> _logger;

    public AdaptiveLimitAdjuster(InferenceMetrics metrics, ILogger<AdaptiveLimitAdjuster> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Check every 10s

            var avgLatency = _metrics.GetAverageLatency();
            var currentLimit = _metrics.GetCurrentLimit();

            if (avgLatency > 12000 && currentLimit > 3)
            {
                // Latency too high, reduce concurrency
                _metrics.UpdateLimit(currentLimit - 1);
                _logger.LogWarning($"High latency detected ({avgLatency}ms). Reducing concurrency to {currentLimit - 1}.");
            }
            else if (avgLatency < 8000 && currentLimit < 5)
            {
                // Latency low, increase concurrency
                _metrics.UpdateLimit(currentLimit + 1);
                _logger.LogInformation($"Low latency detected ({avgLatency}ms). Increasing concurrency to {currentLimit + 1}.");
            }
        }
    }
}
