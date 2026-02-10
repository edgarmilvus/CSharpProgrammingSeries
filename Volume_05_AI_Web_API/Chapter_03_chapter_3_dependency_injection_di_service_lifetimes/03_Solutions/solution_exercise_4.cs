
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ---------------------------------------------------------
// 1. & 2. CACHE STRATEGY & WRAPPER SERVICE
// ---------------------------------------------------------
public class CachedInferenceService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

    public CachedInferenceService(IMemoryCache cache, IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
    }

    public async Task<float> PredictAsync(ModelInput input)
    {
        // 3. CACHE KEY GENERATION
        // Hash the input to create a unique key. 
        // Using SHA256 ensures fixed length and handles complex objects.
        string cacheKey = $"pred_{ComputeHash(input)}";

        // 4. CACHE STAMPEDE PREVENTION
        // We use GetOrCreateAsync to leverage IMemoryCache's internal locking mechanisms,
        // or we can implement manual locking (as shown below) for granular control.
        
        // Check cache first (fast path)
        if (_cache.TryGetValue(cacheKey, out float cachedResult))
        {
            return cachedResult;
        }

        // Slow path: Lock specifically for this key to prevent multiple threads 
        // from computing the same value simultaneously.
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check lock pattern
            if (_cache.TryGetValue(cacheKey, out cachedResult))
            {
                return cachedResult;
            }

            // Create a scope to resolve the Scoped PredictionEngine
            using (var scope = _scopeFactory.CreateScope())
            {
                var engine = scope.ServiceProvider.GetRequiredService<PredictionEngine<ModelInput, ModelOutput>>();
                
                // Expensive operation
                var result = engine.Predict(input);
                
                // Store in cache with eviction policy
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10)) // Evict if not accessed for 10m
                    .SetSize(1) // Size for size-based eviction
                    .RegisterPostEvictionCallback(OnEviction); // Optional callback

                // Configure size limit in DI container (usually in Program.cs)
                // services.AddMemoryCache(options => options.SizeLimit = 512); // 512 items or MB depending on config

                _cache.Set(cacheKey, result.Sentiment, cacheEntryOptions);
                return result.Sentiment;
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private string ComputeHash(ModelInput input)
    {
        // Simplified hashing for example
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input.Text));
        return Convert.ToBase64String(bytes);
    }

    private void OnEviction(object key, object value, EvictionReason reason, object state)
    {
        // Log eviction for monitoring
        Console.WriteLine($"Cache item {key} evicted. Reason: {reason}");
    }
}

// ---------------------------------------------------------
// 3. SERVICE REGISTRATION
// ---------------------------------------------------------
public static class CacheRegistration
{
    public static void RegisterServices(IServiceCollection services)
    {
        // IMemoryCache is registered as Singleton by default
        services.AddMemoryCache(options =>
        {
            // 512MB limit (approximate, depends on object size)
            options.SizeLimit = 512 * 1024 * 1024; 
        });

        // CachedInferenceService must be Singleton to maintain the cache across requests
        services.AddSingleton<CachedInferenceService>();
        
        // PredictionEngine is Scoped (from Exercise 1)
        services.AddScoped<PredictionEngine<ModelInput, ModelOutput>>();
    }
}

// Dummy classes
public class ModelInput { public string Text { get; set; } }
public class ModelOutput { public float Sentiment { get; set; } }
