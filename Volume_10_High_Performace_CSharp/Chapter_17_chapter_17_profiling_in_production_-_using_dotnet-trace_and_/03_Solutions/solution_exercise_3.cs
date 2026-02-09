
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TokenProcessorGC
{
    class Program
    {
        // Leaky cache: Grows unbounded during high load
        private static readonly ConcurrentDictionary<string, object> _cache = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting GC Pressure Simulation...");
            var cts = new CancellationTokenSource();
            bool highLoad = true; // Start with high load to trigger GC

            // Monitor keyboard to toggle load
            var inputTask = Task.Run(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'q') { cts.Cancel(); break; }
                    if (key.KeyChar == 'l') { highLoad = !highLoad; Console.WriteLine($"High Load: {highLoad}"); }
                }
            });

            var random = new Random();
            var sw = Stopwatch.StartNew();

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    // Simulate token processing
                    var tokenData = new byte[1024]; // 1KB allocation
                    random.NextBytes(tokenData);

                    if (highLoad)
                    {
                        // 1. Simulate GC Pressure via Allocations
                        // Allocate strings to fill heap
                        var largeString = new string('A', 5000);

                        // 2. Leaky Cache Mechanism
                        // Store metadata without eviction to force Gen 2 growth
                        _cache.TryAdd(Guid.NewGuid().ToString(), new { Data = tokenData, Timestamp = DateTime.UtcNow });
                    }

                    // Throttle slightly to allow trace collection without crashing immediately
                    if (!highLoad) Thread.Sleep(50);
                    
                    // Artificially trigger Gen 2 collection observation if heap is huge
                    if (_cache.Count > 50000 && highLoad)
                    {
                        Console.WriteLine("Cache size critical... waiting for GC...");
                        // Just waiting to observe GC behavior in trace
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Stopped.");
            }
        }
    }
}
