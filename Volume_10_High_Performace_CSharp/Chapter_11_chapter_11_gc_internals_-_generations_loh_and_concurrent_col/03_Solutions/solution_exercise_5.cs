
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GCInternalsExercise5
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Low-Latency GC Tuning Simulation...");

            // Phase 1: Critical Inference (Low Latency)
            Console.WriteLine("\n--- Phase 1: Critical Inference (SustainedLowLatency) ---");
            try
            {
                // Attempt to set latency mode
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                Console.WriteLine($"Latency Mode set to: {GCSettings.LatencyMode}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Failed to set latency mode: {ex.Message}");
                return;
            }

            var jitterWatch = new Stopwatch();
            var latencies = new List<long>();
            
            // Simulate real-time processing loop
            for (int i = 0; i < 10_000; i++)
            {
                jitterWatch.Restart();
                
                // Simulate allocation (e.g., creating result tensors)
                // In SustainedLowLatency, Gen 2 collections are blocked, 
                // so we must be careful not to exhaust memory.
                var tempData = new byte[1024]; 
                
                // Simulate compute
                Thread.SpinWait(50); 
                
                jitterWatch.Stop();
                latencies.Add(jitterWatch.ElapsedTicks);
            }

            // Calculate Jitter
            long maxLatency = latencies.Max();
            long minLatency = latencies.Min();
            Console.WriteLine($"Allocation Jitter (Ticks): Min={minLatency}, Max={maxLatency}, Diff={maxLatency - minLatency}");
            Console.WriteLine($"Total Gen 0 Collections during phase: {GC.CollectionCount(0)}");

            // Phase 2: Maintenance (Throughput)
            Console.WriteLine("\n--- Phase 2: Maintenance (Throughput) ---");
            
            // Reset latency mode
            GCSettings.LatencyMode = GCLatencyMode.Throughput;
            Console.WriteLine($"Latency Mode set to: {GCSettings.LatencyMode}");

            // Force a Gen 2 collection to clean up the accumulated memory from Phase 1
            // This is safe now because we are not in a low-latency window.
            Console.WriteLine("Forcing Full GC to reclaim memory...");
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            
            Console.WriteLine($"Total Gen 2 Collections after phase: {GC.CollectionCount(2)}");
            Console.WriteLine("Maintenance complete. System ready for next cycle.");
        }
    }
}
