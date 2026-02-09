
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStateMachinesExercises
{
    public class Exercise2
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("=== Exercise 2: Event Loop & Task Queue ===\n");

            // Scenario 1: I/O-bound simulation (Non-blocking)
            Console.WriteLine("--- Scenario 1: I/O-bound (Async/Await) ---");
            var sw = Stopwatch.StartNew();
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int id = i;
                tasks[i] = ProcessRequestAsync(id);
            }
            await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine($"Total Time (I/O-bound): {sw.ElapsedMilliseconds}ms\n");

            // Scenario 2: CPU-bound simulation (Blocking)
            Console.WriteLine("--- Scenario 2: CPU-bound (Blocking Thread.Sleep) ---");
            sw.Restart();
            var blockingTasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int id = i;
                // Note: We use Task.Run here to offload to ThreadPool, 
                // but inside we use Thread.Sleep which blocks that thread.
                blockingTasks[i] = Task.Run(() => ProcessHeavyComputation(id));
            }
            await Task.WhenAll(blockingTasks);
            sw.Stop();
            Console.WriteLine($"Total Time (CPU-bound/Blocking): {sw.ElapsedMilliseconds}ms\n");

            // Challenge Task: Refactored CPU-bound (Truly Asynchronous Simulation)
            Console.WriteLine("--- Challenge: Refactored CPU-bound (Task.Run Simulation) ---");
            sw.Restart();
            var asyncCompTasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int id = i;
                asyncCompTasks[i] = ProcessHeavyComputationAsync(id);
            }
            await Task.WhenAll(asyncCompTasks);
            sw.Stop();
            Console.WriteLine($"Total Time (Async Refactored): {sw.ElapsedMilliseconds}ms");
        }

        // I/O-bound simulation: Yields thread to pool while waiting
        private static async Task ProcessRequestAsync(int id)
        {
            // Console.WriteLine($"Request {id} started on thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(100); // Non-blocking wait
            // Console.WriteLine($"Request {id} finished on thread {Thread.CurrentThread.ManagedThreadId}");
        }

        // CPU-bound simulation: Blocks the thread for 100ms
        private static void ProcessHeavyComputation(int id)
        {
            // Console.WriteLine($"Computation {id} started on thread {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(100); // BLOCKING call
            // Console.WriteLine($"Computation {id} finished on thread {Thread.CurrentThread.ManagedThreadId}");
        }

        // Refactored: Offloads blocking work to a separate thread so the pool thread isn't blocked
        private static async Task ProcessHeavyComputationAsync(int id)
        {
            await Task.Run(() => Thread.Sleep(100));
        }
    }
}
