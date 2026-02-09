
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

using Microsoft.SemanticKernel;
using StackExchange.Redis;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

namespace KernelMemory.Exercises;

public class OptimizedRAGSystem
{
    private readonly IConnectionMultiplexer _redis;
    private readonly Tracer _tracer;
    private readonly Kernel _kernel;

    public OptimizedRAGSystem(IConnectionMultiplexer redis, Kernel kernel)
    {
        _redis = redis;
        _kernel = kernel;
        
        // Initialize OpenTelemetry Tracer
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService("OptimizedRAG");
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("OptimizedRAG")
            .AddConsoleExporter()
            .Build();
            
        _tracer = tracerProvider.GetTracer("OptimizedRAG");
    }

    public async Task<string> ProcessQueryAsync(string query)
    {
        using var activity = _tracer.StartActiveSpan("ProcessQuery");
        
        // 1. Cache-Aside Pattern
        var db = _redis.GetDatabase();
        string cacheKey = $"rag:query:{query.GetHashCode()}";
        
        var cachedResult = await db.StringGetAsync(cacheKey);
        if (cachedResult.HasValue)
        {
            activity.SetTag("cache.hit", true);
            return cachedResult!;
        }
        activity.SetTag("cache.hit", false);

        // 2. Parallel Retrieval (Simulated from previous exercise)
        using var retrievalActivity = _tracer.StartActiveSpan("ParallelRetrieval");
        var tasks = new[] 
        {
            _kernel.InvokeAsync("TechSearch", new KernelArguments { ["query"] = query }),
            _kernel.InvokeAsync("ComplianceSearch", new KernelArguments { ["query"] = query })
        };
        await Task.WhenAll(tasks);
        retrievalActivity.End();

        // 3. Generation (Streaming)
        using var generationActivity = _tracer.StartActiveSpan("Generation");
        // Simulate generation
        var response = $"Generated response for: {query}";
        
        // Measure Time to First Token (TTFT) - simulated here by checking stopwatch
        var sw = Stopwatch.StartNew();
        await Task.Delay(50); // Simulate first token
        var ttft = sw.ElapsedMilliseconds;
        generationActivity.SetTag("ttft_ms", ttft);
        generationActivity.End();

        // 4. Cache Write (Async, fire-and-forget to reduce latency)
        // Set expiration to 1 hour
        _ = db.StringSetAsync(cacheKey, response, TimeSpan.FromHours(1));

        return response;
    }

    // 5. Cache Invalidation Strategy (Pseudocode/Logic)
    public async Task HandleDocumentUpdateAsync(string documentId)
    {
        // Strategy: Event-Driven Invalidation
        // When a document in KM is updated/deleted, an event is published.
        // This handler listens and invalidates relevant cache keys.
        
        // Logic:
        // 1. Identify the semantic scope of the document (e.g., tags, topics).
        // 2. Query Redis for keys matching patterns (e.g., using SCAN or tracking sets).
        //    Example: Redis Set `doc:{documentId}:queries` containing all queries that hit this doc.
        // 3. Delete those keys.
        
        var db = _redis.GetDatabase();
        
        // Retrieve the set of queries associated with this document
        string docQuerySetKey = $"doc:{documentId}:queries";
        var queries = await db.SetMembersAsync(docQuerySetKey);

        var batch = db.CreateBatch();
        var tasks = new List<Task>();

        foreach (var query in queries)
        {
            string cacheKey = $"rag:query:{query}";
            tasks.Add(batch.KeyDeleteAsync(cacheKey));
        }
        
        // Also delete the tracking set
        tasks.Add(batch.KeyDeleteAsync(docQuerySetKey));

        await batch.ExecuteAsync(tasks);
    }
}
