
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
using System.Threading;
using System.Threading.Tasks;

// Simulating a real-world dataset: User interactions with an AI chatbot.
// Each record contains a user ID, a text prompt, and a timestamp.
public class ChatLog
{
    public int UserId { get; set; }
    public string Prompt { get; set; }
    public DateTime Timestamp { get; set; }
}

public class BasicPlinqExample
{
    public static void Run()
    {
        // 1. DATA GENERATION: Creating a large dataset to simulate "Big Data".
        // In a real scenario, this would be loaded from a database or file.
        // We generate 100,000 records to ensure parallelism provides a benefit.
        var rawData = Enumerable.Range(1, 100_000)
            .Select(i => new ChatLog
            {
                UserId = i % 100, // 100 distinct users
                Prompt = i % 5 == 0 ? "   " : $"User query number {i}", // 20% noise/empty data
                Timestamp = DateTime.Now.AddSeconds(-i)
            })
            .ToList(); // Immediate execution to materialize the list.

        Console.WriteLine($"Processing {rawData.Count} raw records...");

        // 2. FUNCTIONAL DATA PIPELINE: Cleaning and Normalizing.
        // We define the pipeline. Note: This is DEFERRED EXECUTION.
        // The code inside .Select/.Where does not run until we iterate (e.g., .ToList()).
        // We use AsParallel() to distribute this workload across CPU cores.
        var processedDataQuery = rawData
            .AsParallel() // <--- KEY: Enables Parallel LINQ (PLINQ).
            .Where(log => !string.IsNullOrWhiteSpace(log.Prompt)) // Filter noise.
            .Select(log => new
            {
                // Normalization: Convert to lowercase and trim.
                // Pure function: No side effects, returns a new anonymous object.
                NormalizedPrompt = log.Prompt.ToLowerInvariant().Trim(),
                log.UserId,
                // Feature Engineering: Calculate a "token length" proxy.
                TokenLength = log.Prompt.Length
            })
            .Where(processed => processed.TokenLength > 5) // Filter out short queries.
            .GroupBy(processed => processed.UserId); // Group by user for aggregation.

        // 3. IMMEDIATE EXECUTION: Materializing the results.
        // The pipeline executes here. PLINQ automatically partitions the data.
        var finalResults = processedDataQuery.ToList();

        // 4. OUTPUT: Processing the grouped results.
        Console.WriteLine($"\nPipeline complete. {finalResults.Count} user groups found.");
        foreach (var userGroup in finalResults.Take(5)) // Displaying first 5 for brevity.
        {
            Console.WriteLine($"User ID: {userGroup.Key} | Avg Token Length: {userGroup.Average(x => x.TokenLength):F2}");
        }
    }
}
