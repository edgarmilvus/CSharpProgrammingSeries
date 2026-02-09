
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Register the HybridCache service with default configuration
builder.Services.AddHybridCache(options =>
{
    // 2. Set a default expiration time for cached entries
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        // 3. AbsoluteExpirationRelativeToNow defines the hard limit
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };
});

var app = builder.Build();

// 4. Define a simple domain object to cache
public record WeatherForecast(string City, DateTime Date, double TemperatureC, string Summary);

// 5. Define a request object for the endpoint
public record WeatherRequest(string City);

// 6. Create a mock service to simulate expensive AI model calls or database queries
public class WeatherService
{
    private readonly HybridCache _cache;

    public WeatherService(HybridCache cache)
    {
        _cache = cache;
    }

    // 7. Method to get weather with caching
    public async Task<WeatherForecast> GetWeatherAsync(string city, CancellationToken ct)
    {
        // 8. Create a unique cache key based on the input
        string cacheKey = $"weather:{city.ToLowerInvariant()}";

        // 9. Attempt to get the value from cache or compute it
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                // 10. This block executes ONLY on a cache miss
                // Simulate a delay (e.g., LLM inference time)
                await Task.Delay(1000, ct);

                // 11. Simulate generating a response
                var rng = new Random();
                return new WeatherForecast(
                    City: city,
                    Date: DateTime.UtcNow,
                    TemperatureC: rng.Next(-10, 35),
                    Summary: rng.Next(0, 10) > 5 ? "Sunny" : "Cloudy"
                );
            },
            // 12. Optional: Override default options per entry
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(2) // Shorter TTL for this specific data
            },
            // 13. Tags allow invalidating multiple related entries later
            tags: ["weather", $"city:{city}"],
            cancellationToken: ct
        );
    }
}

// 14. Define the API endpoint
app.MapPost("/api/weather", async (WeatherRequest request, WeatherService service, CancellationToken ct) =>
{
    // 15. Call the service which handles caching internally
    var forecast = await service.GetWeatherAsync(request.City, ct);
    return Results.Ok(forecast);
});

// 16. Define an endpoint to demonstrate cache invalidation
app.MapDelete("/api/weather/cache", async (string city, HybridCache cache, CancellationToken ct) =>
{
    // 17. Invalidate all entries tagged with the specific city
    await cache.RemoveByTagAsync($"city:{city}", ct);
    return Results.Ok($"Cache for {city} invalidated.");
});

app.Run();
