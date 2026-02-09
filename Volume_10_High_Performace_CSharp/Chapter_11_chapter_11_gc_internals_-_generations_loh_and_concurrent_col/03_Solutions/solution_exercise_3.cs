
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GCInternalsExercise3
{
    public class TokenProcessor
    {
        // Simulate CPU work and memory allocation
        public void ProcessToken(int tokenId)
        {
            // 1. Memory Allocation: Create a string token
            string tokenData = $"Processing_Token_{tokenId}_With_Some_Payload";
            
            // 2. CPU Work: Simulate matrix multiplication (math ops)
            double sum = 0;
            for (int i = 0; i < 1000; i++)
            {
                sum += Math.Sqrt(i) * Math.Sin(tokenId);
            }

            // 3. Allocation for result (preventing optimization)
            if (sum > 1000) 
            {
                // This branch is rarely taken, but creates a new object if hit
                var result = new List<double> { sum };
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Current GC Mode: {(GCSettings.IsServerGC ? "Server GC" : "Workstation GC")}");
            Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");

            var processor = new TokenProcessor();
            int iterations = 1_000_000;

            // Snapshot GC counts before
            int gen0Start = GC.CollectionCount(0);
            int gen1Start = GC.CollectionCount(1);
            int gen2Start = GC.CollectionCount(2);

            Stopwatch sw = Stopwatch.StartNew();

            // High-frequency loop
            for (int i = 0; i < iterations; i++)
            {
                processor.ProcessToken(i);
                
                // Optional: Explicitly induce GC every 50k iterations to force observation
                // In a real scenario, we rely on the GC's natural triggering.
                if (i % 50_000 == 0 && i > 0)
                {
                    // Just a marker, we won't force it here to let the GC do its job naturally
                }
            }

            sw.Stop();

            // Snapshot GC counts after
            int gen0End = GC.CollectionCount(0);
            int gen1End = GC.CollectionCount(1);
            int gen2End = GC.CollectionCount(2);

            Console.WriteLine("\n--- Benchmark Results ---");
            Console.WriteLine($"Iterations: {iterations:N0}");
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Throughput: {(iterations / (sw.ElapsedMilliseconds / 1000.0)):F0} ops/sec");
            
            Console.WriteLine("\n--- GC Statistics ---");
            Console.WriteLine($"Gen 0 Collections: {gen0End - gen0Start}");
            Console.WriteLine($"Gen 1 Collections: {gen1End - gen1Start}");
            Console.WriteLine($"Gen 2 Collections: {gen2End - gen2Start}");
            
            Console.WriteLine("\n--- Analysis ---");
            if (GCSettings.IsServerGC)
            {
                Console.WriteLine("Observation: Server GC typically has larger segment sizes and parallelizes collection.");
                Console.WriteLine("Expectation: Fewer Gen 0 pauses relative to throughput, but potentially more Gen 2 collections due to per-CPU heaps.");
            }
            else
            {
                Console.WriteLine("Observation: Workstation GC is tuned for UI responsiveness.");
                Console.WriteLine("Expectation: Frequent Gen 0 collections to keep pauses short, but potentially higher overhead in tight loops.");
            }
        }
    }
}
