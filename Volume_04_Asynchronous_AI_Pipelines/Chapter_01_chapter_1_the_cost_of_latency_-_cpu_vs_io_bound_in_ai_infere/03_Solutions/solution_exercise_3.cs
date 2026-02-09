
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace CPUParallelism
{
    class Program
    {
        static async Task<string> HeavyCpuPreprocessing(string input)
        {
            // Simulate CPU work.
            // NOTE: In a real application, CPU-bound work should ideally be 
            // offloaded via Task.Run to avoid blocking the async state machine.
            // However, for this simulation, we keep it simple.
            await Task.Delay(100); 
            return input.ToUpper();
        }

        static async Task Main(string[] args)
        {
            // Generate a larger dataset to see the difference
            var inputs = new List<string>();
            for (int i = 0; i < 100; i++) inputs.Add($"input_{i}");

            // --- 1. Sequential Processing ---
            var sw = Stopwatch.StartNew();
            var sequentialResults = new List<string>();
            foreach (var item in inputs)
            {
                sequentialResults.Add(await HeavyCpuPreprocessing(item));
            }
            sw.Stop();
            Console.WriteLine($"Sequential time: {sw.ElapsedMilliseconds} ms");

            // --- 2. Parallel Processing (Task.WhenAll) ---
            sw.Restart();
            
            // Start all tasks immediately
            var tasks = inputs.Select(item => HeavyCpuPreprocessing(item)).ToList();
            var parallelResults = await Task.WhenAll(tasks);
            
            sw.Stop();
            Console.WriteLine($"Parallel (Task.WhenAll) time: {sw.ElapsedMilliseconds} ms");

            // --- 3. Parallel Processing with Throttling (Large Dataset) ---
            Console.WriteLine("\nTesting with 1000 inputs and throttling...");
            var largeInputs = new List<string>();
            for (int i = 0; i < 1000; i++) largeInputs.Add($"large_input_{i}");

            sw.Restart();
            var throttledResults = await ProcessWithThrottling(largeInputs, maxConcurrency: 10);
            sw.Stop();
            Console.WriteLine($"Throttled Parallel time (Max 10 concurrent): {sw.ElapsedMilliseconds} ms");
        }

        static async Task<List<string>> ProcessWithThrottling(List<string> inputs, int maxConcurrency)
        {
            var results = new List<string>();
            // SemaphoreSlim is used to limit the number of concurrent asynchronous operations.
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = inputs.Select(async item =>
            {
                // Wait to enter the semaphore (blocks if max concurrency reached)
                await semaphore.WaitAsync();
                try
                {
                    return await HeavyCpuPreprocessing(item);
                }
                finally
                {
                    // Always release the semaphore
                    semaphore.Release();
                }
            });

            var resolvedResults = await Task.WhenAll(tasks);
            return resolvedResults.ToList();
        }
    }
}
