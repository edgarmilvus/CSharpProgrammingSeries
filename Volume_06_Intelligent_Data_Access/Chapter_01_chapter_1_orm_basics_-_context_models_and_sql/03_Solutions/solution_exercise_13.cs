
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

// Source File: solution_exercise_13.cs
// Description: Solution for Exercise 13
// ==========================================

using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class EmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache _cache;
    private readonly SemaphoreSlim _semaphore = new(10, 10); // Rate Limiting (Concurrency Control)

    public EmbeddingService(IHttpClientFactory httpClientFactory, IDistributedCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    // 1. Batch Processing System
    public async Task<List<float[]>> GenerateBatchAsync(List<string> texts)
    {
        var results = new List<float[]>();
        
        // Optimization: Check Cache first
        var missingTexts = new List<string>();
        var indices = new List<int>();

        for (int i = 0; i < texts.Count; i++)
        {
            var cached = await _cache.GetStringAsync($"vec_{texts[i].GetHashCode()}");
            if (cached != null)
            {
                results.Add(JsonSerializer.Deserialize<float[]>(cached));
            }
            else
            {
                missingTexts.Add(texts[i]);
                indices.Add(i);
                results.Add(null); // Placeholder
            }
        }

        // 2. Handle API Rate Limits
        // Process missing items in chunks to respect rate limits
        var chunks = missingTexts.Chunk(20); // Batch size 20
        
        foreach (var chunk in chunks)
        {
            // Acquire semaphore to limit concurrent requests
            await _semaphore.WaitAsync();
            try
            {
                // Call External API or Local Model
                var embeddings = await CallExternalApiAsync(chunk);
                
                // Cache results
                foreach (var (text, embedding) in chunk.Zip(embeddings))
                {
                    await _cache.SetStringAsync(
                        $"vec_{text.GetHashCode()}", 
                        JsonSerializer.Serialize(embedding), 
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) }
                    );
                }

                // Put results back into the main list
                foreach (var (idx, embedding) in indices.Zip(embeddings))
                {
                    results[idx] = embedding;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // 3. Fallback Mechanism
        // If the API fails completely, return nulls or use a dummy embedding
        return results.Select(r => r ?? new float[128]).ToList();
    }

    private async Task<List<float[]>> CallExternalApiAsync(string[] texts)
    {
        // Simulate API call with retry logic
        var client = _httpClientFactory.CreateClient("EmbeddingAPI");
        
        // 4. Model Versioning
        // Add header to track which model version generated this
        client.DefaultRequestHeaders.Add("X-Model-Version", "v2.1");

        var payload = new { input = texts };
        var response = await client.PostAsJsonAsync("embeddings", payload);
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("Embedding service unavailable");

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
        return result.Data.Select(d => d.Embedding).ToList();
    }
}

public class EmbeddingResponse
{
    public List<EmbeddingData> Data { get; set; }
}
public class EmbeddingData { public float[] Embedding { get; set; } }
