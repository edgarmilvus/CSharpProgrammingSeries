
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LatencyAnalysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting synchronous pipeline simulation...");
            var stopwatch = Stopwatch.StartNew();

            // Simulate 10 concurrent users
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                // Note: We are intentionally NOT awaiting here to simulate concurrency.
                // However, because the pipeline uses blocking calls (.Result), 
                // the threads will be blocked, and concurrency is effectively serialized.
                tasks.Add(ProcessRequestSynchronously(i));
            }

            // Wait for all "concurrent" tasks to complete
            await Task.WhenAll(tasks);

            stopwatch.Stop();
            Console.WriteLine($"Total execution time for 10 requests: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            
            // Analysis Comment:
            // The total time is approximately 25 seconds (10 * 2.5s).
            // Even though we launched 10 tasks "concurrently", the use of .Wait() or .Result 
            // inside ProcessRequestSynchronously blocks the calling thread. 
            // Since we are likely running on a limited thread pool (especially in .NET's default SynchronizationContext),
            // the blocked threads cannot be reused to process other requests immediately.
            // This forces the pipeline to execute essentially sequentially, accumulating latency 
            // rather than overlapping I/O waits with other work.
        }

        static async Task ProcessRequestSynchronously(int requestId)
        {
            // 1. Input Processing (CPU-bound simulation)
            // Simulating CPU work with Delay. In a real scenario, this is blocking CPU work.
            await Task.Delay(500); 

            // 2. Model Inference (I/O-bound simulation)
            // We simulate an I/O call by awaiting a delay, but the blocking nature 
            // comes from how we handle the task in a synchronous context.
            // Here we just await, but if we used .Result on a Task, it would block.
            // To strictly follow "standard synchronous method calls" or blocking simulation:
            // We will simulate a synchronous wait using Task.Delay(2000).GetAwaiter().GetResult();
            // or simply await it. However, the prompt asks to identify the blocking nature.
            // Let's simulate a truly blocking I/O call using .Wait() on a Task to demonstrate the penalty.
            
            var ioTask = Task.Delay(2000);
            ioTask.Wait(); // BLOCKING CALL: This halts the thread until the I/O completes.

            // 3. Result Formatting (CPU-bound simulation)
            await Task.Delay(500);
        }
    }
}
