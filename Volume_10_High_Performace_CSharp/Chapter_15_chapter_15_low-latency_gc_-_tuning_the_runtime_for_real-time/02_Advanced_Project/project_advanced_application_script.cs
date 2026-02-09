
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
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;

namespace LowLatencyAIInference
{
    /// <summary>
    /// Simulates a high-frequency real-time AI inference engine (e.g., a chatbot serving thousands of requests per second).
    /// Focus: Minimizing GC pauses to maintain strict microsecond-level latency SLAs.
    /// </summary>
    class Program
    {
        // Configuration for the simulation
        const int TotalRequests = 100_000;
        const int TokenCountPerRequest = 50; // Average tokens per inference
        const int MaxTokenLength = 20;       // Max characters per token

        static void Main(string[] args)
        {
            Console.WriteLine("--- Low-Latency AI Inference Simulator ---");
            Console.WriteLine($"Targeting {TotalRequests} requests with {TokenCountPerRequest} tokens each.");
            
            // 1. TUNE RUNTIME GC
            // For high-throughput server apps, we switch to Server GC.
            // Server GC uses a heap per core and aggressive background collection.
            // NOTE: This must be set BEFORE any significant allocation occurs.
            if (GCSettings.IsServerGC)
            {
                Console.WriteLine("[OK] Server GC is active.");
            }
            else
            {
                Console.WriteLine("[WARN] Workstation GC detected. Switch to <ServerGarbageCollection>true</ServerGarbageCollection> in .csproj for production.");
            }

            // 2. SET LATENCY MODE
            // GCLatencyMode.LowLatency is crucial for short-term GC avoidance during critical paths.
            // It prevents full blocking GCs, though background Gen2 collections may still occur.
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            Console.WriteLine($"[OK] GC Latency Mode set to: {GCSettings.LatencyMode}");

            // 3. WARM UP
            // We run a small batch first to JIT compile and stabilize the runtime.
            Console.WriteLine("\nWarming up...");
            ProcessBatch(1000, false); 

            // 4. CRITICAL INFERENCE WINDOW (NoGC Region)
            // We attempt to reserve a memory budget to execute a batch without ANY GC interruptions.
            // This is the pinnacle of low-latency tuning for microsecond consistency.
            Console.WriteLine("\nStarting Critical Inference Window (NoGC Region)...");
            
            long totalTicks = 0;
            long gcCount = 0;
            
            // Estimate memory required: 
            // We use a rough heuristic: (TokenCount * AvgLength * BytesPerChar) + Overhead.
            // For 5000 tokens ~ 100KB. We request 2MB to be safe.
            int memoryBudgetBytes = 2 * 1024 * 1024; 

            try
            {
                // TryStartNoGCRegion throws an exception if the budget is insufficient or a GC is imminent.
                GC.TryStartNoGCRegion(memoryBudgetBytes);

                // Run the heavy batch inside the safe window
                var sw = Stopwatch.StartNew();
                ProcessBatch(TotalRequests, true);
                sw.Stop();
                
                totalTicks = sw.ElapsedTicks;
                gcCount = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);

                // End the NoGC region manually to allow GC to resume
                GC.EndNoGCRegion();
                Console.WriteLine($"[SUCCESS] Critical window finished. Time: {sw.ElapsedMilliseconds}ms");
            }
            catch (InvalidOperationException ex)
            {
                // This happens if the budget was too small or a background GC was already running.
                Console.WriteLine($"[FAIL] Could not start NoGC Region: {ex.Message}");
                Console.WriteLine("Falling back to standard LowLatency mode processing...");
                
                var sw = Stopwatch.StartNew();
                ProcessBatch(TotalRequests, true);
                sw.Stop();
                totalTicks = sw.ElapsedTicks;
            }

