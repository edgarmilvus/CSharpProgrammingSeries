
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

// File: Program.cs (Partial - Batching & Metrics Logic)
using System.Threading.Channels;
using Prometheus; // Add NuGet package: prometheus-net

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();

// 1. Setup Prometheus Metrics
var batchQueueDepth = Metrics.CreateGauge("batch_queue_depth", "Current number of items waiting in batch");
var inferenceCounter = Metrics.CreateCounter("http_requests_total", "Total HTTP requests");

// 2. Dynamic Batching with Channels
// Bounded channel creates backpressure if queue fills up
var channel = Channel.CreateBounded<InferenceRequest>(new BoundedChannelOptions(100) {
    FullMode = BoundedChannelFullMode.Wait
});

// Background service to process the batch
builder.Services.AddHostedService<BatchProcessorService>();

var app = builder.Build();

// Metrics Endpoint for Prometheus
app.MapMetrics(); // Exposes /metrics

app.MapPost("/analyze", async (InferenceRequest request) =>
{
    inferenceCounter.Inc();
    
    // Write to channel
    await channel.Writer.WriteAsync(request);
    
    // In a real scenario, we might wait for a result channel here
    return Results.Accepted(null, new { Status = "Queued" });
});

app.Run();

public record InferenceRequest(string Text);

public class BatchProcessorService : BackgroundService
{
    private readonly Channel<InferenceRequest> _channel;
    private readonly ILogger<BatchProcessorService> _logger;
    // Configuration constants
    private const int MaxBatchSize = 8;
    private const int FlushIntervalMs = 50;

    public BatchProcessorService(Channel<InferenceRequest> channel, ILogger<BatchProcessorService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<InferenceRequest>(MaxBatchSize);
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(FlushIntervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Try to drain available items up to MaxBatchSize
            while (batch.Count < MaxBatchSize && _channel.Reader.TryRead(out var item))
            {
                batch.Add(item);
            }

            if (batch.Count > 0)
            {
                // Update Metric
                Metrics.DefaultRegistry.GetGauge("batch_queue_depth")?.Set(batch.Count);

                _logger.LogInformation("Processing batch of {Count} items", batch.Count);
                
                // Simulate Inference Work
                await Task.Delay(20); 
                
                batch.Clear();
            }
        }
    }
}
