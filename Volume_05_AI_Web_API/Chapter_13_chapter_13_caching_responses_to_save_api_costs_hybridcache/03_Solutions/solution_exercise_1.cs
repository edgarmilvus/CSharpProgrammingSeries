
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

// 1. Model Definition
public enum SentimentType { Positive, Neutral, Negative }

public record SentimentResult(string Text, SentimentType Sentiment, double Confidence);

// 2. Service Interface and Implementation
public interface ISentimentAnalysisService
{
    Task<SentimentResult> AnalyzeAsync(string text, CancellationToken ct = default);
}

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly HybridCache _cache;

    public SentimentAnalysisService(HybridCache cache)
    {
        _cache = cache;
    }

    public async Task<SentimentResult> AnalyzeAsync(string text, CancellationToken ct = default)
    {
        // Edge Case: Validation before cache lookup to avoid caching invalid data
        if (string.IsNullOrWhiteSpace(text))
        {
            return new SentimentResult(text ?? string.Empty, SentimentType.Neutral, 0.0);
        }

        // Unique key based on text content
        var cacheKey = $"sentiment:{text.GetHashCode()}";

        // Attempt to retrieve or create
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                // Simulate expensive LLM call
                await Task.Delay(500, ct);

                // Mock generation logic (only executed on cache miss)
                var sentiment = DetermineMockSentiment(text);
                var confidence = Random.Shared.NextDouble() * (1.0 - 0.8) + 0.8; // 0.8 to 1.0

                return new SentimentResult(text, sentiment, confidence);
            },
            // Optional: Override default expiration per entry if needed
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            cancellationToken: ct
        );
    }

    private static SentimentType DetermineMockSentiment(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("good") || lower.Contains("great") || lower.Contains("excellent"))
            return SentimentType.Positive;
        if (lower.Contains("bad") || lower.Contains("terrible") || lower.Contains("poor"))
            return SentimentType.Negative;
        
        return SentimentType.Neutral;
    }
}

// 3. Program.cs Configuration
var builder = WebApplication.CreateBuilder(args);

// Configure HybridCache
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        // Requirement: Default Entry Expiration: 10 minutes
        Expiration = TimeSpan.FromMinutes(10),
        // Requirement: Maximum Entries (Local Cache Size)
        LocalCacheExpiration = TimeSpan.FromMinutes(10) 
    };
    
    // Note: In a real scenario, you would configure the Distributed Cache (Redis) here.
    // For this exercise, we rely on the in-memory layer.
});

// Register the service
builder.Services.AddSingleton<ISentimentAnalysisService, SentimentAnalysisService>();

var app = builder.Build();

// 4. Endpoint
app.MapGet("/analyze", async (string text, ISentimentAnalysisService service, CancellationToken ct) =>
{
    var result = await service.AnalyzeAsync(text, ct);
    return Results.Ok(result);
});

app.Run();
