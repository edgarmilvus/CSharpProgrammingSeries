
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ScatterGatherDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("--- Starting Scatter-Gather Demo ---");

            // Define the data sources we want to query
            var queries = new Dictionary<string, string>
            {
                { "News Summary", "Latest headlines for TechCorp" },
                { "Stock Analysis", "Trend analysis for Ticker: TC" },
                { "Regulatory Filings", "Recent filings for CIK: 000123456" }
            };

            // 1. SCATTER: Initiate all tasks concurrently without awaiting them immediately.
            // We store the "hot" tasks in a collection.
            var processingTasks = queries.Select(kvp => 
                FetchModelDataAsync(kvp.Key, kvp.Value)
            ).ToList();

            // 2. GATHER: Wait for ALL tasks to complete. 
            // If one fails, Task.WhenAll will throw an AggregateException (wrapped in TaskCanceledException in .NET).
            // We use WhenAll to ensure we have all results (or know exactly which ones failed) before proceeding.
            var results = await Task.WhenAll(processingTasks);

            Console.WriteLine("\n--- All Models Completed ---");
            
            // 3. PROCESS: Iterate over the consolidated results.
            foreach (var result in results)
            {
                Console.WriteLine($"[{result.Source}]: {result.Summary}");
            }

            Console.WriteLine("\n--- Demo Complete ---");
        }

        /// <summary>
        /// Simulates calling an external AI model or API endpoint.
        /// </summary>
        /// <param name="modelName">The name of the model/service.</param>
        /// <param name="input">The query/prompt.</param>
        /// <returns>A tuple containing the source and the generated response.</returns>
        private static async Task<ModelResponse> FetchModelDataAsync(string modelName, string input)
        {
            // Simulate network latency (random between 1 to 3 seconds)
            var randomDelay = new Random().Next(1000, 3000);
            
            Console.WriteLine($"[Request] Sent to {modelName} (Delay: {randomDelay}ms)...");

            // Simulate the asynchronous I/O operation
            await Task.Delay(randomDelay);

            // Simulate a random failure for demonstration purposes (10% chance)
            if (new Random().Next(0, 10) == 0)
            {
                Console.WriteLine($"[Error] {modelName} failed to respond.");
                throw new HttpRequestException($"Connection timeout to {modelName}");
            }

            // Simulate a successful response
            Console.WriteLine($"[Success] Received from {modelName}.");
            return new ModelResponse
            {
                Source = modelName,
                Summary = $"Processed input: '{input}' (Latency: {randomDelay}ms)"
            };
        }
    }

    // Simple DTO for the response
    public class ModelResponse
    {
        public string Source { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
