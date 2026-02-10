
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public record InferenceRequest(string Id, string InputData);
public record InferenceResponse(string Result);

// Represents a request ready for batching
public record BatchedRequest(InferenceRequest Request, TaskCompletionSource<InferenceResponse> CompletionSource);

public class GpuResourceManager
{
    private readonly long _totalMemory;
    private long _allocatedMemory;
    // Serialize loading to prevent race conditions on memory allocation
    private readonly SemaphoreSlim _loadingLock = new SemaphoreSlim(1, 1);

    public GpuResourceManager(long totalMemoryBytes)
    {
        _totalMemory = totalMemoryBytes;
    }

    public async Task<bool> TryAllocateAsync(long modelSizeBytes)
    {
        await _loadingLock.WaitAsync();
        try
        {
            if (_allocatedMemory + modelSizeBytes <= _totalMemory)
            {
                _allocatedMemory += modelSizeBytes;
                return true;
            }
            return false;
        }
        finally
        {
            _loadingLock.Release();
        }
    }

    public void Release(long modelSizeBytes)
    {
        // No async needed for release, but locking is still good practice
        _loadingLock.Wait();
        try
        {
            _allocatedMemory -= modelSizeBytes;
            if (_allocatedMemory < 0) _allocatedMemory = 0;
        }
        finally
        {
            _loadingLock.Release();
        }
    }
}

public class OptimizedInferenceEngine
{
    private readonly Channel<BatchedRequest> _channel;
    private readonly GpuResourceManager _gpuManager;

    public OptimizedInferenceEngine(GpuResourceManager gpuManager)
    {
        _gpuManager = gpuManager;
        // Create an unbounded channel to handle high throughput
        _channel = Channel.CreateUnbounded<BatchedRequest>();
        
        // Start the batching loop immediately
        Task.Run(ProcessBatchesAsync);
    }

    // 1. Background loop for batching
    private async Task ProcessBatchesAsync()
    {
        var batchBuffer = new List<BatchedRequest>();

        while (await _channel.Reader.WaitToReadAsync())
        {
            // Try to read as many items as possible immediately
            while (_channel.Reader.TryRead(out var item))
            {
                batchBuffer.Add(item);
            }

            // If we have items, wait for the batching window (50ms) or buffer size limit
            if (batchBuffer.Count > 0)
            {
                await Task.Delay(50); // 50ms window

                // Read any remaining items that arrived during the delay
                while (_channel.Reader.TryRead(out var item))
                {
                    batchBuffer.Add(item);
                }

                // Execute Batch
                await ExecuteBatchAsync(batchBuffer);
                batchBuffer.Clear();
            }
        }
    }

    private async Task ExecuteBatchAsync(List<BatchedRequest> batch)
    {
        // In a real scenario, this would be a GPU kernel call
        // Simulate GPU processing time based on batch size
        await Task.Delay(10 + (batch.Count * 2)); 

        // Complete all tasks in the batch
        foreach (var req in batch)
        {
            // Simulate inference result
            var result = new InferenceResponse($"Processed: {req.Request.InputData}");
            req.CompletionSource.SetResult(result);
        }
    }

    public Task<InferenceResponse> PredictAsync(InferenceRequest request)
    {
        var tcs = new TaskCompletionSource<InferenceResponse>();
        var batchedReq = new BatchedRequest(request, tcs);
        
        // 2. Write to channel (Non-blocking)
        _channel.Writer.TryWrite(batchedReq);
        
        return tcs.Task;
    }
}
