
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AiSummarizer
{
    // Entry point of the application
    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- Starting Multi-Model AI Summarization ---");

        // List of mock AI models to query in parallel
        var modelNames = new List<string> { "GPT-4-Turbo", "Claude-3-Opus", "Gemini-1.5-Pro" };

        try
        {
            // 1. Kick off all tasks concurrently. 
            //    We do NOT await them individually here, which would serialize the execution.
            //    Instead, we store the tasks in a collection to await them together.
            var summaryTasks = modelNames.Select(model => GetSummaryFromModelAsync(model));

            // 2. Await the completion of ALL tasks.
            //    Even if one fails, this method waits for ALL tasks to reach a terminal state (completed or faulted).
            //    If any task throws an exception, Task.WhenAll throws an AggregateException.
            var summaries = await Task.WhenAll(summaryTasks);

            // 3. Process successful results
            Console.WriteLine("\n--- Received Summaries ---");
            foreach (var summary in summaries)
            {
                Console.WriteLine($"[Success]: {summary}");
            }
        }
        catch (AggregateException ae)
        {
            // 4. Handle the batch failure
            Console.WriteLine("\n--- One or more AI models failed ---");
            
            // CRITICAL: Flatten() is essential here. 
            // When tasks are nested or run in parallel, exceptions can be wrapped inside other AggregateExceptions.
            // Flatten() creates a linear list of all underlying exceptions.
            foreach (var ex in ae.Flatten().InnerExceptions)
            {
                Console.WriteLine($"[Error Type]: {ex.GetType().Name}");
                Console.WriteLine($"[Message]: {ex.Message}");
                
                // Specific handling based on exception type (Polymorphic handling)
                if (ex is TimeoutException)
                {
                    Console.WriteLine("-> Action: Retry with backoff or switch to fallback model.");
                }
                else if (ex is HttpRequestException)
                {
                    Console.WriteLine("-> Action: Check network connectivity.");
                }
                else
                {
                    Console.WriteLine("-> Action: Log to monitoring system.");
                }
                Console.WriteLine(); // Spacer for readability
            }
        }
    }

    /// <summary>
    /// Simulates an API call to an AI model.
    /// Randomly succeeds or fails to demonstrate exception handling.
    /// </summary>
    private static async Task<string> GetSummaryFromModelAsync(string modelName)
    {
        Console.WriteLine($"[Requesting]: {modelName}...");

        // Simulate network latency
        await Task.Delay(new Random().Next(100, 500));

        // Simulate different failure modes based on the model name
        return modelName switch
        {
            "GPT-4-Turbo" => await SimulateSuccess(modelName),
            "Claude-3-Opus" => await SimulateTimeout(modelName),
            "Gemini-1.5-Pro" => await SimulateRateLimit(modelName),
            _ => throw new InvalidOperationException("Unknown model")
        };
    }

    // --- Simulation Helpers ---

    private static async Task<string> SimulateSuccess(string model)
    {
        // Simulate async work
        await Task.Delay(100); 
        return $"[{model}] Summary: The quick brown fox jumps over the lazy dog.";
    }

    private static async Task<string> SimulateTimeout(string model)
    {
        await Task.Delay(50); // Fail fast
        throw new TimeoutException($"The request to {model} timed out after 30s.");
    }

    private static async Task<string> SimulateRateLimit(string model)
    {
        await Task.Delay(50);
        throw new HttpRequestException($"429 Too Many Requests: Rate limit exceeded for {model}.");
    }
}
