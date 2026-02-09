
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

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

// --- Client Implementation ---
public class AiInferenceClient : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _defaultTimeout;

    public AiInferenceClient(string baseUrl, TimeSpan defaultTimeout)
    {
        _defaultTimeout = defaultTimeout;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string> QueryAsync(string prompt, CancellationToken cancellationToken, TimeSpan? overrideTimeout = null)
    {
        var timeout = overrideTimeout ?? _defaultTimeout;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/infer")
        {
            Content = new StringContent($"{{\"prompt\": \"{prompt}\"}}")
        };

        // Create a linked token source to handle external cancellation + internal timeout
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            // Start the HTTP request
            var requestTask = _httpClient.SendAsync(request, linkedCts.Token);

            // Race the request against the timeout delay
            var delayTask = Task.Delay(timeout, linkedCts.Token);
            var completedTask = await Task.WhenAny(requestTask, delayTask);

            if (completedTask == delayTask)
            {
                // Timeout won the race
                throw new TimeoutException($"Request timed out after {timeout.TotalSeconds}s for prompt: {prompt}");
            }

            var response = await requestTask; // Await to unwrap exceptions
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API Error: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            // Distinguish between external cancellation and internal timeout
            throw new TimeoutException($"Request timed out after {timeout.TotalSeconds}s for prompt: {prompt}");
        }
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}

// --- Simulator (Minimal API) ---
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/infer", async (HttpContext context) =>
{
    var random = new Random();
    var delayMs = random.Next(100, 10000); // 100ms to 10s

    // 20% chance to hang indefinitely
    if (random.Next(0, 100) < 20)
    {
        // Simulate hang by waiting indefinitely (or very long)
        await Task.Delay(TimeSpan.FromMinutes(5));
    }

    // 10% chance to return 500
    if (random.Next(0, 100) < 10)
    {
        return Results.StatusCode(500);
    }

    await Task.Delay(delayMs);
    return Results.Ok(new { result = $"Processed after {delayMs}ms" });
});

app.Run("http://localhost:5000");
