
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

using System;
using System.Threading.Tasks;

// Mock Data Models
public class PortfolioAnalysis
{
    public decimal Price { get; set; }
    public List<string> Trends { get; set; }
    public double Sentiment { get; set; }
    public decimal? EconomicIndicator { get; set; } // Nullable
    public bool IsEconomicDataIncomplete { get; set; }
}

public interface IFinanceApi
{
    Task<decimal> GetStockPrice(string ticker);
    Task<List<string>> GetHistoricalTrends(string ticker);
    Task<double> GetSentiment(string ticker);
    Task<decimal> GetEconomicIndicator(string ticker); // New Service
}

// Mock Implementation
public class MockFinanceApi : IFinanceApi
{
    public async Task<decimal> GetStockPrice(string ticker) { await Task.Delay(100); return 150.50m; }
    public async Task<List<string>> GetHistoricalTrends(string ticker) { await Task.Delay(120); return new List<string> { "Up", "Stable" }; }
    public async Task<double> GetSentiment(string ticker) { await Task.Delay(80); return 0.85; }
    
    public async Task<decimal> GetEconomicIndicator(string ticker)
    {
        await Task.Delay(500); // Slow
        // Simulate unreliability
        throw new HttpRequestException("Economic API Gateway Timeout");
    }
}

public class PortfolioAnalyzer
{
    private readonly IFinanceApi _financeApi;

    public PortfolioAnalyzer(IFinanceApi financeApi)
    {
        _financeApi = financeApi;
    }

    // Helper to safely extract result
    private async Task<(T Result, bool IsSuccess)> SafeGetResult<T>(Task<T> task)
    {
        try
        {
            var result = await task;
            return (result, true);
        }
        catch
        {
            return (default(T), false);
        }
    }

    public async Task<PortfolioAnalysis> AnalyzeStock(string ticker)
    {
        // 1. Start all tasks (Scatter)
        var priceTask = _financeApi.GetStockPrice(ticker);
        var trendTask = _financeApi.GetHistoricalTrends(ticker);
        var sentimentTask = _financeApi.GetSentiment(ticker);
        var economicTask = _financeApi.GetEconomicIndicator(ticker);

        // 2. Wait for the "Core" tasks (Price, Trends, Sentiment)
        // We ensure the critical path completes before worrying about the optional one.
        await Task.WhenAll(priceTask, trendTask, sentimentTask);

        // 3. Handle the "Best Effort" Economic Indicator
        // We don't await this inside WhenAll to prevent it from throwing.
        // Instead, we await it safely using the helper.
        var economicResult = await SafeGetResult(economicTask);

        // 4. Refactor return logic
        return new PortfolioAnalysis
        {
            Price = priceTask.Result,
            Trends = trendTask.Result,
            Sentiment = sentimentTask.Result,
            EconomicIndicator = economicResult.IsSuccess ? economicResult.Result : null,
            IsEconomicDataIncomplete = !economicResult.IsSuccess
        };
    }
}

// Example Usage
public class Program
{
    public static async Task Main()
    {
        var analyzer = new PortfolioAnalyzer(new MockFinanceApi());
        var analysis = await analyzer.AnalyzeStock("AAPL");

        Console.WriteLine($"Price: {analysis.Price}");
        Console.WriteLine($"Economic Indicator Missing: {analysis.IsEconomicDataIncomplete}");
    }
}
