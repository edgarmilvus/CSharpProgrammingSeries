
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DocumentProcessor
{
    // Simulates a CPU-intensive task
    public string PreprocessDocument(string document)
    {
        // Simulate CPU work
        long sum = 0;
        for (int i = 0; i < 10_000_000; i++)
        {
            sum += i;
        }
        
        // Return processed string with Thread ID for observation
        return $"Processed '{document}' on Thread ID: {Thread.CurrentThread.ManagedThreadId}. Sum: {sum}";
    }

    // Process documents in parallel with a specific concurrency limit
    public async Task<List<string>> ProcessInParallel(List<string> documents, int maxDegreeOfParallelism)
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        var results = new List<string>(documents.Count);
        // Initialize the list with placeholders to ensure thread safety during concurrent writes
        for (int i = 0; i < documents.Count; i++) results.Add(string.Empty);

        await Parallel.ForEachAsync(
            Enumerable.Range(0, documents.Count), 
            options, 
            async (index, cancellationToken) =>
            {
                // Perform the CPU-bound work
                var processed = PreprocessDocument(documents[index]);
                // Note: In a real scenario, we might use a concurrent collection or lock.
                // Here we rely on the fact that we are writing to distinct indices.
                results[index] = processed;
            });

        return results;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var processor = new DocumentProcessor();
        // Generate 20 dummy documents
        var documents = Enumerable.Range(1, 20).Select(i => $"Doc-{i}").ToList();

        Console.WriteLine("--- Sequential Processing ---");
        var stopwatch = Stopwatch.StartNew();
        var seqResults = new List<string>();
        foreach (var doc in documents)
        {
            seqResults.Add(processor.PreprocessDocument(doc));
        }
        stopwatch.Stop();
        Console.WriteLine($"Sequential Time: {stopwatch.ElapsedMilliseconds}ms\n");

        Console.WriteLine("--- Parallel Processing (MaxDegree = 4) ---");
        stopwatch.Restart();
        var parallelResults = await processor.ProcessInParallel(documents, 4);
        stopwatch.Stop();
        Console.WriteLine($"Parallel Time: {stopwatch.ElapsedMilliseconds}ms");
        
        // Display a few results to show thread usage
        Console.WriteLine("\nSample Parallel Results:");
        parallelResults.Take(5).ToList().ForEach(Console.WriteLine);
    }
}
