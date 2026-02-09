
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Generic;

namespace ResilientAIClient
{
    // REAL-WORLD CONTEXT:
    // An internal dashboard application aggregates data from multiple external AI providers (e.g., for sentiment analysis).
    // If one provider (e.g., Provider A) is experiencing a temporary outage, the application should:
    // 1. Retry transient failures (network blips).
    // 2. Stop hammering Provider A if it's consistently failing (Circuit Breaker).
    // 3. Fail fast to maintain dashboard responsiveness during prolonged outages.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Resilient AI Data Aggregator...\n");

            // 1. SETUP: Create a mock HTTP Client that simulates real-world network conditions.
            // In a real app, this would be an HttpClient pointing to an actual API.
            var mockClient = new HttpClient(new MockNetworkHandler());

            // 2. POLICY DEFINITION: Define resilience strategies.
            
            // Strategy A: Retry Policy
            // Handles transient faults (e.g., 503 Service Unavailable, timeouts).
            // Uses exponential backoff: waits 2s, 4s, 8s before retrying.
            var retryPolicy = Policy
                .Handle<HttpRequestException>() // Catch network errors
                .OrResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"[Retry] Attempt {retryCount}. Waiting {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    });

            // Strategy B: Circuit Breaker Policy
            // Monitors failures. If 5 consecutive failures occur, the circuit "opens".
            // Subsequent calls fail immediately without trying the network.
            // After 10 seconds, the circuit "half-opens" to test if the service is back.
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (exception, breakDelay) =>
                    {
                        Console.WriteLine($"[Circuit Breaker] OPENED for {breakDelay.TotalSeconds}s. Service is down.");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("[Circuit Breaker] CLOSED. Service recovered.");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("[Circuit Breaker] HALF-OPEN. Testing service...");
                    });

            // 3. EXECUTION: Wrap the execution logic in the policies.
            // We combine them: Retry inside Circuit Breaker (or vice versa, depending on desired behavior).
            // Here: The Retry policy is the inner execution, protected by the outer Circuit Breaker.
            var resilientPipeline = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);

            // Simulate a burst of requests (e.g., user refreshing dashboard multiple times)
            for (int i = 1; i <= 15; i++)
            {
                try
                {
                    Console.WriteLine($"\n--- Request #{i} ---");

                    // Execute the request through the resilience pipeline
                    var result = await resilientPipeline.ExecuteAsync(async () =>
                    {
                        // Simulate calling the external AI service
                        var response = await mockClient.GetAsync("https://api.mock-ai-provider.com/v1/sentiment");
                        return response;
                    });

                    if (result.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"SUCCESS: Received data.");
                    }
                    else
                    {
                        Console.WriteLine($"FAILURE: Status {result.StatusCode}");
                    }
                }
                catch (BrokenCircuitException)
                {
                    // Specific exception thrown by Polly when Circuit is Open
                    Console.WriteLine("FAST FAIL: Circuit is open. Request aborted immediately to save resources.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CRITICAL ERROR: {ex.GetType().Name}");
                }

                // Small delay between requests to visualize the flow
                Thread.Sleep(1000);
            }
        }
    }

    // MOCK NETWORK HANDLER:
    // Simulates an external AI API that fails intermittently, then fails consistently (outage), then recovers.
    public class MockNetworkHandler : HttpMessageHandler
    {
        private int _requestCount = 0;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestCount++;

            // Phase 1: Transient Failures (First 3 requests)
            // Simulates a blip. Polly should retry and eventually succeed.
            if (_requestCount <= 3)
            {
                // 50% chance of failure to simulate randomness
                if (new Random().Next(0, 2) == 0)
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable));
                }
            }

            // Phase 2: Prolonged Outage (Requests 4 to 10)
            // Simulates a total crash. Polly will fail fast after Circuit Breaker opens.
            if (_requestCount > 3 && _requestCount <= 10)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            }

            // Phase 3: Recovery (Requests 11+)
            // Service comes back online.
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
