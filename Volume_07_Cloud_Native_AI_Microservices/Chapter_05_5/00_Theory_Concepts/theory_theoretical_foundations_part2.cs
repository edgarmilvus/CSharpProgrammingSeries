
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

public interface IAgentStateStore
{
    Task<T?> GetStateAsync<T>(string sessionId, CancellationToken ct);
    Task SetStateAsync<T>(string sessionId, T state, CancellationToken ct);
}

public class RedisAgentStateStore : IAgentStateStore
{
    private readonly IDistributedCache _cache;
    
    public RedisAgentStateStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetStateAsync<T>(string sessionId, CancellationToken ct)
    {
        byte[]? data = await _cache.GetAsync(sessionId, ct);
        if (data == null) return default;

        // Using Source Generators for zero-reflection deserialization
        // This is a modern C# feature that drastically improves performance.
        return JsonSerializer.Deserialize(data, typeof(T)) as T;
    }

    public async Task SetStateAsync<T>(string sessionId, T state, CancellationToken ct)
    {
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(state);
        
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30) // Evict inactive sessions
        };

        await _cache.SetAsync(sessionId, data, options, ct);
    }
}
