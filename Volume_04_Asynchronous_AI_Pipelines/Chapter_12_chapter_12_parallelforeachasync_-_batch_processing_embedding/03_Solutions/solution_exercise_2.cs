
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

public class RateLimitedEmbeddingService : IEmbeddingService
{
    private readonly SemaphoreSlim _semaphore;
    private readonly IEmbeddingService _innerService;

    public RateLimitedEmbeddingService(IEmbeddingService innerService, int rateLimit = 50)
    {
        _innerService = innerService;
        _semaphore = new SemaphoreSlim(rateLimit, rateLimit);
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // Wait asynchronously for permission to proceed
        await _semaphore.WaitAsync();
        try
        {
            // Simulate rate limiting delay (e.g., processing time or cooldown)
            await Task.Delay(20); 
            return await _innerService.GetEmbeddingAsync(text);
        }
        finally
        {
            // Always release the semaphore slot
            _semaphore.Release();
        }
    }
}

public class BatchedEmbeddingProcessor
{
    public async Task<ConcurrentBag<TextEmbedding>> ProcessInBatchesAsync(
        List<string> documents, 
        IEmbeddingService embeddingService,
        int batchSize = 10)
    {
        var results = new ConcurrentBag<TextEmbedding>();

        // 1. Pre-batching step: Group documents into chunks
        var batches = documents
            .Select((value, index) => new { Index = index, Value = value })
            .GroupBy(x => x.Index / batchSize)
            .Select(g => g.Select(x => x.Value).ToList())
            .ToList();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8
        };

        // 2. Process batches in parallel
        await Parallel.ForEachAsync(batches, parallelOptions, async (batch, cancellationToken) =>
        {
            // Process each document in the batch
            // Note: In a real scenario, APIs often support true batch requests (sending a list of texts).
            // Here we simulate individual processing within the batch context.
            var tasks = batch.Select(text => embeddingService.GetEmbeddingAsync(text));
            
            // Wait for all tasks in the batch to complete
            var vectors = await Task.WhenAll(tasks);

            // Thread-safe addition of all results from this batch
            for (int i = 0; i < batch.Count; i++)
            {
                results.Add(new TextEmbedding(batch[i], vectors[i]));
            }
        });

        return results;
    }
}
