
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

# Source File: solution_exercise_6.cs
# Description: Solution for Exercise 6
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class ModelPool
{
    private readonly SemaphoreSlim _semaphore;

    public ModelPool(int maxInstances)
    {
        _semaphore = new SemaphoreSlim(maxInstances);
    }

    public async Task<string> RequestInferenceAsync(string prompt, CancellationToken ct)
    {
        // Log queueing
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Request '{prompt}' queued. Available slots: {_semaphore.CurrentCount}");

        // Acquire semaphore (async wait if pool is full)
        await _semaphore.WaitAsync(ct);

        try
        {
            // Critical Section: Model is being used
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Request '{prompt}' STARTED.");
            var sw = Stopwatch.StartNew();

            // Simulate inference (CPU bound work)
            await Task.Delay(new Random().Next(1000, 2000), ct);

            sw.Stop();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Request '{prompt}' FINISHED in {sw.ElapsedMilliseconds}ms.");
            
            return $"Result for {prompt}";
        }
        finally
        {
            // Always release the semaphore
            _semaphore.Release();
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Pool size of 2 (simulating 2 model slots)
        var pool = new ModelPool(2);
        var cts = new CancellationTokenSource();
        var tasks = new List<Task<string>>();

        Console.WriteLine("Submitting 5 concurrent requests to a pool of size 2...");

        // Launch 5 concurrent tasks
        for (int i = 1; i <= 5; i++)
        {
            int id = i;
            tasks.Add(pool.RequestInferenceAsync($"Req_{id}", cts.Token));
        }

        // Wait for all to complete
        var results = await Task.WhenAll(tasks);

        Console.WriteLine("\nAll requests completed.");
        foreach (var res in results)
        {
            Console.WriteLine($"- {res}");
        }
    }
}
