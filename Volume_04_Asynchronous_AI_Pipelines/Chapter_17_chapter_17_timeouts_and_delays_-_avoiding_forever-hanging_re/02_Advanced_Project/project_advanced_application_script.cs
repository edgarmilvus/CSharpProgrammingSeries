
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTimeoutsApp
{
    class Program
    {
        // Real-world context: A financial data aggregator that fetches stock prices
        // from multiple unreliable third-party APIs. Some APIs are slow, some are flaky.
        // We need to ensure the entire system remains responsive and doesn't hang indefinitely
        // on a single slow API call, while gracefully degrading the user experience.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Financial Data Aggregator...");
            var aggregator = new FinancialDataAggregator();
            
            // Simulate a user requesting data for 3 different stocks simultaneously.
            // We use a list of stock symbols to demonstrate parallelism.
            string[] stockSymbols = { "AAPL", "GOOGL", "TSLA" };
            
            Console.WriteLine($"\nFetching data for {string.Join(", ", stockSymbols)}...");
            
            // The core logic: Fetch data with strict timeouts and retry mechanisms.
            // We pass a cancellation token to allow external cancellation if needed.
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) // Global timeout for the entire operation
            {
                try
                {
                    // Execute all fetches in parallel. The aggregator handles individual timeouts.
                    var results = await aggregator.FetchStockDataParallelAsync(stockSymbols, cts.Token);
                    
                    // Process and display successful results
                    Console.WriteLine("\n--- AGGREGATION COMPLETE ---");
                    foreach (var result in results)
                    {
                        if (result.IsSuccess)
                        {
                            Console.WriteLine($"SUCCESS: {result.Symbol} -> Price: ${result.Price:F2} (Fetched in {result.DurationMs}ms)");
                        }
                        else
                        {
                            Console.WriteLine($"FAILURE: {result.Symbol} -> Reason: {result.ErrorMessage}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("\nCRITICAL ERROR: The entire operation timed out or was cancelled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nUNEXPECTED ERROR: {ex.Message}");
                }
            }

            Console.WriteLine("\nApplication shutting down.");
        }
    }

    // Data structure to hold the result of a stock fetch operation.
    // Using a simple class to avoid advanced features like Records.
    public class StockResult
    {
        public string Symbol { get; set; }
        public bool IsSuccess { get; set; }
        public double Price { get; set; }
        public string ErrorMessage { get; set; }
        public long DurationMs { get; set; }
    }

    public class FinancialDataAggregator
    {
        private readonly HttpClient _httpClient;
        // Simulate API endpoints. In a real app, these would be real URLs.
        // We use a dictionary to map symbols to simulated latency and failure rates.
        private readonly Dictionary<string, (int LatencyMs, double FailureRate)> _apiSimulations;

        public FinancialDataAggregator()
        {
            _httpClient = new HttpClient();
            // Basic timeout for the HttpClient itself (though we handle granular timeouts manually).
            _httpClient.Timeout = TimeSpan.FromSeconds(5); 

            // Simulation data: 
            // AAPL: Fast, reliable.
            // GOOGL: Slow (3 seconds), but reliable.
            // TSLA: Very slow (8 seconds) and flaky (50% chance of failure).
            _apiSimulations = new Dictionary<string, (int, double)>
            {
                { "AAPL", (500, 0.0) },
                { "GOOGL", (3000, 0.0) },
                { "TSLA", (8000, 0.5) }
            };
        }

        // Method to fetch data for multiple stocks in parallel.
        // Implements parallelism and per-request timeout management.
        public async Task<List<StockResult>> FetchStockDataParallelAsync(string[] symbols, CancellationToken globalToken)
        {
            var tasks = new List<Task<StockResult>>();
            var results = new List<StockResult>();

            // 1. PARALLELISM: Launch all requests concurrently.
            foreach (var symbol in symbols)
            {
                // We pass the global token to allow cancellation propagation.
                // However, each task has its own internal timeout logic.
                tasks.Add(FetchSingleStockWithRetryAsync(symbol, globalToken));
            }

            // 2. AWAIT ALL: Wait for all tasks to complete (or fail).
            // Task.WhenAll does not throw immediately; it aggregates exceptions.
            // We catch them later or inspect individual task results.
            StockResult[] completedResults = await Task.WhenAll(tasks);

            // 3. AGGREGATION: Collect results.
            // Note: If a task throws an exception, Task.WhenAll will rethrow it.
            // However, we designed FetchSingleStockWithRetryAsync to return a result object
            // even on failure, so exceptions are contained within the method logic unless
            // a catastrophic timeout occurs.
            foreach (var result in completedResults)
            {
                results.Add(result);
            }

            return results;
        }

        // Core logic: Fetch a single stock with retry logic and timeout handling.
        private async Task<StockResult> FetchSingleStockWithRetryAsync(string symbol, CancellationToken globalToken)
        {
            int maxRetries = 2;
            int attempt = 0;
            var stopwatch = Stopwatch.StartNew();

            // Retrieve simulation parameters
            if (!_apiSimulations.TryGetValue(symbol, out var simulation))
            {
                return new StockResult { Symbol = symbol, IsSuccess = false, ErrorMessage = "Unknown symbol" };
            }

            while (attempt <= maxRetries)
            {
                // 3. TIMEOUT CONFIGURATION: 
                // We define a specific timeout for this API call.
                // This prevents a single attempt from hanging forever.
                // We use a local CancellationTokenSource linked to the global token.
                using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(4))) // Per-attempt timeout
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken, timeoutCts.Token))
                {
                    try
                    {
                        // Simulate the API call delay and potential failure
                        // In a real app, this would be: await _httpClient.GetAsync(...)
                        await SimulateApiCall(simulation.LatencyMs, simulation.FailureRate, linkedCts.Token);

                        stopwatch.Stop();
                        return new StockResult
                        {
                            Symbol = symbol,
                            IsSuccess = true,
                            Price = new Random().NextDouble() * 100 + 100, // Random price between 100-200
                            DurationMs = stopwatch.ElapsedMilliseconds
                        };
                    }
                    catch (OperationCanceledException)
                    {
                        // Check which token triggered the cancellation
                        if (timeoutCts.Token.IsCancellationRequested)
                        {
                            // The per-attempt timeout triggered
                            Console.WriteLine($"[{symbol}] Attempt {attempt + 1} timed out (4s).");
                        }
                        else
                        {
                            // The global token triggered
                            Console.WriteLine($"[{symbol}] Operation cancelled globally.");
                            throw; // Re-throw to stop the entire process
                        }
                    }
                    catch (Exception ex)
                    {
                        // Catch other exceptions (e.g., network errors)
                        Console.WriteLine($"[{symbol}] Attempt {attempt + 1} failed: {ex.Message}");
                    }
                }

                // 4. RETRY WITH EXPONENTIAL BACKOFF & JITTER
                // If we reach here, the attempt failed or timed out.
                attempt++;
                if (attempt <= maxRetries)
                {
                    // Exponential backoff: wait time increases (1s, 2s, 4s...)
                    double delay = Math.Pow(2, attempt) * 500; // 500ms base
                    
                    // Jitter: Add randomness to avoid "Thundering Herd" problem.
                    // If all clients retry at exactly the same time, they might overwhelm the API again.
                    // Randomness spreads out the retry requests.
                    var random = new Random();
                    double jitter = random.Next(0, 300); // 0-300ms jitter
                    int totalDelay = (int)(delay + jitter);

                    Console.WriteLine($"[{symbol}] Retrying in {totalDelay}ms...");
                    try
                    {
                        // Wait before next attempt. 
                        // We use Task.Delay with the linked cancellation token to allow aborting the wait.
                        await Task.Delay(totalDelay, linkedCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // If cancelled during the backoff delay, stop retrying.
                        Console.WriteLine($"[{symbol}] Retry delay cancelled.");
                        throw;
                    }
                }
            }

            // If all retries exhausted
            return new StockResult
            {
                Symbol = symbol,
                IsSuccess = false,
                ErrorMessage = "All retry attempts failed due to timeouts or errors.",
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }

        // Helper method to simulate network conditions
        private async Task SimulateApiCall(int latencyMs, double failureRate, CancellationToken token)
        {
            // Simulate network latency
            await Task.Delay(latencyMs, token);

            // Simulate random failure
            if (new Random().NextDouble() < failureRate)
            {
                throw new HttpRequestException("Simulated API connection failure.");
            }
        }
    }
}
