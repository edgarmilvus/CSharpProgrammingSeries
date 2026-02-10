
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ---------------------------------------------------------
// 1. Domain Models
// ---------------------------------------------------------
public record InferenceRequest(Guid Id, string InputData);
public record InferenceResult(Guid Id, string OutputData, long ProcessingTimeMs);

// ---------------------------------------------------------
// 2. Stateful Orchestrator (The "Brain")
// ---------------------------------------------------------
public class InferenceOrchestrator
{
    private readonly ILogger<InferenceOrchestrator> _logger;
    // Thread-safe collection to act as a dynamic batch buffer
    private readonly ConcurrentQueue<InferenceRequest> _batchQueue = new();
    private readonly Timer _batchProcessorTimer;
    private const int BatchSize = 4; // Optimize for GPU parallelism
    private const int MaxWaitTimeMs = 50; // Max latency budget for batching

    public InferenceOrchestrator(ILogger<InferenceOrchestrator> logger)
    {
        _logger = logger;
        // Start the background batching loop
        _batchProcessorTimer = new Timer(ProcessBatchAsync, null, 0, MaxWaitTimeMs);
    }

    public Task<InferenceResult> ProcessRequestAsync(InferenceRequest request)
    {
        var tcs = new TaskCompletionSource<InferenceResult>();
        // Attach the TCS to the request so the processor can find it later
        request.CompletionSource = tcs;
        _batchQueue.Enqueue(request);
        return tcs.Task;
    }

    private void ProcessBatchAsync(object? state)
    {
        if (_batchQueue.IsEmpty) return;

        var batch = new List<InferenceRequest>();
        while (batch.Count < BatchSize && _batchQueue.TryDequeue(out var req))
        {
            batch.Add(req);
        }

        if (batch.Count > 0)
        {
            // Offload to the stateless inference worker
            _ = Task.Run(() => ExecuteInferenceBatch(batch));
        }
    }

    private async Task ExecuteInferenceBatch(List<InferenceRequest> batch)
    {
        _logger.LogInformation("Processing batch of {Count} requests", batch.Count);
        
        // SIMULATION: In a real scenario, this sends data to a GPU-backed container
        // or a sidecar process (e.g., via gRPC or shared memory).
        await Task.Delay(20); // Simulate GPU compute latency

        var stopwatch = Stopwatch.StartNew();
        
        foreach (var req in batch)
        {
            // Simulate AI Inference Logic
            var result = new InferenceResult(
                req.Id, 
                $"Processed: {req.InputData.ToUpper()}", 
                stopwatch.ElapsedMilliseconds
            );
            
            // Complete the waiting task
            req.CompletionSource?.SetResult(result);
        }
    }
}

// Extension to attach TCS to the record (requires a wrapper or property mutation)
// For this simple demo, we use a static dictionary to map IDs to completion sources
// to avoid modifying the immutable record directly.
public static class RequestTracker
{
    public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<InferenceResult>> PendingRequests = new();
}

// ---------------------------------------------------------
// 3. Stateless Inference Microservice (The "Muscle")
// ---------------------------------------------------------
public class InferenceService
{
    private readonly InferenceOrchestrator _orchestrator;

    public InferenceService(InferenceOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<IResult> HandleInference(HttpRequest request)
    {
        // 1. Deserialization
        var inputData = request.Query["data"].ToString();
        if (string.IsNullOrWhiteSpace(inputData)) 
            return Results.BadRequest("Missing 'data' query parameter.");

        var inferenceRequest = new InferenceRequest(Guid.NewGuid(), inputData);

        // 2. Orchestration Call
        // The service itself is stateless; it delegates stateful batching to the orchestrator.
        var result = await _orchestrator.ProcessRequestAsync(inferenceRequest);

        // 3. Response
        return Results.Ok(new 
        { 
            RequestId = result.Id, 
            Output = result.OutputData,
            LatencyMs = result.ProcessingTimeMs 
        });
    }
}

// ---------------------------------------------------------
// 4. Main Application Entry Point
// ---------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// Register the stateful orchestrator as a Singleton (lifecycle matches the app)
builder.Services.AddSingleton<InferenceOrchestrator>();
builder.Services.AddSingleton<InferenceService>();

var app = builder.Build();

// Map the endpoint
app.MapGet("/infer", (InferenceService service, HttpRequest request) => 
    service.HandleInference(request));

app.Run();

// ---------------------------------------------------------
// 5. Helper Class for the Immutable Record Workaround
// ---------------------------------------------------------
public static class InferenceRequestExtensions
{
    // Since records are immutable, we use a static tracker for the demo
    public static void RegisterCompletion(this InferenceRequest request, TaskCompletionSource<InferenceResult> tcs)
    {
        RequestTracker.PendingRequests.TryAdd(request.Id, tcs);
    }
}
