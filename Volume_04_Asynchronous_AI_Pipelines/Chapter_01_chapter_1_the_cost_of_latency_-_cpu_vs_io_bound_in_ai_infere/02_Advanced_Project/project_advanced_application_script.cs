
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
using System.Threading.Tasks;

namespace AsyncAIPipeline
{
    // Represents a request to an AI model (e.g., a prompt sent to a Large Language Model).
    public class AIRequest
    {
        public int Id { get; set; }
        public string Prompt { get; set; }
    }

    // Represents the response from the AI model.
    public class AIResponse
    {
        public int RequestId { get; set; }
        public string Result { get; set; }
        public long ProcessingTimeMs { get; set; }
    }

    public class AIService
    {
        // Simulates a network call to an AI inference endpoint.
        // This is an I/O-bound operation (waiting for network latency).
        // In a real scenario, this would be an HTTP POST request.
        public async Task<AIResponse> InferAsync(AIRequest request)
        {
            // Simulate network latency (e.g., 500ms to 1500ms).
            Random rnd = new Random();
            int latency = rnd.Next(500, 1500);
            await Task.Delay(latency);

            return new AIResponse
            {
                RequestId = request.Id,
                Result = $"Processed: {request.Prompt}",
                ProcessingTimeMs = latency
            };
        }

        // Synchronous version to demonstrate blocking behavior.
        // DO NOT use this in production for I/O operations.
        public AIResponse InferSync(AIRequest request)
        {
            Random rnd = new Random();
            int latency = rnd.Next(500, 1500);
            // This blocks the calling thread entirely.
            Task.Delay(latency).Wait(); 
            return new AIResponse
            {
                RequestId = request.Id,
                Result = $"Processed: {request.Prompt}",
                ProcessingTimeMs = latency
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== AI Inference Latency Analysis ===");
            Console.WriteLine("Scenario: Processing 5 concurrent AI prompts.\n");

            var requests = new List<AIRequest>
            {
                new AIRequest { Id = 1, Prompt = "Explain quantum computing." },
                new AIRequest { Id = 2, Prompt = "Write a Python hello world." },
                new AIRequest { Id = 3, Prompt = "Summarize 'The Odyssey'." },
                new AIRequest { Id = 4, Prompt = "Generate a poem about code." },
                new AIRequest { Id = 5, Prompt = "What is asynchronous programming?" }
            };

            var aiService = new AIService();

            // --- TEST 1: Synchronous Execution (Blocking) ---
            // This demonstrates the "Cost of Latency" where the CPU sits idle
            // waiting for I/O, degrading throughput.
            Console.WriteLine("--- 1. Synchronous Execution (Blocking) ---");
            Stopwatch syncWatch = Stopwatch.StartNew();
            
            List<AIResponse> syncResponses = new List<AIResponse>();
            foreach (var req in requests)
            {
                // The thread is blocked here until the network returns.
                // Total time = Sum of all individual latencies.
                var response = aiService.InferSync(req);
                syncResponses.Add(response);
                Console.WriteLine($"[Sync] Received Response for ID: {response.RequestId}");
            }
            
            syncWatch.Stop();
            Console.WriteLine($"Total Synchronous Time: {syncWatch.ElapsedMilliseconds}ms\n");

            // --- TEST 2: Asynchronous Execution (Non-Blocking) ---
            // This demonstrates solving the latency bottleneck using Async/Await.
            Console.WriteLine("--- 2. Asynchronous Execution (Non-Blocking) ---");
            Stopwatch asyncWatch = Stopwatch.StartNew();

            // Create a list of tasks representing the ongoing operations.
            var tasks = new List<Task<AIResponse>>();
            foreach (var req in requests)
            {
                // Fire off the request. The code does NOT wait here.
                // It immediately moves to the next iteration.
                tasks.Add(aiService.InferAsync(req));
            }

            // Asynchronously wait for ALL tasks to complete.
            // The thread is not blocked; it can handle other work if available.
            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                var response = task.Result;
                Console.WriteLine($"[Async] Received Response for ID: {response.RequestId}");
            }

            asyncWatch.Stop();
            Console.WriteLine($"Total Asynchronous Time: {asyncWatch.ElapsedMilliseconds}ms");

            // Analysis
            Console.WriteLine("\n--- Analysis ---");
            Console.WriteLine("In the synchronous example, the total time is roughly the sum of all requests (e.g., 5s).");
            Console.WriteLine("In the asynchronous example, the total time is roughly the duration of the longest single request (e.g., 1.5s).");
            Console.WriteLine("This demonstrates how async/await maximizes CPU utilization for I/O-bound tasks.");
        }
    }
}
