
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Text.Json;

public class TokenBucketState
{
    public double Tokens { get; set; }
    public DateTime LastRefill { get; set; }
}

public class TokenBucket
{
    private readonly double _capacity;
    private readonly double _refillRate; // Tokens per second
    private readonly ConcurrentDictionary<string, TokenBucketState> _storage;

    public TokenBucket(double capacity, double refillRatePerMinute)
    {
        _capacity = capacity;
        _refillRate = refillRatePerMinute / 60.0;
        _storage = new ConcurrentDictionary<string, TokenBucketState>();
    }

    public bool TryConsume(string key, double tokensToConsume = 1)
    {
        var state = _storage.AddOrUpdate(key,
            // Factory: Create new state
            k => new TokenBucketState { Tokens = _capacity, LastRefill = DateTime.UtcNow },
            // Update: Refill and Consume
            (k, existing) =>
            {
                Refill(existing);
                return existing;
            });

        // Lock-free consumption check (optimistic concurrency)
        // In a high-concurrency scenario, we might need a lock or atomic decrement.
        // For this exercise, we use a simple check. 
        // Note: In a distributed system, this logic must be atomic (e.g., Redis Lua script).
        
        if (state.Tokens >= tokensToConsume)
        {
            state.Tokens -= tokensToConsume;
            return true;
        }

        return false;
    }

    private void Refill(TokenBucketState state)
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - state.LastRefill).TotalSeconds;
        
        if (timePassed > 0)
        {
            state.Tokens = Math.Min(_capacity, state.Tokens + (timePassed * _refillRate));
            state.LastRefill = now;
        }
    }

    public double GetRemainingTokens(string key)
    {
        if (_storage.TryGetValue(key, out var state))
        {
            Refill(state);
            return state.Tokens;
        }
        return _capacity;
    }

    public DateTime GetResetTime(string key)
    {
        if (_storage.TryGetValue(key, out var state))
        {
            // Time to fill from 0 to capacity
            var secondsToFill = _capacity / _refillRate;
            return state.LastRefill.AddSeconds(secondsToFill);
        }
        return DateTime.UtcNow;
    }
}

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets;

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
        _buckets = new ConcurrentDictionary<string, TokenBucket>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Identify Client
        string key = "anonymous";
        double limitPerMinute = 60; // Default for JWT

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var authMethod = context.User.FindFirst("auth_method")?.Value;
            if (authMethod == "api_key")
            {
                key = $"apikey_{context.User.Identity.Name}";
                limitPerMinute = 1000; // API Key limit
            }
            else
            {
                key = $"user_{context.User.Identity.NameIdentifier}";
            }
        }
        else
        {
            // Unauthenticated users get a very strict limit or are blocked by Auth middleware
            await _next(context);
            return;
        }

        // 2. Get or Create Bucket for this key
        var bucket = _buckets.GetOrAdd(key, k => new TokenBucket(limitPerMinute, limitPerMinute));

        // 3. Try Consume
        bool allowed = bucket.TryConsume(key);

        // 4. Set Headers
        var remaining = bucket.GetRemainingTokens(key);
        var resetTime = bucket.GetResetTime(key);
        var resetUnix = new DateTimeOffset(resetTime).ToUnixTimeSeconds();

        context.Response.Headers.Add("X-RateLimit-Limit", limitPerMinute.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", Math.Floor(remaining).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", resetUnix.ToString());

        if (!allowed)
        {
            context.Response.StatusCode = 429;
            var retryAfter = (int)(resetTime - DateTime.UtcNow).TotalSeconds;
            context.Response.Headers.Add("Retry-After", retryAfter.ToString());
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        await _next(context);
    }
}
