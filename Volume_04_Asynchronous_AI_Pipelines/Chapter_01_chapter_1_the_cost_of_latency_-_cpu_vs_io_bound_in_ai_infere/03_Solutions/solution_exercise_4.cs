
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AdvancedInferenceService
{
    // Throttling mechanism to limit concurrent access to external resources (GPU/Network)
    private readonly SemaphoreSlim _throttler;

    public AdvancedInferenceService(int maxConcurrentRequests)
    {
        _throttler = new SemaphoreSlim(maxConcurrentRequests);
    }

    // Refactored method: Async and Concurrent
    public async Task<List<string>> ProcessBatchAsync(List<string> inputs)
    {
        // Create a list of tasks to process all inputs concurrently
        var processingTasks = inputs.Select(input => ProcessSingleRequestAsync(input));
        
        // Await all tasks to complete
        var results = await Task.WhenAll(processingTasks);
        
        return results.ToList();
    }

    private async Task<string> ProcessSingleRequestAsync(string input)
    {
        // 1. Simulate GPU/Network Check (I/O) with Throttling
        await _throttler.WaitAsync();
        try
        {
            await Task.Delay(1000); // Asynchronous wait
        }
        finally
        {
            _throttler.Release();
        }

        // 2. Simulate Tokenization (CPU)
        // Note: For pure CPU work, it's often better to use Task.Run to avoid blocking 
        // the async state machine if this runs on a UI thread or ASP.NET context.
        // However, for console benchmarks, await is acceptable if the delay simulates work.
        await Task.Delay(500); 

        // 3. Simulate DB Save (I/O)
        await Task.Delay(500);

        return $"Processed_{input}";
    }

    // Legacy synchronous method for comparison
    public List<string> ProcessBatchSync(List<string> inputs)
    {
        var results = new List<string>();
        foreach (var input in inputs)
        {
            Thread.Sleep(1000); // I/O
            Thread.Sleep(500);  // CPU
            Thread.Sleep(500);  // I/O
            results.Add($"Processed_{input}");
        }
        return results;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var inputs = new List<string> { "A", "B", "C", "D", "E" };
        var service = new AdvancedInferenceService(maxConcurrentRequests: 3);

        // 1. Benchmark Legacy Synchronous Method
        Console.WriteLine("--- Running Synchronous Version ---");
        var sw = Stopwatch.StartNew();
        service.ProcessBatchSync(inputs);
        sw.Stop();
        Console.WriteLine($"Sync Time: {sw.Elapsed.TotalSeconds:F2}s (Sequential)");

        // 2. Benchmark Refactored Asynchronous Method
        Console.WriteLine("\n--- Running Asynchronous Concurrent Version ---");
        sw.Restart();
        await service.ProcessBatchAsync(inputs);
        sw.Stop();
        Console.WriteLine($"Async Time: {sw.Elapsed.TotalSeconds:F2}s (Concurrent with Throttling)");

        // 3. Edge Case: Massive Batch
        Console.WriteLine("\n--- Running Massive Batch (100 items) ---");
        var massiveBatch = Enumerable.Range(0, 100).Select(i => $"Item_{i}").ToList();
        sw.Restart();
        await service.ProcessBatchAsync(massiveBatch);
        sw.Stop();
        Console.WriteLine($"Massive Batch Time: {sw.Elapsed.TotalSeconds:F2}s");
    }
}
