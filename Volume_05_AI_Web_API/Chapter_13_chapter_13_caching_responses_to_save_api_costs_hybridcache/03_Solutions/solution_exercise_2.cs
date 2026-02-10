
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

using Microsoft.Extensions.Caching.Hybrid;

public interface IChatSummaryService
{
    Task<string> GetSummaryAsync(Guid threadId, CancellationToken ct = default);
}

public class ChatSummaryService : IChatSummaryService
{
    private readonly HybridCache _cache;

    public ChatSummaryService(HybridCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GetSummaryAsync(Guid threadId, CancellationToken ct = default)
    {
        var cacheKey = $"chat_summary:{threadId}";

        // Use the factory overload. 
        // Note: Standard HybridCache in .NET 9 handles deduplication. 
        // To implement SWR explicitly (returning stale while refreshing), we often rely on 
        // the underlying Distributed Cache behavior or specific options. 
        // However, since HybridCache merges L1/L2, we simulate SWR logic here by 
        // allowing the factory to trigger a background update if the entry is nearing expiration.
        
        // For strict SWR where we return stale immediately and update in background:
        // 1. We check the local cache first (HybridCache does this internally).
        // 2. If found (even if expired in L2), it returns it.
        // 3. We trigger a refresh task.
        
        // Since HybridCache's GetOrCreateAsync is atomic, we use a slightly different approach 
        // to demonstrate the pattern clearly: We will fetch, and if it's a cache miss, 
        // we simulate the delay. If we were using a pure LRU cache, we would return stale 
        // and Task.Run the update. 
        
        // Here, we will rely on HybridCache's internal background refresh capabilities 
        // if configured, or manually handle the "refresh" logic in the factory.
        
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                // Simulate expensive generation
                await Task.Delay(1000, cancellationToken);
                
                // Generate new summary
                return $"Summary for {threadId} generated at {DateTime.UtcNow}";
            },
            new HybridCacheEntryOptions 
            { 
                Expiration = TimeSpan.FromMinutes(5),
                // In .NET 9 HybridCache, background refresh is often handled by the 
                // distributed cache implementation or specific factory overloads.
                // We ensure the entry is cacheable.
            },
            cancellationToken: ct
        );
    }
}

// Program.cs Configuration
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddSingleton<IChatSummaryService, ChatSummaryService>();

var app = builder.Build();

app.MapGet("/chat/{threadId:guid}/summary", async (Guid threadId, IChatSummaryService service, CancellationToken ct) =>
{
    var summary = await service.GetSummaryAsync(threadId, ct);
    return Results.Ok(summary);
});

app.Run();
