
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

// 1. PROTOCOL DEFINITION (ImageClassifier.proto)
// syntax = "proto3";
// package grpcservice;
// service ImageClassifier {
//   rpc Classify(ClassifyRequest) returns (ClassifyResponse);
// }
// message ClassifyRequest { bytes imageData = 1; }
// message ClassifyResponse { string label = 1; }

using Grpc.Core;
using GrpcService; // Generated from .proto
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using System.Diagnostics;

// 2. SERVER IMPLEMENTATION
public class ImageClassifierService : ImageClassifier.ImageClassifierBase
{
    private readonly Channel<InferenceTask> _channel;
    private readonly ILogger<ImageClassifierService> _logger;

    public ImageClassifierService(ILogger<ImageClassifierService> logger)
    {
        // Unbounded channel to handle burst traffic
        _channel = Channel.CreateUnbounded<InferenceTask>();
        _logger = logger;
    }

    public override async Task<ClassifyResponse> Classify(ClassifyRequest request, ServerCallContext context)
    {
        var tcs = new TaskCompletionSource<string>();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        
        // Enqueue the request with a timeout token
        var inferenceTask = new InferenceTask(request.ImageData.ToByteArray(), tcs, cts.Token);
        
        // Write to channel (non-blocking)
        await _channel.Writer.WriteAsync(inferenceTask);

        try
        {
            // Wait for the batch processor to complete the inference
            // Timeout logic: If stuck in queue > 100ms, the processor handles it as singleton
            var label = await tcs.Task;
            return new ClassifyResponse { Label = label };
        }
        catch (OperationCanceledException)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Request timed out or cancelled"));
        }
    }

    // Helper class to track the request lifecycle
    public record InferenceTask(byte[] ImageData, TaskCompletionSource<string> CompletionSource, CancellationToken Token);
}

// 3. BATCH PROCESSOR
public class BatchProcessor : BackgroundService
{
    private readonly ImageClassifierService _service;
    private readonly ILogger<BatchProcessor> _logger;
    
    // Configuration
    private const int MaxBatchSize = 32;
    private const int MaxWaitTimeMs = 10;
    private const int RequestTimeoutMs = 100;

    public BatchProcessor(ImageClassifierService service, ILogger<BatchProcessor> logger)
    {
        _service = service;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Access the internal channel of the service
        var reader = _service._channel.Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = new List<ImageClassifierService.InferenceTask>();
            var sw = Stopwatch.StartNew();

            // 1. Wait for the first item
            if (!await reader.WaitToReadAsync(stoppingToken)) break;
            
            // 2. Fill the batch
            while (batch.Count < MaxBatchSize && sw.ElapsedMilliseconds < MaxWaitTimeMs)
            {
                if (reader.TryRead(out var item))
                {
                    // Check if the specific request has already timed out client-side or via cancellation
                    if (!item.Token.IsCancellationRequested)
                    {
                        batch.Add(item);
                    }
                    else
                    {
                        item.CompletionSource.TrySetCanceled();
                    }
                }
                else
                {
                    break;
                }
            }

            // 3. Process Batch (or Singleton if timeout occurred)
            if (batch.Count > 0)
            {
                _ = ProcessBatchAsync(batch, stoppingToken);
            }
        }
    }

    private async Task ProcessBatchAsync(List<ImageClassifierService.InferenceTask> batch, CancellationToken ct)
    {
        try
        {
            // Simulate GPU Inference
            // In a real scenario, we would stack inputs and run a single model.forward() call
            await Task.Delay(20, ct); // Simulated compute time

            foreach (var item in batch)
            {
                // Simulate result generation
                if (DateTime.UtcNow.Ticks % 10 == 0) // Random failure simulation
                {
                    item.CompletionSource.TrySetException(new Exception("Inference failed"));
                }
                else
                {
                    item.CompletionSource.TrySetResult($"Label_{item.ImageData.Length}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            foreach (var item in batch)
            {
                item.CompletionSource.TrySetException(ex);
            }
        }
    }
}
