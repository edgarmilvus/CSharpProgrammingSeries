
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

// Source File: solution_exercise_10.cs
// Description: Solution for Exercise 10
// ==========================================

// Services/SessionStateManager.cs
using InferenceService.Models;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;

namespace InferenceService.Services;

public class SessionStateManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SessionStateManager> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public SessionStateManager(IConnectionMultiplexer redis, IMemoryCache memoryCache, ILogger<SessionStateManager> logger)
    {
        _redis = redis;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    // Read-Through Pattern with L1/L2 Cache
    public async Task<ConversationHistory?> GetSessionHistoryAsync(string sessionId)
    {
        // 1. Check L1 (Memory Cache) - Sliding Expiration 10s
        if (_memoryCache.TryGetValue(sessionId, out ConversationHistory? history))
        {
            return history;
        }

        // 2. Check L2 (Redis)
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"session:{sessionId}");
        
        if (json.IsNullOrEmpty) return null;

        history = JsonSerializer.Deserialize<ConversationHistory>(json!, _jsonOptions);
        
        // Populate L1
        if (history != null)
        {
            _memoryCache.Set(sessionId, history, TimeSpan.FromSeconds(10));
        }

        return history;
    }

    // Write-Through Pattern
    public async Task UpdateSessionHistoryAsync(string sessionId, string newMessage)
    {
        // 1. Update Local Object
        var history = await GetSessionHistoryAsync(sessionId) ?? new ConversationHistory();
        history.Messages.Add(newMessage);
        history.LastUpdated = DateTimeOffset.UtcNow;

        // 2. Invalidate L1 immediately to ensure consistency for subsequent reads on this pod
        _memoryCache.Remove(sessionId);

        // 3. Write to L2 (Redis) synchronously (Write-Through)
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(history, _jsonOptions);
        await db.StringSetAsync(
            key: $"session:{sessionId}", 
            value: json, 
            expiry: TimeSpan.FromMinutes(30) // TTL 30 mins
        );

        // 4. Update L1 with new value (optional, but good for immediate subsequent reads)
        _memoryCache.Set(sessionId, history, TimeSpan.FromSeconds(10));
    }
}
