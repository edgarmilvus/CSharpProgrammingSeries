
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

namespace AsyncStateMachineDemo
{
    // Represents a request to an AI model (e.g., GPT-4)
    public class AIRequest
    {
        public string Id { get; set; }
        public string Input { get; set; }
        public string? Result { get; set; }
    }

    class Program
    {
        // Simulates a high-latency external API call (e.g., OpenAI API)
        // This method represents the "I/O Bound" work that allows the thread to be released.
        static async Task<string> CallExternalAIAsync(string prompt)
        {
            // 1. PENDING STATE: The method is called, but execution hasn't yielded yet.
            // We simulate network latency.
            await Task.Delay(2000); 

            // 2. SUSPENDED STATE: When 'await' is hit, the method pauses.
            // The compiler generates a state machine. The Task is returned to the caller,
            // and the thread is freed up to do other work (like handling other requests).

            // 3. RESUMED STATE: Once the Task.Delay completes, the scheduler picks this up.
            // It restores the context (local variables, stack) and continues execution here.
            return $"[Processed]: {prompt} (Generated at {DateTime.Now:HH:mm:ss})";
        }

        // Simulates a local CPU-intensive task (e.g., parsing JSON or formatting text)
        // This method represents "CPU Bound" work.
        static string ProcessLocally(string data)
        {
            // Simulate CPU work (blocking the thread intentionally for demo purposes)
            Thread.Sleep(500);
            return data.ToUpper();
        }

        // The core orchestrator for our AI pipeline
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AI Pipeline State Machine Demo ===");
            Console.WriteLine("Starting requests on Thread ID: " + Thread.CurrentThread.ManagedThreadId);

            var requests = new List<AIRequest>
            {
                new AIRequest { Id = "REQ-001", Input = "Summarize quantum physics" },
                new AIRequest { Id = "REQ-002", Input = "Explain photosynthesis" },
                new AIRequest { Id = "REQ-003", Input = "Write a haiku about code" }
            };

            // We create a list to hold the asynchronous tasks.
            // At this point, the tasks are created but NOT started (Lazy evaluation).
            var tasks = new List<Task<string>>();

            // ---------------------------------------------------------
            // CONCURRENCY PATTERN: Fan-out
            // ---------------------------------------------------------
            // We iterate through the requests and start the async operations.
            // This mimics a web server handling multiple incoming connections.
            foreach (var req in requests)
            {
                // Calling CallExternalAIAsync returns a Task immediately (Pending/Suspended).
                // It does NOT block here. The loop continues instantly.
                tasks.Add(CallExternalAIAsync(req.Input));
            }

            Console.WriteLine($"All {tasks.Count} requests dispatched. Main thread is free.");

            // ---------------------------------------------------------
            // CONCURRENCY PATTERN: Fan-in (Aggregation)
            // ---------------------------------------------------------
            // Task.WhenAll waits for all state machines to reach their 'Completed' state.
            // Crucially, this await yields control back to the event loop.
            string[] results = await Task.WhenAll(tasks);

            // ---------------------------------------------------------
            // PROCESSING RESULTS
            // ---------------------------------------------------------
            Console.WriteLine("\n--- Batch Results ---");
            for (int i = 0; i < results.Length; i++)
            {
                // Synchronous processing on the result (CPU bound)
                string finalOutput = ProcessLocally(results[i]);
                Console.WriteLine($"Request {i + 1}: {finalOutput}");
            }

            Console.WriteLine("\nPipeline Complete.");
        }
    }
}
