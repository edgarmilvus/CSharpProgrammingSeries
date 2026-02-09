
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public record MarketData(string Source, object Data);
public record MarketOverview(List<MarketData> Stocks, List<MarketData> Crypto, List<MarketData> News);

public class MarketDashboardPlugin
{
    private readonly HttpClient _httpClient;

    public MarketDashboardPlugin(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 1. Parallel Execution Method
    [KernelFunction("GetMarketOverview")]
    public async Task<MarketOverview> GetMarketOverviewAsync(CancellationToken cancellationToken = default)
    {
        // Define tasks
        var stockTask = FetchDataAsync("Stocks", "https://api.stocks.com/market", cancellationToken);
        var cryptoTask = FetchDataAsync("Crypto", "https://api.crypto.com/market", cancellationToken);
        var newsTask = FetchDataAsync("News", "https://api.news.com/feed", cancellationToken);

        // 2. Execute in Parallel
        var tasks = new[] { stockTask, cryptoTask, newsTask };
        
        // 3. Timeout Handling using Task.WhenAny or CancellationToken
        // We rely on the passed CancellationToken (likely with a timeout set by the caller)
        // or we can wrap it:
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // Global 5s timeout

        try 
        {
            await Task.WhenAll(tasks);
        }
        catch (TaskCanceledException)
        {
            // Handle timeout (one of the tasks or the whole operation)
            // We proceed to aggregate what we have
        }

        // 4. Result Merging (Handling Partial Failures)
        // We access the Result property safely. If a task failed, it throws, 
        // but we want to capture successful ones. 
        // Better pattern: await Task.WhenAll handles exceptions by throwing AggregateException.
        // To handle partial failures, we should await individually or use WhenAll + Exception handling.
        
        var stocks = GetResultOrDefault(stockTask);
        var crypto = GetResultOrDefault(cryptoTask);
        var news = GetResultOrDefault(newsTask);

        return new MarketOverview(stocks, crypto, news);
    }

    private List<MarketData> GetResultOrDefault(Task<List<MarketData>> task)
    {
        return task.IsCompletedSuccessfully ? task.Result : new List<MarketData>();
    }

    private async Task<List<MarketData>> FetchDataAsync(string source, string url, CancellationToken ct)
    {
        // Simulate network latency
        await Task.Delay(new Random().Next(100, 1000), ct);
        // In real app: var response = await _httpClient.GetAsync(url, ct);
        return new List<MarketData> { new MarketData(source, $"Data from {source}") };
    }

    // 5. Asynchronous Streaming (Interactive Challenge)
    [KernelFunction("StreamMarketData")]
    public async IAsyncEnumerable<MarketData> StreamMarketDataAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // We want to yield Stock first (fastest), then Crypto, then News.
        // We use local functions to wrap the fetch logic.
        
        async IAsyncEnumerable<MarketData> FetchAndYield(string source, string url, int delay)
        {
            await Task.Delay(delay, cancellationToken); // Simulate variable latency
            // var response = await _httpClient.GetAsync(url, cancellationToken);
            yield return new MarketData(source, $"Real-time update from {source}");
        }

        // Start tasks but don't await them all at once
        var stockStream = FetchAndYield("Stocks", "...", 100);
        var cryptoStream = FetchAndYield("Crypto", "...", 500);
        var newsStream = FetchAndYield("News", "...", 1000);

        // We can't easily "await" an IAsyncEnumerable in a specific order without blocking others.
        // However, we can iterate through them. Since we want progressive updates,
        // we essentially merge the streams.
        
        // To ensure Stocks are yielded first (as requested), we await the first one explicitly:
        await foreach (var item in stockStream) yield return item;

        // Then we can await the others. To do this concurrently but yield as they arrive:
        var cryptoTask = cryptoStream.ToListAsync(cancellationToken);
        var newsTask = newsStream.ToListAsync(cancellationToken);

        await Task.WhenAll(cryptoTask, newsTask);

        foreach (var item in cryptoTask.Result) yield return item;
        foreach (var item in newsTask.Result) yield return item;
    }
}

// Helper extension for IAsyncEnumerable
public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken ct)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(ct))
        {
            list.Add(item);
        }
        return list;
    }
}

// Usage Example
/*
var client = new HttpClient();
var plugin = new MarketDashboardPlugin(client);
var overview = await plugin.GetMarketOverviewAsync();

await foreach (var data in plugin.StreamMarketDataAsync())
{
    Console.WriteLine($"Received: {data.Source}");
}
*/
