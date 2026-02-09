
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EmbeddingBatchProcessor
{
    // 1. Define a simple record to hold our data and results
    public record TextChunk(string Id, string Content);

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Batch Embedding Generation...");

        // 2. Generate dummy data (simulating 100 document chunks)
        var chunks = Enumerable.Range(1, 100)
                               .Select(i => new TextChunk($"ID-{i:D3}", $"Content for document {i}"))
                               .ToList();

        // 3. Thread-safe collection to store results (ConcurrentBag is ideal for unordered inserts)
        var embeddings = new ConcurrentBag<(string Id, float[] Vector)>();

        // 4. Configure Parallel Options
        // MaxDegreeOfParallelism limits concurrent tasks to avoid overwhelming the API or local resources.
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5 
        };

        // 5. Define the cancellation token source
        var cts = new CancellationTokenSource();

        try
        {
            // 6. Execute the parallel loop
            // 'async' keyword is required here to allow await inside the loop body
            await Parallel.ForEachAsync(chunks, parallelOptions, async (chunk, cancellationToken) =>
            {
                // 7. Simulate generating an embedding (e.g., calling an external API)
                // We pass the cancellation token to support cooperative cancellation.
                var vector = await GenerateEmbeddingAsync(chunk, cancellationToken);

                // 8. Add the result to the thread-safe collection
                embeddings.Add((chunk.Id, vector));

                // 9. Report progress (Thread-safe writing to Console)
                Console.WriteLine($"Processed {chunk.Id} on Thread {Thread.CurrentThread.ManagedThreadId}");
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Batch processing was cancelled.");
        }

        // 10. Output results
        Console.WriteLine($"\nCompleted. Generated {embeddings.Count} embeddings.");
        foreach (var emb in embeddings.Take(3)) // Show first 3
        {
            Console.WriteLine($"ID: {emb.Id}, Vector: [{string.Join(", ", emb.Vector.Take(3))}...]");
        }
    }

    /// <summary>
    /// Simulates an asynchronous call to an AI Embedding API.
    /// </summary>
    private static async Task<float[]> GenerateEmbeddingAsync(TextChunk chunk, CancellationToken ct)
    {
        // Simulate network latency (random between 100ms and 300ms)
        var delay = Random.Shared.Next(100, 300);
        
        // Simulate an API rate limit check or processing time
        await Task.Delay(delay, ct);

        // Simulate a random vector of size 384 (common for small embedding models)
        var vector = new float[384];
        Random.Shared.NextBytes(vector); // Filling with random bytes (simplified for demo)

        // Convert bytes to floats in range [0, 1]
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = vector[i] / 255.0f;
        }

        return vector;
    }
}
