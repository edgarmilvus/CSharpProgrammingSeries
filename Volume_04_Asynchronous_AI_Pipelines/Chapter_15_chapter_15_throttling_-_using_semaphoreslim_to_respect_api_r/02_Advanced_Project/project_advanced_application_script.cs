
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
using System.Threading;
using System.Threading.Tasks;

// Problem: A sentiment analysis service needs to process 100 user reviews.
// The external AI API (simulated here) has a strict rate limit of 5 concurrent requests.
// Without throttling, the application would spawn 100 tasks immediately, causing
// HTTP 429 (Too Many Requests) errors or IP bans.

namespace ThrottledSentimentAnalysis
{
    // 1. Custom DelegatingHandler for centralized rate limiting.
    // This adheres to the "Cross-cutting concern" architectural pattern.
    public class RateLimitHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _semaphore;

        public RateLimitHandler(int maxConcurrency)
        {
            // Initialize the semaphore with the max number of concurrent slots.
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            InnerHandler = new HttpClientHandler(); // Standard HTTP handler
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Wait asynchronously for a slot to open up in the semaphore.
            // This blocks the request thread but frees up the CPU.
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                // Execute the actual HTTP request.
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                // ALWAYS release the semaphore slot in a finally block
                // to prevent deadlocks if the request fails.
                _semaphore.Release();
            }
        }
    }

    class Program
    {
        // Simulated API endpoint (using a mock service for demonstration).
        private const string ApiUrl = "https://api.mock-ai.com/v1/sentiment";
        
        // Configuration: We are allowed only 5 concurrent calls.
        private const int MaxConcurrentRequests = 5;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Batch Sentiment Analysis...");
            Console.WriteLine($"Rate Limit: {MaxConcurrentRequests} concurrent requests.");
            Console.WriteLine("--------------------------------------------------");

            // 2. Instantiate the HttpClient with our custom RateLimitHandler.
            // We wrap this in a 'using' statement to ensure proper disposal of resources.
            using var httpClient = new HttpClient(new RateLimitHandler(MaxConcurrentRequests));
            
            // 3. Prepare the data batch (100 reviews).
            List<Task> processingTasks = new List<Task>();
            int totalReviews = 100;

            // 4. Launch tasks for processing.
            // We use a standard 'for' loop here. No LINQ or Parallel.ForEach.
            for (int i = 1; i <= totalReviews; i++)
            {
                // Capture the current loop variable to avoid closure issues in async context.
                int reviewId = i;

                // Create the task but do NOT await it immediately.
                // If we awaited inside the loop, we would process sequentially (1 by 1).
                Task task = ProcessReviewAsync(httpClient, reviewId);
                processingTasks.Add(task);
            }

            Console.WriteLine($"Launched {totalReviews} tasks. Waiting for completion...");
            
            // 5. Parallel Execution via Task.WhenAll.
            // This waits for ALL tasks in the list to complete.
            // The SemaphoreSlim inside the handler ensures that no more than 
            // 5 requests are active at any specific moment.
            await Task.WhenAll(processingTasks);

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Batch processing complete. All reviews analyzed.");
        }

        // Helper method to simulate fetching sentiment for a single review.
        static async Task ProcessReviewAsync(HttpClient client, int id)
        {
            try
            {
                // Simulate the payload.
                var payload = $"{{ \"reviewId\": {id}, \"text\": \"Review content {id}\" }}";
                
                // Create the HTTP request.
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                
                // In a real scenario, we would post to the API. 
                // Here we simulate the network delay and concurrency control.
                // The 'await client.PostAsync' is where the SemaphoreSlim.WaitAsync() is triggered.
                var response = await client.PostAsync(ApiUrl, content);
                
                // Simulate processing time (e.g., parsing JSON response).
                // We use Task.Delay to mimic the CPU/IO work of handling the response.
                await Task.Delay(100); 

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Success] Review #{id} processed.");
                }
                else
                {
                    Console.WriteLine($"[Error] Review #{id} failed with status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] Review #{id}: {ex.Message}");
            }
        }
    }

    // 6. Mock Class for Demonstration purposes only.
    // Since we cannot hit a real external API in this static code block,
    // we override the SendAsync behavior locally if needed, or rely on the 
    // logic flow. In a real app, the RateLimitHandler would intercept 
    // actual network calls.
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Simulate network latency (random between 50ms and 200ms).
            await Task.Delay(new Random().Next(50, 200), cancellationToken);
            
            // Return a success status code.
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}
