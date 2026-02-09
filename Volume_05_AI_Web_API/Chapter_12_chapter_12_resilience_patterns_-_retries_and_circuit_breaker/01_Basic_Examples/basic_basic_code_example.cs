
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

using Polly;
using Polly.Retry;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net;

namespace PollyRetryDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup a mock handler that simulates failure
            var mockHandler = new SimulatedFailureHandler();
            var client = new HttpClient(mockHandler);

            // 2. Define the Retry Policy with Exponential Backoff
            // We will try up to 3 times.
            // Wait times: 2s, 4s, 8s (Exponential)
            AsyncRetryPolicy retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"[Retry] Attempt {retryCount}: Waiting {timespan.TotalSeconds}s due to {exception.Message}");
                    });

            Console.WriteLine("--- Starting Request Execution ---");

            try
            {
                // 3. Wrap the execution inside the policy
                await retryPolicy.ExecuteAsync(async () =>
                {
                    Console.WriteLine("Executing HTTP Request...");
                    // This will hit our mock handler which simulates failures
                    var response = await client.GetAsync("https://api.mock-weather.com/data");
                    response.EnsureSuccessStatusCode();
                    return response;
                });

                Console.WriteLine("SUCCESS: Data retrieved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILURE: All retries exhausted. Final Error: {ex.Message}");
            }

            Console.WriteLine("\n--- Execution Finished ---");
        }
    }

    // --- Mock Infrastructure to simulate the scenario ---

    /// <summary>
    /// A custom HttpMessageHandler that simulates a flaky external API.
    /// It fails the first 3 requests and succeeds on the 4th.
    /// </summary>
    public class SimulatedFailureHandler : HttpMessageHandler
    {
        private int _requestCount = 0;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestCount++;
            Console.WriteLine($"   [Server Log] Received request #{_requestCount}");

            // Simulate network latency
            await Task.Delay(500, cancellationToken);

            if (_requestCount <= 3)
            {
                // Simulate a transient server error (503 Service Unavailable)
                throw new HttpRequestException("Simulated transient network error");
            }

            // Success on the 4th attempt
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Temperature: 22°C")
            };
        }
    }
}
