
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System.Collections.Concurrent;
using System.Threading.Tasks;

// Define the interface and record here for context
public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text);
}

public record TextEmbedding(string Text, float[] Vector);

public class EmbeddingProcessor
{
    public async Task<ConcurrentBag<TextEmbedding>> GenerateEmbeddingsAsync(
        List<string> documents, 
        IEmbeddingService embeddingService)
    {
        var results = new ConcurrentBag<TextEmbedding>();
        
        // Configure parallel options to limit concurrency
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8
        };

        // Use Parallel.ForEachAsync to process documents concurrently
        await Parallel.ForEachAsync(documents, parallelOptions, async (document, cancellationToken) =>
        {
            // Asynchronously get the embedding for the current document
            var vector = await embeddingService.GetEmbeddingAsync(document);
            
            // Add the result to the thread-safe collection
            results.Add(new TextEmbedding(document, vector));
        });

        return results;
    }
}

// Mock implementation for testing purposes
public class MockEmbeddingService : IEmbeddingService
{
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // Simulate network delay (100ms)
        await Task.Delay(100);
        
        // Return a dummy vector (e.g., 128 dimensions)
        return new float[128]; 
    }
}
