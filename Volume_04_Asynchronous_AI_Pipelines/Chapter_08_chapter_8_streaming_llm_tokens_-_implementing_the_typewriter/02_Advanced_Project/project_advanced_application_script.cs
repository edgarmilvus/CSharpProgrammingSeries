
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
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStreamingLLM
{
    /// <summary>
    /// Real-World Problem Context:
    /// A software development team is building a "Smart Terminal" for their internal CLI tool.
    /// Developers frequently ask the AI to generate boilerplate code, SQL queries, or regex patterns.
    /// 
    /// Problem: When the AI generates a large block of code, waiting 10-20 seconds for the full response
    /// feels broken or frozen to the user. The user loses focus and thinks the app crashed.
    /// 
    /// Solution: Implement a "Typewriter Effect" using asynchronous streaming. We will simulate an LLM
    /// API that sends tokens (words) one by one, and the client application will render them instantly
    /// as they arrive, maintaining high UI responsiveness and perceived performance.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Smart Terminal: Async LLM Stream Processor ---\n");

            // 1. Initialize the simulated LLM API Client
            var llmClient = new SimulatedLLMClient();

            // 2. Define a prompt (e.g., asking for a C# method)
            string userPrompt = "Write a C# method to calculate Fibonacci numbers efficiently.";

            Console.WriteLine($"[User]: {userPrompt}\n");
            Console.WriteLine("[AI]: ");

            // 3. Start the stopwatch to measure streaming latency
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 4. Initiate the stream (Async/Await pattern)
            // We use 'await foreach' to iterate over the stream as tokens arrive.
            // This keeps the Main thread responsive without blocking.
            try
            {
                await foreach (var token in llmClient.GetStreamingResponseAsync(userPrompt))
                {
                    // 5. Render the token immediately
                    // This creates the "Typewriter Effect"
                    Console.Write(token);
                    
                    // Optional: Simulate a tiny delay to mimic human typing speed visually
                    // In a real UI (WPF/WinUI), this is not needed as rendering is vsync'd.
                    await Task.Delay(30); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error]: Connection lost. {ex.Message}");
            }

            stopwatch.Stop();
            
            Console.WriteLine($"\n\n--- Stream Complete ---");
            Console.WriteLine($"Total time elapsed: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// ARCHITECTURE BREAKDOWN:
    /// 
    /// 1. The Problem of Latency:
    ///    Traditional API calls are "Atomic" - you send a request and wait for the full response.
    ///    Large Language Models (LLMs) generate text sequentially (token by token).
    ///    If we wait for the full generation, we introduce high latency.
    /// 
    /// 2. The Solution: IAsyncEnumerable (C# 8.0+)
    ///    This interface allows a method to yield results incrementally.
    ///    Instead of returning `Task<string>` (the whole blob), it returns `IAsyncEnumerable<string>` (stream).
    /// 
    /// 3. The Mechanism: Server-Sent Events (SSE) Simulation
    ///    Real-world LLM APIs (like OpenAI) use HTTP Streaming. They keep the connection open and 
    ///    send chunks of data as they are generated.
    ///    Our `SimulatedLLMClient` mimics this by using `Channel<T>` or `yield return` to push data 
    ///    asynchronously to the consumer.
    /// 
    /// 4. The Consumer: await foreach
    ///    This syntax sugar allows the application to "pause" execution at the `yield` point of the producer,
    ///    process the token, and then automatically resume when the next token is ready.
    /// </summary>

    // ---------------------------------------------------------
    // SIMULATED LLM BACKEND
    // ---------------------------------------------------------
    public class SimulatedLLMClient
    {
        private readonly HttpClient _httpClient; // Placeholder, not used in simulation but represents real architecture

        public SimulatedLLMClient()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Simulates a real LLM API endpoint that streams JSON fragments.
        /// Returns an IAsyncEnumerable to allow token-by-token processing.
        /// </summary>
        public async IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt)
        {
            // 1. Define the "Backend" logic.
            // In a real app, this would loop over an HTTP Response Stream.
            // Here, we simulate the network delay and chunk generation.
            
            string[] fullResponseTokens = GenerateMockLLMResponse(prompt);

            foreach (var token in fullResponseTokens)
            {
                // 2. Simulate Network Latency (Time for LLM to generate next token)
                // Randomized to show variable throughput.
                int delay = new Random().Next(50, 150); 
                await Task.Delay(delay);

                // 3. Yield the token back to the caller (The 'Typewriter' logic)
                // This resumes the 'await foreach' loop in the Main method.
                yield return token;
            }
        }

        /// <summary>
        /// Helper to generate a realistic code snippet token-by-token.
        /// </summary>
        private string[] GenerateMockLLMResponse(string prompt)
        {
            // We construct a code block based on the prompt context
            if (prompt.Contains("Fibonacci"))
            {
                return new string[] {
                    "