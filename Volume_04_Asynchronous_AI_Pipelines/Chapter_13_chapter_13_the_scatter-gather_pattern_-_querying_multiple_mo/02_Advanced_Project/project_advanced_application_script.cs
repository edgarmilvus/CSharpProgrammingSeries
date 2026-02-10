
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AsyncAIPipelines_ScatterGather
{
    // Real-world context: A financial trading dashboard needs to fetch real-time stock prices
    // from multiple data providers (e.g., AlphaVantage, YahooFinance, Bloomberg) simultaneously.
    // We use the Scatter-Gather pattern to minimize latency by querying them in parallel
    // rather than sequentially, then aggregate the results to calculate a consensus price.

    class Program
    {
        // HttpClient is intended to be instantiated once and re-used throughout the life of an application.
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Financial Data Aggregation (Scatter-Gather Pattern)...");
            
            // List of stock symbols we want to query
            string[] symbols = { "MSFT", "GOOGL", "AAPL" };
            
            // List of data providers (simulated endpoints)
            string[] providers = { "AlphaVantage", "YahooFinance", "Bloomberg" };

            // We will store the tasks here to scatter them concurrently
            var tasks = new List<Task<StockData>>();

            // SCATTER PHASE: Launch requests to all providers for all symbols concurrently
            Console.WriteLine("\n[Scatter Phase] Launching concurrent requests...");
            foreach (var symbol in symbols)
            {
                foreach (var provider in providers)
                {
                    // We do not await here! We start the task and store it.
                    tasks.Add(GetStockPriceAsync(symbol, provider));
                }
            }

            // GATHER PHASE: Wait for ALL tasks to complete using Task.WhenAll
            // This blocks until every single request in the collection has finished (success or failure).
            Console.WriteLine("[Gather Phase] Waiting for all requests to complete...");
            StockData[] results = await Task.WhenAll(tasks);

            // Process the aggregated results
            Console.WriteLine("\n--- Aggregated Results ---");
            ProcessResults(results);

            // Advanced Handling: What if one fails? (Partial Failure Scenario)
            // Task.WhenAll throws immediately if ANY task throws. 
            // To handle partial failures gracefully (keep successful data, log failures),
            // we use a helper method to await tasks individually without crashing the whole batch.
            Console.WriteLine("\n--- Handling Partial Failures (Robust Aggregation) ---");
            await RobustScatterGather(symbols, providers);
        }

        /// <summary>
        /// Simulates an asynchronous API call to a specific financial data provider.
        /// </summary>
        /// <param name="symbol">Stock ticker (e.g., MSFT)</param>
        /// <param name="provider">Data source name</param>
        /// <returns>Task containing StockData</returns>
        private static async Task<StockData> GetStockPriceAsync(string symbol, string provider)
        {
            // Simulate network latency (random between 100ms and 2000ms)
            var random = new Random();
            int delay = random.Next(100, 2000);
            await Task.Delay(delay);

            // Simulate a failure for specific cases to demonstrate error handling later
            if (provider == "Bloomberg" && symbol == "GOOGL")
            {
                throw new HttpRequestException($"[Error] {provider} failed to connect for {symbol}.");
            }

            // Simulate varying prices from different providers
            double basePrice = symbol switch
            {
                "MSFT" => 370.00,
                "GOOGL" => 140.00,
                "AAPL" => 180.00,
                _ => 0
            };

            // Add random variance based on provider
            double variance = (provider.Length % 3) * 0.5; 
            double finalPrice = basePrice + variance;

            Console.WriteLine($"  -> Received: {symbol} @ {finalPrice:C} from {provider} (took {delay}ms)");

            return new StockData 
            { 
                Symbol = symbol, 
                Provider = provider, 
                Price = finalPrice, 
                Timestamp = DateTime.UtcNow 
            };
        }

        /// <summary>
        /// Aggregates and displays the collected data.
        /// </summary>
        private static void ProcessResults(StockData[] results)
        {
            // Group results by symbol to calculate an average consensus price
            var symbols = new HashSet<string>();
            foreach(var r in results) symbols.Add(r.Symbol);

            foreach(var symbol in symbols)
            {
                double sum = 0;
                int count = 0;
                foreach(var r in results)
                {
                    if(r.Symbol == symbol)
                    {
                        sum += r.Price;
                        count++;
                    }
                }
                double average = sum / count;
                Console.WriteLine($"Consensus for {symbol}: {average:C} (from {count} providers)");
            }
        }

        /// <summary>
        /// Demonstrates robust Scatter-Gather where individual task failures don't crash the whole batch.
        /// </summary>
        private static async Task RobustScatterGather(string[] symbols, string[] providers)
        {
            var tasks = new List<Task<StockData>>();
            
            // Scatter
            foreach (var symbol in symbols)
            {
                foreach (var provider in providers)
                {
                    tasks.Add(GetStockPriceAsync(symbol, provider));
                }
            }

            // Instead of Task.WhenAll (which throws), we await each task individually
            // using Task.WhenAll on a transformed list of tasks that never throw.
            var exceptionTasks = new List<Task>();
            var successfulResults = new List<StockData>();

            foreach (var task in tasks)
            {
                try
                {
                    // Await immediately to catch exceptions per task
                    StockData result = await task;
                    successfulResults.Add(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [!] Caught exception: {ex.Message}");
                }
            }

            Console.WriteLine($"\nSuccessfully gathered {successfulResults.Count} out of {tasks.Count} records.");
            ProcessResults(successfulResults.ToArray());
        }
    }

    // Simple POCO (Plain Old CLR Object) to hold data
    // Note: Using basic class structure as per constraints.
    public class StockData
    {
        public string Symbol { get; set; }
        public string Provider { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
