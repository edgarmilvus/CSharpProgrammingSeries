
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
using System.Threading.Tasks;

public class BatchInferenceEngine
{
    private readonly Random _rng = new();

    public async ValueTask<int> InferAsync(string prompt)
    {
        await Task.Delay(_rng.Next(10, 50));
        return prompt.Length;
    }

    // Approach 1: Parallel.ForEachAsync
    public async Task ProcessWithParallelForEachAsync(IEnumerable<string> prompts)
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };

        await Parallel.ForEachAsync(prompts, options, async (prompt, token) =>
        {
            int result = await InferAsync(prompt);
            // Console.WriteLine($"Processed: {prompt} -> {result}");
        });
    }

    // Approach 2: Task.WhenAll with ValueTask
    public async Task ProcessWithWhenAllAsync(IEnumerable<string> prompts)
    {
        // 1. Create the collection of ValueTasks.
        // IMPORTANT: We must not await these individually yet.
        var tasks = prompts.Select(p => InferAsync(p)).ToList();

        // 2. Await them all.
        // Note: Task.WhenAll expects Task, so ValueTasks are implicitly converted (boxed).
        // This creates some overhead, but allows concurrency.
        await Task.WhenAll(tasks);

        // 3. Aggregating results safely.
        // Since tasks is a List<ValueTask<int>>, we can iterate and await now that WhenAll has completed.
        // All tasks are completed, so awaiting is synchronous (fast).
        var results = new List<int>();
        foreach (var vt in tasks)
        {
            results.Add(await vt);
        }
    }

    // Approach 2b: Optimized Aggregation (Avoiding double awaiting overhead)
    public async Task<int[]> ProcessWithWhenAllOptimizedAsync(IEnumerable<string> prompts)
    {
        // Convert to array to avoid multiple enumerations
        var promptsArray = prompts.ToArray();
        
        // Create a matching array for results to avoid boxing/casting if we tracked tasks differently,
        // but standard Task.WhenAll requires Tasks.
        
        var tasks = promptsArray.Select(p => InferAsync(p).AsTask()).ToArray();

        await Task.WhenAll(tasks);

        // Extract results from the completed tasks
        return tasks.Select(t => t.Result).ToArray();
    }
}

