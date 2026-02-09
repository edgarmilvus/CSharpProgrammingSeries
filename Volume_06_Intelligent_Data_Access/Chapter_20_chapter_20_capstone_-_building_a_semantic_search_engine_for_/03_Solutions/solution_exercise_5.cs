
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// 1. Updated Entity for Compression
public class MemoryEntry
{
    public Guid Id { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public string OriginalContext { get; set; } = string.Empty; // Full chunks
    public string CompressedContext { get; set; } = string.Empty; // Summarized/Truncated
    public DateTime CreatedAt { get; set; }
    public int UsageCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

// 2. RagOrchestrator
public class RagOrchestrator
{
    private readonly HybridSearchService _searchService;
    private readonly RagMemoryStore _memoryStore;
    private readonly ILogger<RagOrchestrator> _logger;

    public RagOrchestrator(HybridSearchService searchService, RagMemoryStore memoryStore, ILogger<RagOrchestrator> logger)
    {
        _searchService = searchService;
        _memoryStore = memoryStore;
        _logger = logger;
    }

    public async Task<string> RetrieveContextAsync(string query, CancellationToken ct = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("RagOrchestrator.RetrieveContext");
        activity?.SetTag("query.length", query.Length);

        // 1. Normalize
        var normalizedQuery = query.Trim().ToLower();

        // 2. Get or Set (Single query flow)
        var contextChunks = await _memoryStore.GetOrSetAsync(
            normalizedQuery, 
            async () => await FetchAndCompressAsync(normalizedQuery, ct), 
            ct);

        // 3. Return formatted string (using compressed version if available)
        // In a real scenario, we might return the full chunks to the LLM, 
        // but for this exercise we return the formatted string.
        return string.Join("\n\n---\n\n", contextChunks.Select(c => c.Content));
    }

    // Interactive Challenge: Batch Processing
    public async Task<Dictionary<string, string>> RetrieveContextBatchAsync(IEnumerable<string> queries, CancellationToken ct = default)
    {
        var results = new Dictionary<string, string>();
        var distinctQueries = queries.Distinct().ToList();
        var queryMap = distinctQueries.ToDictionary(q => q.Trim().ToLower(), q => q);

        // 1. Check Cache for all distinct queries
        // (Assuming RagMemoryStore supports batch lookup for efficiency, 
        // but here we simulate via loop for simplicity of the exercise structure)
        var tasks = distinctQueries.Select(async q =>
        {
            var normalized = q.Trim().ToLower();
            // Note: In a real implementation, we would optimize this to hit the DB once.
            // Here we rely on the internal GetOrSet logic.
            var context = await RetrieveContextAsync(q, ct);
            return (Original: q, Context: context);
        });

        var resolved = await Task.WhenAll(tasks);
        foreach(var item in resolved)
        {
            results[item.Original] = item.Context;
        }

        return results;
    }

    private async Task<IEnumerable<DocumentChunk>> FetchAndCompressAsync(string query, CancellationToken ct)
    {
        // A. Hybrid Search
        var stopwatch = Stopwatch.StartNew();
        var chunks = await _searchService.HybridSearchWeightedAsync(query, 5, 0.6, 0.4);
        _logger.LogInformation("Search execution took {Ms}ms", stopwatch.ElapsedMilliseconds);

        // B. Context Compression (Interactive Challenge)
        // Mock summarization: Truncate content to 200 chars per chunk or summarize
        var compressed = chunks.Select(c => new DocumentChunk 
        {
            Id = c.Id,
            Content = TruncateForCache(c.Content), 
            Embedding = c.Embedding,
            DocumentId = c.DocumentId
        }).ToList();

        return compressed;
    }

    private string TruncateForCache(string content)
    {
        // Simple compression logic: keep first 200 chars + "..."
        if (string.IsNullOrEmpty(content)) return string.Empty;
        return content.Length <= 200 ? content : content.Substring(0, 200) + "... [Compressed]";
    }
}

// 3. Dependency Injection Setup (Program.cs)
public static class ServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        // DbContexts
        services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(connectionString)); // Or Sqlite for demo
        
        services.AddDbContext<CacheDbContext>(options => 
            options.UseSqlite("Data Source=rag_cache.db"));

        // HttpClient
        services.AddHttpClient<IEmbeddingGenerator, MockEmbeddingGenerator>();

        // Services
        services.AddScoped<IEmbeddingGenerator, MockEmbeddingGenerator>();
        services.AddScoped<HybridSearchService>();
        
        // RagMemoryStore as Singleton to share the SemaphoreSlim across requests 
        // (or Scoped if using distributed locking)
        services.AddSingleton<RagMemoryStore>(); 
        
        services.AddScoped<RagOrchestrator>();

        // Background Service
        services.AddHostedService<CacheCleanupService>();

        // Logging
        services.AddLogging(builder => builder.AddConsole());
    }
}
