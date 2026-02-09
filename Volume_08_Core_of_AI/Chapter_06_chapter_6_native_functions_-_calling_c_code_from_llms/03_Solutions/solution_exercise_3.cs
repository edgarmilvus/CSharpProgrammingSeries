
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StockPlugin
{
    // 6. Serialization Context: Adding [JsonSerializable] is crucial for Native AOT 
    // compilation scenarios where reflection metadata might be stripped.
    [JsonSerializable(typeof(StockAnalysisResult))]
    public partial class StockJsonContext : JsonSerializerContext { }

    // 1. Define the complex return type
    public record StockAnalysisResult
    {
        public string Symbol { get; init; } = string.Empty;
        public decimal CurrentPrice { get; init; }
        public double VolatilityIndex { get; init; }
        public bool IsBuyRecommended { get; init; }
    }

    public class StockMarketAnalyzer
    {
        [KernelFunction, Description("Analyzes a stock symbol and returns current metrics.")]
        public async Task<StockAnalysisResult> AnalyzeStockAsync(
            [Description("The stock symbol (e.g., MSFT)")] string stockSymbol)
        {
            // 4. Simulate network delay
            await Task.Delay(500);

            // 5. Mock logic
            if (stockSymbol.Equals("MSFT", StringComparison.OrdinalIgnoreCase))
            {
                return new StockAnalysisResult
                {
                    Symbol = "MSFT",
                    CurrentPrice = 415.50m,
                    VolatilityIndex = 1.24,
                    IsBuyRecommended = true
                };
            }

            throw new ArgumentException($"Symbol {stockSymbol} is not supported.");
        }
    }
}
