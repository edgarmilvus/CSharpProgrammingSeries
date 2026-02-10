
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

using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

public class DistributedStateStore<T>
{
    private readonly IConnectionMultiplexer _redis;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<DistributedStateStore<T>> _logger;

    public DistributedStateStore(IConnectionMultiplexer redis, ILogger<DistributedStateStore<T>> logger)
    {
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false 
        };
    }

    public async Task<T?> GetStateAsync(string key)
    {
        return await ExecuteWithRetry(async () =>
        {
            var db = _redis.GetDatabase();
            var json = await db.StringGetAsync(key);

            if (string.IsNullOrEmpty(json))
            {
                throw new StateNotFoundException($"State for key '{key}' not found.");
            }

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        });
    }

    public async Task SetStateAsync(string key, T state, TimeSpan? ttl = null)
    {
        await ExecuteWithRetry(async () =>
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(state, _jsonOptions);
            
            // Set the value with optional TTL
            await db.StringSetAsync(key, json, ttl);
            
            return true; // Dummy return for void wrapper
        });
    }

    // Resilience Strategy: Exponential Backoff
    private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action)
    {
        int retries = 0;
        int maxRetries = 3;
        int delayMs = 100;

        while (true)
        {
            try
            {
                return await action();
            }
            catch (RedisConnectionException ex)
            {
                retries++;
                if (retries > maxRetries)
                {
                    _logger.LogError(ex, "Redis operation failed after {Retries} retries.", maxRetries);
                    throw; // Re-throw to let caller handle critical failure
                }

                _logger.LogWarning(ex, "Redis connection failed. Retrying in {Delay}ms...", delayMs);
                await Task.Delay(delayMs);
                delayMs *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                // Catch other exceptions (e.g., serialization errors) and log them
                _logger.LogError(ex, "An unexpected error occurred during Redis operation.");
                throw;
            }
        }
    }
}

// Custom Exception for Cache Miss
public class StateNotFoundException : Exception
{
    public StateNotFoundException(string message) : base(message) { }
}
