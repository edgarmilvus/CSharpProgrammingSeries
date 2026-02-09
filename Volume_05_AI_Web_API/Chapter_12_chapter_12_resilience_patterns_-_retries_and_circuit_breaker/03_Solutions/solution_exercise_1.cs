
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Polly;
using Polly.Retry;
using System.Net;

namespace ResilientAiClient
{
    // 1. Define the interface
    public interface ILlmService
    {
        Task<string> GetCompletionAsync(string prompt);
    }

    // 2. Implement the service with injected Retry Policy
    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public LlmService(HttpClient httpClient, AsyncRetryPolicy retryPolicy)
        {
            _httpClient = httpClient;
            _retryPolicy = retryPolicy;
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            // Wrap the execution in the policy
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                // Simulate calling the external API
                // Using httpstat.us to simulate specific status codes
                var response = await _httpClient.GetAsync($"https://httpstat.us/503");
                
                if (!response.IsSuccessStatusCode)
                {
                    // Throw exception to trigger retry logic
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                return $"Response for prompt: {prompt}";
            });
        }
    }

    // 3. Main Program to configure and run the simulation
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Resilient AI Client Starting...");

            // Configure the Retry Policy (Requirement 5, 6, 7, 8)
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Handle specific status codes if returning HttpResponseMessage
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(attempt), // Linear backoff: 1s, 2s, 3s
                    onRetry: (exception, delay, attempt, context) =>
                    {
                        Console.WriteLine($"Attempt {attempt} failed. Retrying in {delay.TotalSeconds}s...");
                    });

            // Setup Dependency Injection (Simulated for Console App)
            // Requirement 9: Inject policy via constructor
            var services = new ServiceCollection();
            services.AddHttpClient<ILlmService, LlmService>(client => 
            {
                // Base address setup if needed
            })
            .AddTypedClient<ILlmService>((client, serviceProvider) => 
                new LlmService(client, retryPolicy));

            var serviceProvider = services.BuildServiceProvider();
            var llmService = serviceProvider.GetRequiredService<ILlmService>();

            // Requirement 10: Simulate failure
            try
            {
                // Note: httpstat.us/503 simulates a 503 error. 
                // Since the policy retries 3 times, this request will fail 3 times and then throw.
                // To see a success, you would need a mock that fails twice then succeeds.
                // For this exercise, we demonstrate the retry logic on failure.
                var result = await llmService.GetCompletionAsync("Hello AI");
                Console.WriteLine($"Success: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Final Error after retries: {ex.Message}");
            }
        }
    }
}
