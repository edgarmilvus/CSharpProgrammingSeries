
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
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace HighPerformanceTokenProcessing
{
    /// <summary>
    /// Simulates a high-throughput AI inference scenario where variable-length
    /// token streams (text prompts) are processed. Instead of allocating new 
    /// large byte arrays for every request, we utilize ArrayPool<T> to rent 
    /// and return buffers, drastically reducing Garbage Collection (GC) pressure.
    /// </summary>
    class Program
    {
        // Configuration for the simulation
        private const int ConcurrentRequests = 1000;
        private const int MaxTokenLength = 2048; // Simulating large context windows

        static void Main(string[] args)
        {
            Console.WriteLine("Starting High-Performance Token Processing Simulation...");
            
            // Warm up the ArrayPool to ensure initial allocation costs are not 
            // counted in the benchmark.
            ArrayPool<byte>.Shared.Rent(1024);
            ArrayPool<byte>.Shared.Rent(4096);

            // 1. Traditional Approach: Allocating new arrays for every request.
            // This simulates a naive implementation often found in unoptimized code.
            long baselineMemory = GC.GetTotalMemory(true);
            Stopwatch baselineWatch = Stopwatch.StartNew();
            
            ProcessRequestsTraditionally(ConcurrentRequests);
            
            baselineWatch.Stop();
            long baselineAllocated = GC.GetTotalMemory(true) - baselineMemory;
            long baselineCollections = GC.CollectionCount(0) + GC.CollectionCount(1);

            // Force a clean GC state before the optimized run
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // 2. Optimized Approach: Using ArrayPool<T>.
            // This reuses memory buffers across the request lifecycle.
            long optimizedMemory = GC.GetTotalMemory(true);
            Stopwatch optimizedWatch = Stopwatch.StartNew();

            ProcessRequestsWithPool(ConcurrentRequests);

            optimizedWatch.Stop();
            long optimizedAllocated = GC.GetTotalMemory(true) - optimizedMemory;
            long optimizedCollections = GC.CollectionCount(0) + GC.CollectionCount(1);

            // 3. Results Analysis
            PrintResults(baselineWatch, baselineAllocated, baselineCollections,
                         optimizedWatch, optimizedAllocated, optimizedCollections);
        }

        /// <summary>
        /// Simulates processing requests using standard 'new' allocation.
        /// High allocation rates trigger frequent Gen 0 Garbage Collections.
        /// </summary>
        static void ProcessRequestsTraditionally(int requestCount)
        {
            Random rng = new Random(42); // Fixed seed for consistency
            
            for (int i = 0; i < requestCount; i++)
            {
                // Simulate dynamic token size (e.g., user prompt length)
                int tokenSize = rng.Next(128, MaxTokenLength);
                
                // ALLOCATION: Creates a new byte array on the Heap.
                // In a real server, this creates immediate memory pressure.
                byte[] buffer = new byte[tokenSize];
                
                // Simulate filling the buffer (Tokenization step)
                FillBuffer(buffer, (byte)i);
                
                // Simulate processing (Inference step)
                ProcessBuffer(buffer);
                
                // REFERENCE LOSS: The buffer becomes eligible for GC immediately.
                // No explicit cleanup is done; the GC handles it eventually.
            }
        }

        /// <summary>
        /// Simulates processing requests using ArrayPool<T>.Shared.
        /// Buffers are rented from a shared pool and returned when done.
        /// </summary>
        static void ProcessRequestsWithPool(int requestCount)
        {
            Random rng = new Random(42); // Same seed for identical workload

            for (int i = 0; i < requestCount; i++)
            {
                int tokenSize = rng.Next(128, MaxTokenLength);
                
                // RENT: Request a buffer from the shared pool.
                // If a suitable buffer exists in the pool, it is returned immediately
                // without a new heap allocation. If not, a new one is created and cached.
                byte[] buffer = ArrayPool<byte>.Shared.Rent(tokenSize);

                try
                {
                    // IMPORTANT: ArrayPool may return a buffer larger than requested.
                    // We must respect the actual length to avoid processing garbage data.
                    // In a real scenario, we might use Span<T> to slice it, but here
                    // we simulate logic that respects the input size.
                    int actualLength = Math.Min(buffer.Length, tokenSize);

                    // Simulate filling the buffer
                    FillBuffer(buffer, (byte)i);

                    // Simulate processing
                    // We pass the buffer and the logical size to the processor
                    ProcessBufferSlice(buffer, actualLength);
                }
                finally
                {
                    // RETURN: Crucial step. If we forget this, the pool assumes the 
                    // buffer is still in use and will eventually allocate a new one 
                    // when the pool is empty, defeating the purpose.
                    // In a high-performance server (e.g., ASP.NET Core), this is often 
                    // handled via Disposable patterns or ObjectPool<T>.
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        // --- Helper Methods (Simulating Logic) ---

        static void FillBuffer(byte[] buffer, byte seed)
        {
            // Simple initialization to simulate data loading
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)((seed + i) % 256);
            }
        }

        static void ProcessBuffer(byte[] buffer)
        {
            // Simulate CPU work (e.g., Matrix Multiplication for a token)
            // Accessing memory ensures the JIT doesn't optimize this away
            long sum = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i];
            }
        }

        static void ProcessBufferSlice(byte[] buffer, int length)
        {
            // Similar to ProcessBuffer but respects the logical length
            long sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += buffer[i];
            }
        }

        static void PrintResults(Stopwatch baselineWatch, long baselineMem, long baselineGC,
                                 Stopwatch optWatch, long optMem, long optGC)
        {
            Console.WriteLine("\n--- Performance Results ---");
            Console.WriteLine($"{' ',-20} | {'Time (ms)',-12} | {'Allocated (bytes)',-18} | {'GC Gen 0/1',-10}");
            Console.WriteLine(new string('-', 70));
            
            Console.WriteLine($"{'Traditional Alloc',-20} | {baselineWatch.ElapsedMilliseconds,-12} | {baselineMem,-18} | {baselineGC,-10}");
            Console.WriteLine($"{'ArrayPool Shared',-20} | {optWatch.ElapsedMilliseconds,-12} | {optMem,-18} | {optGC,-10}");
            
            Console.WriteLine("\n--- Analysis ---");
            Console.WriteLine("1. Time: ArrayPool reduces overhead of heap allocations.");
            Console.WriteLine("2. Memory: Drastically lower memory churn (allocations).");
            Console.WriteLine("3. GC: Fewer collections mean fewer 'Stop-the-World' pauses, crucial for latency-sensitive AI.");
        }
    }
}
