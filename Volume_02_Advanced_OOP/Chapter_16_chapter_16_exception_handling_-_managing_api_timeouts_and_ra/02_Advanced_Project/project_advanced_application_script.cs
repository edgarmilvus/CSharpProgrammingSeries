
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
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedOOP_DataStructures
{
    // ---------------------------------------------------------
    // 1. CUSTOM EXCEPTION HIERARCHY
    // ---------------------------------------------------------
    // We define specific exceptions to distinguish between different failure modes.
    // This allows the calling code to react differently to timeouts vs. rate limits.

    public class ApiTimeoutException : Exception
    {
        public ApiTimeoutException(string message) : base(message) { }
    }

    public class RateLimitException : Exception
    {
        public int RetryAfterSeconds { get; }

        public RateLimitException(string message, int retryAfter) : base(message)
        {
            RetryAfterSeconds = retryAfter;
        }
    }

    // ---------------------------------------------------------
    // 2. MOCK EXTERNAL API SERVICE
    // ---------------------------------------------------------
    // Simulates an external AI API that exhibits timeouts and rate limiting.
    // In a real app, this would be an HttpClient call.

    public class MockAiApiService
    {
        private int _currentRequestCount = 0;
        private readonly int _rateLimitThreshold = 5;
        private readonly Random _random = new Random();

        public async Task<string> QueryModelAsync(string input)
        {
            // Simulate network latency (between 50ms and 3000ms)
            int latency = _random.Next(50, 3000);
            await Task.Delay(latency);

            _currentRequestCount++;

            // Case 1: Rate Limit Exceeded
            if (_currentRequestCount > _rateLimitThreshold)
            {
                // Reset counter for next minute simulation
                _currentRequestCount = 0; 
                // Simulate server telling us to wait 3 seconds
                throw new RateLimitException("API Rate Limit Exceeded (429).", 3);
            }

            // Case 2: Random Timeout Simulation (5% chance)
            if (_random.Next(0, 100) < 5)
            {
                throw new ApiTimeoutException("The request timed out waiting for the server.");
            }

            // Success
            return $"Processed: '{input}' (Sentiment: Positive)";
        }
    }

    // ---------------------------------------------------------
    // 3. EXCEPTION HANDLER WITH RETRY LOGIC
    // ---------------------------------------------------------
    // Uses Delegates and Lambda Expressions for flexible retry logic.

    public class ApiRequestHandler
    {
        private readonly MockAiApiService _apiService;

        public ApiRequestHandler(MockAiApiService apiService)
        {
            _apiService = apiService;
        }

        // This method encapsulates the retry logic.
        // It accepts a "function delegate" (Func<Task<string>>) which represents the operation to retry.
        public async Task<string> ExecuteWithRetryAsync(Func<Task<string>> apiCall)
        {
            int retryCount = 0;
            int maxRetries = 4;
            int delay = 1000; // Initial delay in ms

            while (retryCount <= maxRetries)
            {
                try
                {
                    // Execute the provided lambda expression (the API call)
                    return await apiCall();
                }
                catch (ApiTimeoutException ex)
                {
                    retryCount++;
                    if (retryCount > maxRetries)
                    {
                        Console.WriteLine($"[Error] Max retries ({maxRetries}) reached for timeout. Giving up.");
                        throw; // Re-throw the exception if we can't recover
                    }

                    Console.WriteLine($"[Warning] Timeout detected: {ex.Message}");
                    Console.WriteLine($"[Action] Retrying in {delay}ms... (Attempt {retryCount}/{maxRetries})");
                    
                    await Task.Delay(delay);
                    delay *= 2; // Exponential Backoff: Double the wait time
                }
                catch (RateLimitException ex)
                {
                    // Rate limits usually require a specific wait time defined by the server
                    Console.WriteLine($"[Warning] Rate limit hit: {ex.Message}");
                    
                    // We treat rate limits as a "hard" stop for this batch of retries 
                    // or wait the specific time requested.
                    Console.WriteLine($"[Action] Waiting server-specified cooldown of {ex.RetryAfterSeconds}s...");
                    
                    await Task.Delay(ex.RetryAfterSeconds * 1000);
                    
                    // Reset retry count for the new "minute" window
                    retryCount = 0; 
                    delay = 1000; 
                }
                catch (Exception ex)
                {
                    // Catch-all for unexpected errors
                    Console.WriteLine($"[Critical] Unexpected error: {ex.Message}");
                    throw;
                }
            }

            return "Failed to get result.";
        }
    }

    // ---------------------------------------------------------
    // 4. MAIN PROGRAM EXECUTION
    // ---------------------------------------------------------

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- AI Data Pipeline Initialization ---");

            var apiService = new MockAiApiService();
            var handler = new ApiRequestHandler(apiService);

            // List of user feedback to process
            var feedbackItems = new List<string>
            {
                "The product is amazing!",
                "Terrible service, very slow.",
                "It's okay, but could be better.",
                "Absolutely love the new features.",
                "Not worth the price."
            };

            Console.WriteLine($"\nProcessing {feedbackItems.Count} feedback items...\n");

            foreach (var item in feedbackItems)
            {
                try
                {
                    Console.WriteLine($"[Input] Processing: \"{item}\"");

                    // LAMBDA EXPRESSION USAGE:
                    // We pass a lambda expression to the ExecuteWithRetryAsync method.
                    // This lambda captures the 'item' variable and defines the specific API call.
                    string result = await handler.ExecuteWithRetryAsync(async () =>
                    {
                        // This code block is only executed when the delegate is invoked inside the handler
                        return await apiService.QueryModelAsync(item);
                    });

                    Console.WriteLine($"[Success] {result}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Failure] Failed to process item '{item}'. Final Error: {ex.GetType().Name}\n");
                }

                // Simulate a small gap between requests to avoid immediate rate limits in rapid succession
                await Task.Delay(100);
            }

            Console.WriteLine("--- Pipeline Execution Complete ---");
        }
    }
}
