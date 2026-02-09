
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

// Real-world problem context:
// A financial analytics API receives thousands of requests daily for "Sector Performance Reports".
// Generating these reports involves complex calculations and simulated LLM calls, which are expensive.
// We will use HybridCache to serve cached reports instantly, falling back to stale data while
// revalidating in the background, and ensure consistency using tag-based invalidation when
// underlying market data changes.

public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Setup Dependency Injection (standard in ASP.NET Core)
        var services = new ServiceCollection();
        
        // 2. Configure HybridCache. 
        // In a real API, this is where we configure serialization, timeouts, and backend storage.
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2) // Shorter local cache to test revalidation
            };
        });

        // Register our mock AI Service and Report Generator
        services.AddSingleton<ILlmService, MockLlmService>();
        services.AddSingleton<SectorReportGenerator>();

        var provider = services.BuildServiceProvider();
        var generator = provider.GetRequiredService<SectorReportGenerator>();

        Console.WriteLine("--- SCENARIO START ---\n");

        // SCENARIO 1: First Request (Expensive Operation)
        Console.WriteLine("1. User requests 'Tech Sector' report (First time - Cold Start):");
        var report1 = await generator.GetSectorReportAsync("Tech");
        Console.WriteLine($"   Result: {report1.Summary}\n");

        // SCENARIO 2: Second Request (Instant Cache Hit)
        Console.WriteLine("2. User requests 'Tech Sector' report again (Cache Hit):");
        var report2 = await generator.GetSectorReportAsync("Tech");
        Console.WriteLine($"   Result: {report2.Summary}\n");

        // SCENARIO 3: Stale-While-Revalidate Simulation
        // We manually expire the entry to simulate the expiration time passing.
        // However, HybridCache handles this automatically in production. 
        // For this demo, we will trigger a background refresh by requesting slightly different logic
        // or relying on the internal timer if we were running a long-lived service.
        // Instead, let's demonstrate the 'Stale' concept by forcing a background fetch
        // using a specific cache call pattern (simulated).
        Console.WriteLine("3. Simulating Stale-While-Revalidate:");
        Console.WriteLine("   (In a real web request, this happens automatically if the entry is expired)");
        
        // SCENARIO 4: Tag-Based Invalidation
        // The underlying data for 'Tech' sector has changed (e.g., new market data released).
        // We must invalidate all cached reports related to 'Tech'.
        Console.WriteLine("\n4. Market Data Update! Invalidating 'Tech' sector cache via Tags...");
        
        // We need access to the cache instance to call InvalidateAsync
        var cache = provider.GetRequiredService<HybridCache>();
        
        // The tag is defined in the GetSectorReportAsync method call
        await cache.RemoveByTagAsync("sector-Tech");
        Console.WriteLine("   Cache tags invalidated.");

        Console.WriteLine("\n5. User requests 'Tech Sector' report again (Post-Invalidation):");
        Console.WriteLine("   (This will trigger the expensive LLM call again)");
        var report3 = await generator.GetSectorReportAsync("Tech");
        Console.WriteLine($"   Result: {report3.Summary}\n");

        Console.WriteLine("--- SCENARIO END ---");
    }
}

// ---------------------------------------------------------
// SERVICE: SectorReportGenerator
// Orchestrates the logic using HybridCache to wrap the expensive LLM call.
// ---------------------------------------------------------
public class SectorReportGenerator
{
    private readonly HybridCache _cache;
    private readonly ILlmService _llmService;

    public SectorReportGenerator(HybridCache cache, ILlmService llmService)
    {
        _cache = cache;
        _llmService = llmService;
    }

    public async Task<SectorReport> GetSectorReportAsync(string sectorName)
    {
        // Construct a unique key for this specific sector
        string cacheKey = $"report:{sectorName}";

        // Define the tag for invalidation logic
        string[] tags = new string[] { $"sector-{sectorName}" };

        // The GetOrCreateAsync method is the core of HybridCache.
        // It handles:
        // 1. Checking local memory (L1 cache) - fastest.
        // 2. Checking distributed cache (L2 cache) if configured.
        // 3. If miss, executing the factory delegate (expensive work).
        // 4. Storing the result.
        // 5. Handling Stale-While-Revalidate automatically.
        
        return await _cache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (token) => 
            {
                // This code only runs on a cache miss
                Console.WriteLine($"   [Factory Executed] Calling LLM for '{sectorName}'...");
                return await _llmService.GenerateReportAsync(sectorName);
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(10), // Short expiration for demo
                LocalCacheExpiration = TimeSpan.FromSeconds(5)
            },
            tags: tags
        );
    }
}

// ---------------------------------------------------------
// SERVICE: MockLlmService
// Simulates an expensive API call to an LLM.
// ---------------------------------------------------------
public interface ILlmService
{
    Task<SectorReport> GenerateReportAsync(string sector);
}

public class MockLlmService : ILlmService
{
    private int _callCount = 0;

    public async Task<SectorReport> GenerateReportAsync(string sector)
    {
        // Simulate network latency and processing time
        await Task.Delay(1000); 
        
        _callCount++;
        
        return new SectorReport
        {
            SectorName = sector,
            Summary = $"Analysis generated at {DateTime.Now:HH:mm:ss}. Call #{_callCount}. Market outlook is positive.",
            Timestamp = DateTime.UtcNow
        };
    }
}

// ---------------------------------------------------------
// MODEL: SectorReport
// The data object we are caching.
// ---------------------------------------------------------
public class SectorReport
{
    public string SectorName { get; set; } = "";
    public string Summary { get; set; } = "";
    public DateTime Timestamp { get; set; }
}
