
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryDeadlockTest
{
    // 1. Simulate Legacy Context (Single Threaded)
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback, object?)> _queue = new();

        public override void Post(SendOrPostCallback d, object? state)
        {
            _queue.Add((d, state));
        }

        public void Run()
        {
            SetSynchronizationContext(this);
            foreach (var (callback, state) in _queue.GetConsumingEnumerable())
            {
                callback(state);
            }
        }
    }

    // 2. The Library Method
    public class MathLibrary
    {
        public async Task PerformHeavyCalculationAsync(bool useConfigureAwait)
        {
            // CPU bound work offloaded to ThreadPool
            // Task.Run automatically unwinds the context (it runs on ThreadPool)
            await Task.Run(() => 
            {
                // Simulate heavy calculation
                long sum = 0;
                for (int i = 0; i < 1_000_000; i++) sum++;
                Console.WriteLine("Calculation done.");
            }).ConfigureAwait(useConfigureAwait);

            // File I/O
            // Even though Task.Run puts us on the ThreadPool, 
            // awaiting the next I/O operation captures the context again if not configured.
            await WriteFileAsync().ConfigureAwait(useConfigureAwait);
        }

        private async Task WriteFileAsync()
        {
            // Simulate async I/O
            await Task.Delay(50);
            Console.WriteLine("File write done.");
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("--- Exercise 4: Task.Run and ConfigureAwait ---");

            // Scenario A: Blocking on the result (Classic Deadlock setup)
            Console.WriteLine("\n[Scenario A] Blocking .Wait() without ConfigureAwait...");
            RunScenario(false, true); 

            // Scenario B: Blocking with ConfigureAwait(false)
            Console.WriteLine("\n[Scenario B] Blocking .Wait() with ConfigureAwait(false)...");
            RunScenario(true, true);

            // Scenario C: Non-blocking (No deadlock risk)
            Console.WriteLine("\n[Scenario C] Non-blocking execution...");
            RunScenario(false, false);
        }

        private static void RunScenario(bool useConfigureAwait, bool shouldBlock)
        {
            var context = new SingleThreadSynchronizationContext();
            var t = new Thread(() => context.Run());
            t.Start();

            context.Post(async _ =>
            {
                var lib = new MathLibrary();
                try
                {
                    var task = lib.PerformHeavyCalculationAsync(useConfigureAwait);

                    if (shouldBlock)
                    {
                        // Simulate legacy code blocking the thread
                        task.Wait(); 
                        Console.WriteLine("Blocking call completed.");
                    }
                    else
                    {
                        await task;
                        Console.WriteLine("Async call completed.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                finally
                {
                    // Clean exit
                    Environment.Exit(0); 
                }
            }, null);
        }
    }
}
