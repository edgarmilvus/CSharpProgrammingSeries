
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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Register the Hosted Service for monitoring
builder.Services.AddHostedService<LatencyMonitorService>();
// Register the sliding window as a singleton to share state between the service and the API
builder.Services.AddSingleton<SlidingWindow>();

var app = builder.Build();

// Minimal API endpoint for Prometheus scraping
app.MapGet("/metrics", ([FromServices] SlidingWindow window) =>
{
    var avgLatency = window.GetAverageLatency();
    
    // Prometheus text format: # TYPE, # HELP, and the metric value
    var sb = new StringBuilder();
    sb.AppendLine("# HELP ai_agent_inference_latency_seconds Average inference latency in seconds.");
    sb.AppendLine("# TYPE ai_agent_inference_latency_seconds gauge");
    sb.AppendLine($"ai_agent_inference_latency_seconds {avgLatency.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}");
    
    return Results.Text(sb.ToString(), "text/plain");
});

app.Run();

// ---------------------------------------------------------
// Core Logic: Thread-Safe Sliding Window
// ---------------------------------------------------------
public class SlidingWindow
{
    private readonly ConcurrentQueue<TimeSpan> _latencies = new();
    private readonly int _windowSize = 60; // Store last 60 seconds
    private long _count = 0;
    private double _sumSeconds = 0;

    public void AddLatency(TimeSpan latency)
    {
        double seconds = latency.TotalSeconds;
        
        // Add new value
        _latencies.Enqueue(latency);
        Interlocked.Increment(ref _count);
        Interlocked.Add(ref _sumSeconds, seconds);

        // Remove old values if we exceed the window size
        // Note: In a high-throughput scenario, we might optimize this to remove in batches
        // rather than checking every single add, but for < 1000 ops/sec this is fine.
        while (_latencies.Count > _windowSize)
        {
            if (_latencies.TryDequeue(out var oldLatency))
            {
                Interlocked.Decrement(ref _count);
                Interlocked.Add(ref _sumSeconds, -oldLatency.TotalSeconds);
            }
        }
    }

    public double GetAverageLatency()
    {
        long currentCount = Interlocked.Read(ref _count);
        if (currentCount == 0) return 0.0;

        // Read the volatile sum
        double currentSum = Interlocked.Read(ref _sumSeconds);
        return currentSum / currentCount;
    }
}

// ---------------------------------------------------------
// Hosted Service: Simulates receiving inference results
// ---------------------------------------------------------
public class LatencyMonitorService : BackgroundService
{
    private readonly SlidingWindow _window;
    private readonly Random _random = new();
    private readonly ILogger<LatencyMonitorService> _logger;

    public LatencyMonitorService(SlidingWindow window, ILogger<LatencyMonitorService> logger)
    {
        _window = window;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Latency Monitor Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Simulate processing a request
            var sw = Stopwatch.StartNew();
            
            // Simulate variable latency (e.g., 100ms to 800ms)
            var simulatedProcessingTime = _random.Next(100, 800);
            await Task.Delay(simulatedProcessingTime, stoppingToken);
            
            sw.Stop();

            // Record the latency
            _window.AddLatency(sw.Elapsed);
        }
    }
}
