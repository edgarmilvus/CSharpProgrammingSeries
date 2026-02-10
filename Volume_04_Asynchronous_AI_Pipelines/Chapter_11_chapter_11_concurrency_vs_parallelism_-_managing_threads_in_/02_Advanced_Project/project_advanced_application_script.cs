
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
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAIPipeline
{
    // Real-world context: A financial trading dashboard that needs to fetch
    // real-time stock prices from multiple sources simultaneously (Parallelism)
    // and process the incoming stream of data asynchronously (Concurrency)
    // without blocking the main UI thread (simulated here by the console).
    class Program
    {
        // HttpClient is intended to be instantiated once and reused throughout the life of an application.
        private static readonly HttpClient _httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Async AI Pipeline: Real-time Stock Sentiment Analysis");
            Console.WriteLine("-------------------------------------------------------------------");

            // 1. Define the data source (simulating an AI model's input requirements)
            string[] stockSymbols = { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA" };

            // 2. Create a cancellation token source to manage graceful shutdown
            var cts = new CancellationTokenSource();
            
            // 3. Start the pipeline
            await ProcessStockDataPipeline(stockSymbols, cts.Token);

            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Pipeline execution completed successfully.");
        }

        /// <summary>
        /// Orchestrates the asynchronous pipeline: Fetching -> Processing -> Aggregating.
        /// </summary>
        static async Task ProcessStockDataPipeline(string[] symbols, CancellationToken token)
        {
            // A. CONCURRENCY: Managing multiple tasks over time using async/await.
            // We create a list of tasks to represent the ongoing work.
            var tasks = new List<Task>();

            foreach (var symbol in symbols)
            {
                // We don't await immediately. We kick off the operation and store the task.
                // This allows the loop to continue, initiating parallel requests.
                tasks.Add(ProcessSingleStockAsync(symbol, token));
            }

            // B. PARALLELISM: Waiting for all tasks to complete.
            // Task.WhenAll executes the tasks concurrently (if hardware supports it).
            // Note: Since these are I/O bound (Network calls), they run concurrently 
            // on the OS level, not necessarily consuming multiple CPU cores.
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // Handle aggregate exceptions or specific cancellation exceptions
                Console.WriteLine($"Pipeline Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the lifecycle of a single stock symbol: Fetch -> Analyze -> Stream Result.
        /// </summary>
        static async Task ProcessSingleStockAsync(string symbol, CancellationToken token)
        {
            // 1. Simulate fetching raw data (Async I/O)
            string rawData = await FetchStockDataAsync(symbol, token);
            
            // 2. Simulate AI Processing (CPU bound work, but wrapped in async for consistency)
            double sentimentScore = await AnalyzeSentimentAsync(rawData, token);
            
            // 3. Stream the result back to the console
            await StreamResultAsync(symbol, sentimentScore, token);
        }

        /// <summary>
        /// Simulates an asynchronous network call to an external API.
        /// </summary>
        static async Task<string> FetchStockDataAsync(string symbol, CancellationToken token)
        {
            // Simulate network latency (500ms - 2000ms)
            Random rand = new Random(symbol.GetHashCode()); // Seed for deterministic-ish simulation
            int delay = rand.Next(500, 2000);
            
            Console.WriteLine($"[Network] Requesting data for {symbol} (Delay: {delay}ms)...");

            // NON-BLOCKING: Using Task.Delay instead of Thread.Sleep
            await Task.Delay(delay, token);

            // Simulate API response
            return $"{{ \"Symbol\": \"{symbol}\", \"Price\": {rand.Next(100, 500)}, \"Volume\": {rand.Next(1000, 50000)} }}";
        }

        /// <summary>
        /// Simulates an AI model inference (CPU intensive).
        /// In a real scenario, this might use ML.NET or Tensorflow.NET.
        /// </summary>
        static async Task<double> AnalyzeSentimentAsync(string rawData, CancellationToken token)
        {
            // Simulate heavy CPU computation
            Console.WriteLine($"[AI Model] Processing data for sentiment analysis...");
            
            // Run CPU-bound work on a background thread to avoid blocking the main thread
            return await Task.Run(() =>
            {
                // Simulate complex matrix multiplication or neural network inference
                double score = 0.0;
                for (int i = 0; i < 10000000; i++)
                {
                    score += 0.0000001;
                }
                return Math.Round(score % 1.0, 4); // Normalize between 0.0 and 1.0
            }, token);
        }

        /// <summary>
        /// Streams the processed result to the output console.
        /// </summary>
        static async Task StreamResultAsync(string symbol, double score, CancellationToken token)
        {
            // Simulate streaming chunks of data
            Console.Write($"[Stream] {symbol} Sentiment: ");
            
            // Visualize the streaming effect
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(50, token);
                Console.Write(".");
            }
            
            Console.WriteLine($" {score} (Completed)");
        }
    }
}
