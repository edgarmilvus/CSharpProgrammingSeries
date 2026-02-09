
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

using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LowLatencyGcDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Initial GC Mode: {GCSettings.IsServerGC} (Server: true, Workstation: false)");
            Console.WriteLine($"Current Latency Mode: {GCSettings.LatencyMode}");
            Console.WriteLine(new string('-', 50));

            // 1. Simulate a standard workload (High Allocation)
            await SimulateStandardInference();

            Console.WriteLine(new string('-', 50));

            // 2. Simulate a real-time workload (Low Allocation + NoGC Region)
            await SimulateRealTimeInference();
        }

        /// <summary>
        /// Simulates a standard inference step that relies on the Garbage Collector.
        /// This is typical for prototyping or non-critical batch processing.
        /// </summary>
        static async Task SimulateStandardInference()
        {
            Console.WriteLine("Starting Standard Inference (Allocating Heavy)...");
            
            // Force a GC to start from a clean slate
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long memStart = GC.GetTotalMemory(false);

            var sw = Stopwatch.StartNew();

            // PROCESS: Tokenization and processing
            // We simulate processing 1000 tokens. 
            // In a naive implementation, we might allocate a new string for every token transformation.
            for (int i = 0; i < 1000; i++)
            {
                // BAD PRACTICE: Allocating on the heap for every token.
                // In a real AI model, this could be tensor slices or string manipulations.
                string token = $"Token_{i}_Processed";
                
                // Simulate some CPU work
                await Task.Delay(1); 
            }

            sw.Stop();
            long memEnd = GC.GetTotalMemory(false); // This might trigger a Gen0 collection

            Console.WriteLine($"Standard Inference Complete.");
            Console.WriteLine($"Time Elapsed: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Memory Allocated: ~{(memEnd - memStart) / 1024.0:F2} KB");
            
            // Check how many GC collections occurred in Gen0 during this process
            Console.WriteLine($"Gen0 Collections: {GC.CollectionCount(0)}");
        }

        /// <summary>
        /// Simulates a real-time inference step optimized for low latency.
        /// Uses ArrayPool and TryStartNoGCRegion to prevent pauses.
        /// </summary>
        static async Task SimulateRealTimeInference()
        {
            Console.WriteLine("Starting Real-Time Inference (Optimized)...");
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long memStart = GC.GetTotalMemory(false);

            // CRITICAL: Attempt to enter a NoGC region.
            // If the heap is too fragmented or we request too much memory, this returns false.
            // We wrap it in a try-finally to ensure we exit the region.
            bool noGCRegionEntered = false;
            try
            {
                // Request a budget: 1MB of memory that is guaranteed not to be collected.
                // We estimate this is enough for our token buffers.
                if (GC.TryStartNoGCRegion(1 * 1024 * 1024))
                {
                    noGCRegionEntered = true;
                    Console.WriteLine("Entered NoGC Region successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to enter NoGC Region (heap pressure). Falling back to standard mode.");
                }

                var sw = Stopwatch.StartNew();

                // PROCESS: Tokenization using ArrayPool
                // We use a shared array pool to avoid heap allocations for token buffers.
                var pool = ArrayPool<byte>.Shared;
                
                // Rent an array (might be reused from a pool, or newly allocated if empty)
                // This does NOT allocate on the managed heap if reused.
                byte[] buffer = pool.Rent(1024); 

                for (int i = 0; i < 1000; i++)
                {
                    // Simulate processing data into the buffer
                    // In a real scenario, this might be reading from a stream or processing a tensor.
                    System.Text.Encoding.UTF8.GetBytes($"Token_{i}_", 0, 7, buffer, 0);

                    // Simulate CPU work (e.g., matrix multiplication)
                    // Note: We cannot allocate strings here if we are in a NoGCRegion!
                    // The following line would throw an OutOfMemoryException if uncommented inside the region:
                    // string dummy = $"Allocating_{i}"; 
                    
                    // Simulate async work without yielding (if possible) or carefully
                    // Since we are in a NoGC region, we generally avoid 'await' inside the region
                    // because it might switch threads or contexts. 
                    // For this demo, we simulate CPU work directly.
                    Thread.SpinWait(100); 
                }

                // Return the buffer to the pool so it can be reused
                pool.Return(buffer);

                sw.Stop();
                long memEnd = GC.GetTotalMemory(false); // Should be 0 or very low change

                Console.WriteLine($"Real-Time Inference Complete.");
                Console.WriteLine($"Time Elapsed: {sw.ElapsedMilliseconds}ms");
                Console.WriteLine($"Memory Allocated: ~{(memEnd - memStart) / 1024.0:F2} KB (Should be near 0)");
                Console.WriteLine($"Gen0 Collections: {GC.CollectionCount(0)}");
            }
            finally
            {
                // ALWAYS end the NoGC region, even if an exception occurs.
                if (noGCRegionEntered)
                {
                    GC.EndNoGCRegion();
                    Console.WriteLine("Exited NoGC Region.");
                }
            }
        }
    }
}
