
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class StreamingProcessor
{
    // 2. Helper method with retry logic
    public async Task<string> ProcessChunkWithRetryAsync(Func<Task<string>> chunkFactory, int maxRetries)
    {
        int attempt = 0;
        while (attempt <= maxRetries)
        {
            try
            {
                return await chunkFactory();
            }
            catch (HttpRequestException) // 4. Catch specific network error
            {
                attempt++;
                if (attempt > maxRetries)
                {
                    // 4. Return placeholder if retries exhausted
                    return "[Failed Chunk]";
                }
                // 3. Delay before retry
                await Task.Delay(100 * attempt); 
            }
        }
        return "[Failed Chunk]";
    }

    public async Task<string> ProcessStreamAsync()
    {
        // 1. Simulate 5 tasks, 2 fail
        var chunkFactories = new List<Func<Task<string>>>
        {
            () => Task.FromResult("Chunk1 Data"),
            () => Task.FromException<string>(new HttpRequestException("Network error 1")),
            () => Task.FromResult("Chunk3 Data"),
            () => Task.FromException<string>(new HttpRequestException("Network error 2")),
            () => Task.FromResult("Chunk5 Data")
        };

        // 5. Aggregate results
        var processingTasks = chunkFactories
            .Select(factory => ProcessChunkWithRetryAsync(factory, 3))
            .ToList();

        // 6. Handle AggregateException by inspecting task status
        // Note: Since ProcessChunkWithRetryAsync catches exceptions internally and returns strings,
        // Task.WhenAll will not throw here. However, the requirement asks to handle the scenario 
        // where Task.WhenAll might throw (e.g., if we didn't catch inside the helper).
        // To strictly follow the prompt's intent of "handling AggregateException by inspecting status":
        try 
        {
            await Task.WhenAll(processingTasks);
        }
        catch (AggregateException ae)
        {
            // This block would only be hit if ProcessChunkWithRetryAsync re-threw exceptions.
            // Included to demonstrate the pattern requested.
            Console.WriteLine("Unexpected failures detected in WhenAll:");
            foreach (var ex in ae.Flatten().InnerExceptions)
            {
                Console.WriteLine($" - {ex.Message}");
            }
        }

        // 5. Aggregate results safely
        var results = await Task.WhenAll(processingTasks);
        return string.Join(" | ", results);
    }
}

public class Program
{
    public static async Task Main()
    {
        var processor = new StreamingProcessor();
        string finalStream = await processor.ProcessStreamAsync();
        
        Console.WriteLine($"Final Aggregated Stream: {finalStream}");
        // 7. Note: The pipeline continues despite the 2 initial failures.
    }
}
