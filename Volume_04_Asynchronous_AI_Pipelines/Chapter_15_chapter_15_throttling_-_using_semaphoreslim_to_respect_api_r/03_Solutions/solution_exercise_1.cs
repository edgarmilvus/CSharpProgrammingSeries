
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class LlmClient
{
    // SemaphoreSlim with an initial count of 5 and max count of 5.
    // This acts as our concurrency gatekeeper.
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5, 5);

    public async Task<string> GetCompletionAsync(string prompt)
    {
        // Asynchronously wait to enter the semaphore.
        // If the count is 0, this task will yield until a slot is available.
        await _semaphore.WaitAsync();

        try
        {
            // Simulate API latency (e.g., network call + processing)
            // Using a slightly randomized delay to mimic real-world variance
            var randomJitter = new Random().Next(50, 150);
            await Task.Delay(200 + randomJitter);

            return $"Completion for: '{prompt}' processed.";
        }
        finally
        {
            // Ensure the semaphore slot is released even if an exception occurs
            _semaphore.Release();
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        var client = new LlmClient();
        var tasks = new List<Task<string>>();
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Starting 20 concurrent requests at {DateTime.Now:HH:mm:ss.fff}");

        // Spawn 20 concurrent tasks
        for (int i = 0; i < 20; i++)
        {
            int index = i; // Capture variable for closure
            tasks.Add(Task.Run(async () => await client.GetCompletionAsync($"Prompt {index}")));
        }

        // Await all tasks to complete
        var results = await Task.WhenAll(tasks);
        
        stopwatch.Stop();

        Console.WriteLine($"All requests completed at {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds}ms");
        
        // Expected time: 
        // 20 requests / 5 concurrent = 4 batches.
        // Each batch takes ~250ms (simulated delay).
        // Total ~1000ms + overhead.
        Console.WriteLine("Throttling active: Total time should be significantly longer than a single request.");
    }
}
