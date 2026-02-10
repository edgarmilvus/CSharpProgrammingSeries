
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

// Source File: solution_exercise_7.cs
// Description: Solution for Exercise 7
// ==========================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class ModelCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ModelCacheService> _logger;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public ModelCacheService(IMemoryCache cache, ILogger<ModelCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        
        // Configure options with size limit and sliding expiration
        _cacheOptions = new MemoryCacheEntryOptions
        {
            Size = 1, // Unit of cost
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal
        };

        // Register callback for eviction (Dispose logic)
        _cacheOptions.RegisterPostEvictionCallback(OnModelEvicted);
    }

    public async Task<ModelInstance> GetOrLoadAsync(string modelId)
    {
        // Try to get from cache
        if (_cache.TryGetValue(modelId, out ModelInstance cachedModel))
        {
            return cachedModel;
        }

        // Cache miss - Load logic
        // Use GetOrCreateAsync to handle concurrency for the same key
        return await _cache.GetOrCreateAsync(modelId, async entry =>
        {
            _logger.LogInformation("Loading model {ModelId} into cache...", modelId);
            
            // Simulate loading
            await Task.Delay(100);
            var model = new ModelInstance(modelId, _logger);

            // Apply options
            entry.SetOptions(_cacheOptions);
            
            return model;
        });
    }

    private void OnModelEvicted(object key, object value, EvictionReason reason, object state)
    {
        if (value is ModelInstance instance)
        {
            _logger.LogInformation("Evicting model {Key}. Reason: {Reason}. Disposing...", key, reason);
            instance.Dispose();
        }
    }
}

// Program.cs Registration
// services.AddMemoryCache(options => options.SizeLimit = 100); // Limit total items
// services.AddSingleton<ModelCacheService>();
