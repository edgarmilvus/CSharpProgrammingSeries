
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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// 1. Interface and Mock Implementation
public interface IEmbeddingGenerator
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}

public class MockEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly Random _random = new();
    private readonly HttpClient _httpClient; // Simulated network dependency

    public MockEmbeddingGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        // Simulate network latency
        await Task.Delay(_random.Next(100, 500), ct);

        // Simulate transient failure (10% chance)
        if (_random.Next(100) < 10)
        {
            throw new HttpRequestException("Transient network error");
        }

        // Return a dummy vector of 1536 dimensions (common for OpenAI)
        return Enumerable.Repeat(0.5f, 1536).ToArray();
    }
}

// 2. Service Implementation
public class EmbeddingService
{
    private readonly IEmbeddingGenerator _generator;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _batchSize;

    public EmbeddingService(IEmbeddingGenerator generator, ILogger<EmbeddingService> logger, int batchSize = 10, int maxConcurrency = 5)
    {
        _generator = generator;
        _logger = logger;
        _batchSize = batchSize;
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    // Interactive Challenge: IAsyncEnumerable for streaming
    public async IAsyncEnumerable<DocumentChunk> ProcessChunksAsync(
        IEnumerable<DocumentChunk> chunks, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var batchBuffer = new List<DocumentChunk>();

        foreach (var chunk in chunks)
        {
            batchBuffer.Add(chunk);

            if (batchBuffer.Count >= _batchSize)
            {
                // Process batch and stream results
                await foreach (var processed in ProcessBatchAsync(batchBuffer, ct))
                {
                    yield return processed;
                }
                batchBuffer.Clear();
            }
        }

        // Process remaining
        if (batchBuffer.Count > 0)
        {
            await foreach (var processed in ProcessBatchAsync(batchBuffer, ct))
            {
                yield return processed;
            }
        }
    }

    private async IAsyncEnumerable<DocumentChunk> ProcessBatchAsync(List<DocumentChunk> batch, CancellationToken ct)
    {
        _logger.LogInformation("Starting processing batch of {Count} chunks", batch.Count);

        // Create tasks for the batch
        var tasks = batch.Select(chunk => GetEmbeddingWithRetry(chunk, ct));

        // Await tasks as they complete to stream results
        var completedTasks = new List<Task<DocumentChunk>>();
        
        // We use a standard Task.WhenAll here to process in parallel, 
        // but yield results as they finish processing.
        var allTasks = tasks.ToList();
        
        while (allTasks.Count > 0)
        {
            // Wait for any task to complete
            var completed = await Task.WhenAny(allTasks);
            allTasks.Remove(completed);

            try 
            {
                var result = await completed; // Unwrap potential exceptions
                yield return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for a chunk after retries.");
                // Decide if we want to yield the original chunk or skip it. 
                // Here we skip it to maintain data integrity in the pipeline.
            }
        }
    }

    private async Task<DocumentChunk> GetEmbeddingWithRetry(DocumentChunk chunk, CancellationToken ct)
    {
        int retries = 0;
        int maxRetries = 3;
        int delay = 2000; // 2 seconds

        while (true)
        {
            try
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    var embedding = await _generator.GenerateEmbeddingAsync(chunk.Content, ct);
                    chunk.Embedding = embedding.Select(f => (byte)(f * 255)).ToArray(); // Mock serialization
                    return chunk;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                retries++;
                if (retries > maxRetries)
                {
                    _logger.LogWarning("Permanent failure for chunk {Id} after {Retries} retries", chunk.Id, retries);
                    throw; // Let the caller handle the failure
                }

                _logger.LogWarning("Retry {Retries}/{Max} for chunk {Id} due to {Error}", retries, maxRetries, chunk.Id, ex.Message);
                
                // Exponential Backoff
                await Task.Delay(delay, ct);
                delay *= 2;
            }
        }
    }
}
