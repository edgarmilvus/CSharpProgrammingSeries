
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeadlockSimulation
{
    // 1. Simulate the UI SynchronizationContext
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback callback, object state)> _queue = new();
        private readonly object _lock = new();
        private bool _isRunning;

        public override void Post(SendOrPostCallback d, object? state)
        {
            lock (_lock)
            {
                _queue.Enqueue((d, state));
                Monitor.Pulse(_lock);
            }
        }

        public void RunOnCurrentThread()
        {
            SetSynchronizationContext(this);
            _isRunning = true;
            
            while (true)
            {
                (SendOrPostCallback callback, object state) item;
                lock (_lock)
                {
                    while (_queue.Count == 0 && _isRunning)
                    {
                        Monitor.Wait(_lock);
                    }
                    
                    if (!_isRunning && _queue.Count == 0) break;
                    
                    item = _queue.Dequeue();
                }
                
                item.callback(item.state);
            }
        }

        public void Complete() 
        {
            lock (_lock)
            {
                _isRunning = false;
                Monitor.Pulse(_lock);
            }
        }
    }

    // 2. Simulate the Application Logic
    public class ApplicationLogic
    {
        // Simulate the problematic GetDataAsync
        public async Task<string> GetDataAsync(bool useConfigureAwait)
        {
            // Simulate network latency
            await Task.Delay(100).ConfigureAwait(useConfigureAwait);
            
            // Return to the captured SynchronizationContext (Simulated UI Thread)
            return "Data from API";
        }

        public string FetchDataSync(bool useConfigureAwait)
        {
            // BLOCKING CALL: This is where the deadlock occurs if context is captured
            return GetDataAsync(useConfigureAwait).Result;
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("--- Exercise 1: Deadlock Simulation ---");
            
            // Scenario A: Deadlock (No ConfigureAwait)
            Console.WriteLine("\n[Scenario A] Running without ConfigureAwait(false)...");
            RunScenario(useConfigureAwait: false);

            // Scenario B: Fixed (With ConfigureAwait)
            Console.WriteLine("\n[Scenario B] Running with ConfigureAwait(false)...");
            RunScenario(useConfigureAwait: true);
        }

        private static void RunScenario(bool useConfigureAwait)
        {
            var syncContext = new SingleThreadSynchronizationContext();
            
            // We must run the simulation on the dedicated thread to mimic the UI thread
            var thread = new Thread(() =>
            {
                syncContext.RunOnCurrentThread();
            });
            
            thread.Start();

            // Post the work to the simulated UI thread context
            syncContext.Post(_ =>
            {
                try
                {
                    var logic = new ApplicationLogic();
                    Console.WriteLine("Starting blocking fetch...");
                    
                    // BLOCKING THE UI THREAD HERE
                    string result = logic.FetchDataSync(useConfigureAwait);
                    
                    Console.WriteLine($"Success! Result: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    syncContext.Complete();
                }
            }, null);

            thread.Join();
        }
    }
}
