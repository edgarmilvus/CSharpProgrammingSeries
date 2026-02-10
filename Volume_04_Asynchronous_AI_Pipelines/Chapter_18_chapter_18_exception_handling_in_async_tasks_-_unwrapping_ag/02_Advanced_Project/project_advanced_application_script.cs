
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
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAiPipelines
{
    class Program
    {
        // REAL-WORLD CONTEXT:
        // An AI Content Moderation System for a social media platform.
        // The system receives a batch of user posts and must run three parallel checks:
        // 1. Sentiment Analysis (LLM)
        // 2. Toxicity Detection (LLM)
        // 3. PII (Personal Identifiable Information) Scanning (LLM)
        // If ANY of these checks fail (e.g., API timeout, model error), we need to log the specific failure
        // without crashing the entire pipeline, ensuring the platform remains responsive.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Async AI Content Moderation Pipeline...\n");

            // Simulate a batch of 3 user posts to process concurrently.
            // We use a List to hold the tasks, representing the concurrent operations.
            List<Task<ModerationResult>> moderationTasks = new List<Task<ModerationResult>>();

            string[] posts = new string[]
            {
                "Post 1: I love this product! It's amazing.",
                "Post 2: This is the worst service ever. I hate it.",
                "Post 3: Contact me at 555-0199 for details." // Simulated PII
            };

            // 1. INITIATION:
            // We start all tasks asynchronously. We do not await them immediately here.
            // This allows all three AI models to run in parallel (Parallelism).
            foreach (var post in posts)
            {
                moderationTasks.Add(RunFullModerationCheck(post));
            }

            // 2. CONCURRENCY & EXCEPTION HANDLING:
            // We use Task.WhenAll to wait for all tasks to complete OR fail.
            // This is the critical point where AggregateException is often thrown if multiple tasks fail.
            try
            {
                // Await the completion of all concurrent tasks.
                ModerationResult[] results = await Task.WhenAll(moderationTasks);

                Console.WriteLine("\n--- Batch Processing Complete (Success) ---");
                foreach (var result in results)
                {
                    Console.WriteLine($"[ID: {result.PostId}] Status: {result.Status} | Flags: {string.Join(", ", result.Flags)}");
                }
            }
            catch (Exception ex)
            {
                // 3. UNWRAPPING AGGREGATE EXCEPTION:
                // When using Task.WhenAll, the caught exception is often an AggregateException.
                // However, when awaited directly, .NET unwraps it to the first exception by default.
                // To handle ALL concurrent failures, we must inspect the inner exceptions carefully.
                Console.WriteLine("\n--- Pipeline Error Detected ---");
                HandlePipelineFailure(ex, moderationTasks);
            }

            Console.WriteLine("\nPipeline execution finished.");
        }

        // SIMULATION: Represents a call to an external AI API (e.g., Azure OpenAI, AWS Bedrock)
        // This method simulates random failures (network timeouts, rate limits) to demonstrate exception handling.
        static async Task<ModerationResult> RunFullModerationCheck(string postContent)
        {
            // Generate a unique ID for the post for tracking.
            string postId = Guid.NewGuid().ToString().Substring(0, 8);
            var result = new ModerationResult { PostId = postId, Content = postContent };

            // Simulate processing time (network latency)
            await Task.Delay(new Random().Next(100, 500));

            // SIMULATION LOGIC:
            // Randomly throw exceptions to test the robustness of the catch block.
            int randomizer = new Random().Next(1, 10);
            
            if (randomizer == 2)
            {
                // Simulate a specific API failure
                throw new HttpRequestException($"API Timeout for Post {postId}: Sentiment analysis service unavailable.");
            }
            else if (randomizer == 5)
            {
                // Simulate a model specific error
                throw new InvalidOperationException($"Model Error for Post {postId}: Invalid input format.");
            }
            else if (randomizer == 8)
            {
                // Simulate a generic network error
                throw new TimeoutException($"Network Error for Post {postId}: Request took too long.");
            }

            // If no exception, populate result based on content (mock logic)
            if (postContent.Contains("hate") || postContent.Contains("worst"))
                result.Flags.Add("Toxic");
            if (postContent.Contains("555"))
                result.Flags.Add("PII");
            
            result.Status = "Processed";
            return result;
        }

        // 4. RESILIENT LOGGING & RECOVERY:
        // A dedicated method to handle the complexity of unwrapping exceptions.
        // This ensures the main loop remains clean and logic is isolated.
        static void HandlePipelineFailure(Exception caughtException, List<Task<ModerationResult>> tasks)
        {
            // We iterate through the tasks to find which ones failed.
            // We do not rely solely on the caught exception object because
            // Task.WhenAll aggregates failures inside the Task objects themselves.
            int failureCount = 0;

            foreach (var task in tasks)
            {
                // Check if the task completed with a faulted state
                if (task.IsFaulted)
                {
                    failureCount++;
                    
                    // Access the Exception property of the faulted task.
                    // This contains the specific exception thrown for this specific AI call.
                    Exception innerEx = task.Exception;

                    // We can inspect the type of the exception to decide on recovery logic.
                    if (innerEx is HttpRequestException)
                    {
                        Console.WriteLine($"[CRITICAL] API Failure: {innerEx.Message}");
                        // Recovery strategy: Queue for retry with exponential backoff.
                    }
                    else if (innerEx is TimeoutException)
                    {
                        Console.WriteLine($"[WARNING] Network Timeout: {innerEx.Message}");
                        // Recovery strategy: Log to dead-letter queue for manual review.
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] General Failure: {innerEx.Message}");
                    }
                }
            }

            Console.WriteLine($"\nSummary: {failureCount} out of {tasks.Count} AI checks failed.");
            Console.WriteLine("Action: Successful posts are saved; Failed posts are flagged for retry.");
        }
    }

    // Basic DTO (Data Transfer Object) to hold the result of the AI analysis.
    // Kept simple (no Records or advanced features) as per constraints.
    class ModerationResult
    {
        public string PostId { get; set; }
        public string Content { get; set; }
        public string Status { get; set; } = "Pending";
        public List<string> Flags { get; set; } = new List<string>();
    }
}
