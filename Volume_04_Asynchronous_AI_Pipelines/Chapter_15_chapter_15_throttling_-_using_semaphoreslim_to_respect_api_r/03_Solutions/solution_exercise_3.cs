
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BatchProcessor
{
    private readonly int _maxConcurrency;
    private readonly Random _random = new Random();

    public BatchProcessor(int maxConcurrency)
    {
        _maxConcurrency = maxConcurrency;
    }

    public async Task<List<string>> ProcessBatchAsync(List<string> prompts)
    {
        var semaphore = new SemaphoreSlim(_maxConcurrency);
        var tasks = new List<Task<string>>();
        var results = new List<string>();

        // Sliding window logic:
        // We iterate through prompts, creating tasks.
        // To prevent memory overflow if 'prompts' is massive (e.g., millions),
        // we monitor the number of active tasks.
        
        foreach (var prompt in prompts)
        {
            // Wait for a semaphore slot before creating the task
            // This limits how many tasks are "in-flight" in the list
            await semaphore.WaitAsync();

            // Capture variables for the closure
            string p = prompt;
            var task = Task.Run(async () =>
            {
                try
                {
                    // Simulate API call with random jitter
                    await Task.Delay(_random.Next(100, 300));
                    return $"Processed: {p}";
                }
                finally
                {
                    // Release the semaphore slot
                    semaphore.Release();
                }
            });

            tasks.Add(task);

            // Memory Optimization: 
            // If we have accumulated a large number of tasks (e.g., 100),
            // wait for at least one to complete before adding more.
            // This keeps the memory footprint of the 'tasks' list manageable.
            if (tasks.Count >= 100)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                results.Add(await completedTask); // Await to unwrap result/exception
            }
        }

        // Await remaining tasks
        var remainingResults = await Task.WhenAll(tasks);
        results.AddRange(remainingResults);

        return results;
    }
}

public class Program
{
    public static async Task Main()
    {
        var processor = new BatchProcessor(maxConcurrency: 5);
        
        // Generate 50 prompts
        var prompts = Enumerable.Range(1, 50).Select(i => $"Prompt {i}").ToList();

        Console.WriteLine("Processing batch...");
        var stopwatch = Stopwatch.StartNew();

        var results = await processor.ProcessBatchAsync(prompts);

        stopwatch.Stop();
        Console.WriteLine($"Processed {results.Count} items in {stopwatch.ElapsedMilliseconds}ms");
    }
}
