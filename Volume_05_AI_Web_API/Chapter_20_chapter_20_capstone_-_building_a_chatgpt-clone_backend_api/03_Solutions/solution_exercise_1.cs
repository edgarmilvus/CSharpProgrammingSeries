
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

// 1. Service Abstraction
public interface IChatSessionCache
{
    Task<ChatSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task SetSessionAsync(string sessionId, ChatSession session, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}

// 2. Data Structure (Thread-safe wrapper for the session)
public class ChatSession
{
    // Using a thread-safe collection for concurrent updates during streaming
    private readonly List<ChatMessage> _messages = new();
    
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    // Expose as read-only to prevent external modification without logic
    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void AddMessage(ChatMessage message)
    {
        lock (_messages)
        {
            _messages.Add(message);
        }
    }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

// Implementation using IDistributedCache (Redis)
public class RedisChatSessionCache : IChatSessionCache
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisChatSessionCache(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<ChatSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        // Try to get the cached JSON string
        var cachedJson = await _cache.GetStringAsync($"chat_session:{sessionId}", cancellationToken);
        
        if (string.IsNullOrEmpty(cachedJson))
            return null;

        // Deserialize
        return JsonSerializer.Deserialize<ChatSession>(cachedJson, _jsonOptions);
    }

    public async Task SetSessionAsync(string sessionId, ChatSession session, CancellationToken cancellationToken)
    {
        // Serialize the session
        var cachedJson = JsonSerializer.Serialize(session, _jsonOptions);

        // Configure sliding expiration (30 minutes)
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync($"chat_session:{sessionId}", cachedJson, options, cancellationToken);
    }

    public async Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync($"chat_session:{sessionId}", cancellationToken);
    }
}
