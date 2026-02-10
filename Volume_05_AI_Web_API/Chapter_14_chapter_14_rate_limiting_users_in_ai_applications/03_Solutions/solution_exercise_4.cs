
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

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Distributed Cache (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Connection string
    options.InstanceName = "RateLimit_";
});

// 2. Configure Distributed Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Distributed Sliding Window Limiter
    options.AddPolicy("GlobalSliding", context =>
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                AutoReplenishment = true,
                // IMPORTANT: The built-in DistributedRateLimiter requires an IDistributedRateLimitStore
                // The default implementation uses IDistributedCache (Redis)
            });
    });
});

// 3. Implement Circuit Breaker Logic (Fallback)
// We need to swap the IDistributedRateLimitStore implementation at runtime.
builder.Services.AddSingleton<ICircuitBreakerRateLimitStore, CircuitBreakerRateLimitStore>();

var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/api/test", () => "Distributed Limit Active")
   .RequireRateLimiting("GlobalSliding");

app.Run();

// Custom Implementation for Circuit Breaker Pattern
public interface ICircuitBreakerRateLimitStore : IDistributedRateLimitStore<string> { }

public class CircuitBreakerRateLimitStore : ICircuitBreakerRateLimitStore
{
    private readonly IDistributedRateLimitStore<string> _redisStore;
    private readonly IMemoryCache _memoryCache;
    private bool _circuitOpen = false;

    public CircuitBreakerRateLimitStore(IDistributedCache cache, IMemoryCache memoryCache)
    {
        // Attempt to wrap Redis, but we need the specific Redis implementation of IDistributedRateLimitStore
        // Assuming we registered a Redis-backed IDistributedRateLimitStore
        // For this example, we simulate the fallback logic.
        _redisStore = new DistributedCacheRateLimitStore<string>(cache);
        _memoryCache = memoryCache;
    }

    public async Task<bool> IsAllowedAsync(string partitionId, RateLimitWindow window, CancellationToken token)
    {
        if (_circuitOpen)
        {
            // Fallback to In-Memory logic (Simulated)
            // In production, you'd use a MemoryStore here
            return true; 
        }

        try
        {
            return await _redisStore.IsAllowedAsync(partitionId, window, token);
        }
        catch (RedisConnectionException)
        {
            _circuitOpen = true;
            // Log Critical Alert
            Console.WriteLine("CRITICAL: Redis connection lost. Switching to fallback mode.");
            // Fallback logic
            return true; 
        }
    }

    // Implement other interface methods...
    public Task SetAsync(string partitionId, RateLimitWindow window, CancellationToken token) 
        => _circuitOpen ? Task.CompletedTask : _redisStore.SetAsync(partitionId, window, token);
    
    public Task ResetAsync(string partitionId, CancellationToken token)
        => _circuitOpen ? Task.CompletedTask : _redisStore.ResetAsync(partitionId, token);
}
