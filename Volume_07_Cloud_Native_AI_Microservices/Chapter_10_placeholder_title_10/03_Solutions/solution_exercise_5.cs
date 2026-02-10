
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory; // For caching fallback

public class InferenceClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public InferenceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Define the policy
        _circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>() // Network errors
            .Or<TimeoutException>()         // Explicit timeouts
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429 is a failure
            .OrResult(msg => msg.StatusCode >= System.Net.HttpStatusCode.InternalServerError) // 5xx errors
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,    // Failure threshold
                durationOfBreak: TimeSpan.FromSeconds(30), // Open for 30s
                onBreak: (ex, breakDelay) => 
                {
                    // Log circuit open
                    Console.WriteLine($"Circuit OPENED for {breakDelay.TotalSeconds}s due to {ex.Exception?.Message}");
                },
                onReset: () => Console.WriteLine("Circuit CLOSED (Reset)")
            );
    }

    public async Task<string> GetInferenceAsync(string prompt)
    {
        // We wrap the execution in a fallback policy to handle the BrokenCircuitException
        // or to provide a default value if the circuit is open.
        var fallbackPolicy = Policy<HttpResponseMessage>
            .Handle<BrokenCircuitException>() // Triggered when circuit is open
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            .FallbackAsync(
                fallbackValue: new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
                { 
                    Content = new StringContent("{\"result\":\"Cached/Safe Default Response\"}") 
                },
                onFallbackAsync: (outcome, context) => 
                {
                    Console.WriteLine("Fallback triggered: Returning cached/default response.");
                    return Task.CompletedTask;
                }
            );

        // Combine policies: Fallback wraps the Circuit Breaker
        var policyWrap = fallbackPolicy.WrapAsync(_circuitBreakerPolicy);

        var response = await policyWrap.ExecuteAsync(async () => 
        {
            // Simulate the downstream call
            // In a real scenario, this is _httpClient.GetAsync(...)
            // To simulate failure for testing, we can throw exceptions randomly
            if (DateTime.Now.Ticks % 7 == 0) throw new HttpRequestException("Simulated Network Error");
            
            return await _httpClient.GetAsync($"/inference?prompt={prompt}");
        });

        return await response.Content.ReadAsStringAsync();
    }
}