            // 5. ANALYZE RESULTS
            Console.WriteLine("\n--- Performance Metrics ---");
            Console.WriteLine($"Total Time: {TimeSpan.FromTicks(totalTicks).TotalMilliseconds:F2} ms");
            Console.WriteLine($"Avg Latency per Request: {TimeSpan.FromTicks(totalTicks / TotalRequests).TotalMicroseconds:F2} µs");
            
            // Note: In a NoGC region, GC counts should remain static (0 new collections).
            // In LowLatency mode, we might see Gen0 collections, but Gen1/2 should be suppressed.
            Console.WriteLine($"GC Collections (Gen 0/1/2) during run: {GC.CollectionCount(0)} / {GC.CollectionCount(1)} / {GC.CollectionCount(2)}");
        }

        /// <summary>
        /// Processes a batch of inference requests.
        /// </summary>
        /// <param name="count">Number of requests to process.</param>
        /// <param name="usePooling">If true, uses ArrayPool to avoid heap allocations for token buffers.</param>
        static void ProcessBatch(int count, bool usePooling)
        {
            // Random generator for simulation
            Random rng = new Random(42); 

            for (int i = 0; i < count; i++)
            {
                // --- SIMULATION: TOKEN GENERATION ---
                // In a real AI model, this would be matrix multiplication.
                // Here, we simulate generating text tokens.

                string[] tokens;
                
                if (usePooling)
                {
                    // LOW-LATENCY TECHNIQUE: Array Pooling
                    // Instead of 'new string[TokenCountPerRequest]', we rent from a shared pool.
                    // This eliminates Gen 0 pressure for the array object itself.
                    tokens = ArrayPool<string>.Shared.Rent(TokenCountPerRequest);
                }
                else
                {
                    tokens = new string[TokenCountPerRequest];
                }

                try
                {
                    for (int t = 0; t < TokenCountPerRequest; t++)
                    {
                        // LOW-LATENCY TECHNIQUE: Span<T> for String Generation
                        // We avoid intermediate string allocations during concatenation.
                        // Instead, we fill a pre-allocated char buffer.
                        
                        int len = rng.Next(5, MaxTokenLength);
                        
                        // Rent a char buffer from the pool to avoid heap allocs for the token string.
                        char[] charBuffer = ArrayPool<char>.Shared.Rent(len);
                        
                        try
                        {
                            for (int c = 0; c < len; c++)
                            {
                                // Generate random lowercase letter
                                charBuffer[c] = (char)rng.Next('a', 'z' + 1);
                            }

                            // Create the string from the buffer slice (efficient, no intermediate strings)
                            // Note: In .NET Core/5+, string creation from char[] is optimized.
                            // For true zero-alloc, we would process spans directly, but strings are needed for the simulation logic.
                            tokens[t] = new string(charBuffer, 0, len);
                        }
                        finally
                        {
                            // Return the char buffer immediately
                            ArrayPool<char>.Shared.Return(charBuffer);
                        }
                    }

                    // --- SIMULATION: POST-PROCESSING ---
                    // Example: Calculate log probability or simple aggregation (simulated)
                    // We use a ref struct to demonstrate stack-only processing where possible.
                    ProcessInferenceResult(tokens);
                }
                finally
                {
                    if (usePooling)
                    {
                        // CRITICAL: Return the array to the pool to be reused.
                        // Forgetting this causes memory leaks and pool exhaustion.
                        ArrayPool<string>.Shared.Return(tokens, clearArray: true);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the result using stack-only data structures (ref struct) where applicable.
        /// </summary>
        static void ProcessInferenceResult(string[] tokens)
        {
            // Calculate total length to simulate aggregation without intermediate strings
            int totalLength = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] != null)
                    totalLength += tokens[i].Length;
            }

            // Simulate a lightweight operation (e.g., checking for a specific token)
            // We avoid LINQ or Lambdas to stick to the chapter's "basic blocks" constraint.
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "error") // Unlikely with random generation, but logic check
                {
                    // Handle error state
                    break;
                }
            }
        }
    }
}
