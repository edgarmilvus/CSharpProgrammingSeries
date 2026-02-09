
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
using System.Threading.Tasks;

public class LlmTokenProcessor
{
    // A mock database of cached token embeddings to simulate a real-world scenario.
    // In a real system, this might be a distributed cache like Redis.
    private static readonly Dictionary<string, float[]> _embeddingCache = new()
    {
        { "the", new float[] { 0.1f, 0.2f } },
        { "quick", new float[] { 0.3f, 0.4f } },
        { "brown", new float[] { 0.5f, 0.6f } },
        { "fox", new float[] { 0.7f, 0.8f } }
    };

    public static async Task Main()
    {
        Console.WriteLine("--- Starting LLM Token Processing Simulation ---");

        // Simulate a stream of tokens coming from an LLM response.
        // In a real scenario, this would be an async stream (IAsyncEnumerable<string>).
        var tokens = new[] { "the", "quick", "brown", "fox", "jumps", "over" };

        long initialMemory = GC.GetTotalMemory(true);
        
        // PROCESSING STRATEGY 1: Using Task (Standard Approach)
        Console.WriteLine("\n[Strategy 1] Using Task (Allocates on Heap):");
        await ProcessTokensWithTask(tokens);
        
        long memoryAfterTask = GC.GetTotalMemory(true);
        Console.WriteLine($"Memory used: {memoryAfterTask - initialMemory:N0} bytes");

        // Force GC to clean up for a clean comparison
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryBeforeValueTask = GC.GetTotalMemory(true);

        // PROCESSING STRATEGY 2: Using ValueTask (Optimized for Hot Loops)
        Console.WriteLine("\n[Strategy 2] Using ValueTask (Reduces Heap Allocations):");
        await ProcessTokensWithValueTask(tokens);

        long memoryAfterValueTask = GC.GetTotalMemory(true);
        Console.WriteLine($"Memory used: {memoryAfterValueTask - memoryBeforeValueTask:N0} bytes");
    }

    /// <summary>
    /// Standard approach using Task. This is safe and correct but allocates
    /// a new Task object on the heap for every operation, even if the result is synchronous.
    /// </summary>
    private static async Task ProcessTokensWithTask(IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            // We await a method that returns a Task.
            // Even if the result is ready immediately, the Task object is allocated on the heap.
            var embedding = await GetEmbeddingAsTaskAsync(token);
            
            // Simulate work (e.g., calculating similarity)
            if (embedding != null)
            {
                Console.Write(".");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Optimized approach using ValueTask. This avoids heap allocations
    /// when the result is available synchronously (e.g., from a cache).
    /// </summary>
    private static async Task ProcessTokensWithValueTask(IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            // We await a method that returns a ValueTask.
            // If the result is synchronous (cache hit), no heap allocation occurs.
            var embedding = await GetEmbeddingAsValueTaskAsync(token);
            
            // Simulate work
            if (embedding != null)
            {
                Console.Write(".");
            }
        }
        Console.WriteLine();
    }

    // --- Helper Methods ---

    /// <summary>
    /// Simulates fetching an embedding. Returns a Task, forcing a heap allocation
    /// even for cached results (unless manually optimized with Task.FromResult, 
    /// but the caller still awaits a Task).
    /// </summary>
    private static Task<float[]> GetEmbeddingAsTaskAsync(string token)
    {
        if (_embeddingCache.TryGetValue(token, out var cachedEmbedding))
        {
            // Even though we return a completed task, the Task object itself 
            // is typically allocated on the heap (unless using Task.CompletedTask, 
            // but that returns a Task, not a float[]).
            return Task.FromResult(cachedEmbedding);
        }

        // Simulate async I/O for unknown tokens
        return Task.Run(async () => 
        {
            await Task.Delay(10); // Simulate network latency
            return new float[] { 0.9f, 0.9f }; 
        });
    }

    /// <summary>
    /// Simulates fetching an embedding. Returns a ValueTask.
    /// If the result is ready immediately (cache hit), it returns a struct (stack-allocated).
    /// If async (cache miss), it wraps the result in a ValueTask.
    /// </summary>
    private static ValueTask<float[]> GetEmbeddingAsValueTaskAsync(string token)
    {
        if (_embeddingCache.TryGetValue(token, out var cachedEmbedding))
        {
            // CRITICAL: Returning a result directly creates a ValueTask wrapping the result.
            // This is a struct, so it is allocated on the stack (zero heap allocation).
            return new ValueTask<float[]>(cachedEmbedding);
        }

        // If the result requires async I/O, we convert it to a ValueTask.
        // Note: This path DOES allocate a Task internally, but the optimization
        // applies to the synchronous path (the cache hit).
        return new ValueTask<float[]>(Task.Run(async () => 
        {
            await Task.Delay(10);
            return new float[] { 0.9f, 0.9f };
        }));
    }
}
