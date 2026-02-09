
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAIPipelines.ThreadSafety
{
    public class BasicLockExample
    {
        // Represents a shared resource: a log of processed AI requests.
        // In a real scenario, this could be a database context, a file stream, or a shared cache.
        private readonly List<string> _requestLog = new List<string>();

        // The lock object. This must be a reference type (not a value type like int)
        // and should ideally be private and readonly to prevent accidental reassignment.
        private readonly object _logLock = new object();

        public async Task RunSimulationAsync()
        {
            Console.WriteLine("Starting simulated AI request processing with locking...");

            // Create 10 concurrent tasks simulating simultaneous user requests.
            var tasks = new List<Task>();
            for (int i = 1; i <= 10; i++)
            {
                int requestId = i;
                tasks.Add(Task.Run(() => ProcessRequestAsync(requestId)));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("\nFinal Request Log:");
            foreach (var entry in _requestLog)
            {
                Console.WriteLine($" - {entry}");
            }
        }

        private async Task ProcessRequestAsync(int requestId)
        {
            // Simulate some network latency or LLM processing time.
            await Task.Delay(new Random().Next(50, 150));

            // CRITICAL SECTION START
            // We enter a lock to ensure that only one thread can modify the shared
            // _requestLog at a time.
            lock (_logLock)
            {
                Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Processing Request #{requestId} - Lock Acquired");

                // Check current count (simulating a read-then-write operation)
                int currentCount = _requestLog.Count;

                // Simulate a tiny processing delay inside the lock to exaggerate
                // the chance of collision if the lock were missing.
                Thread.Sleep(10);

                // Modify the shared resource
                _requestLog.Add($"Request {requestId} processed at {DateTime.Now:HH:mm:ss.fff} by Thread {Thread.CurrentThread.ManagedThreadId} (Log Index: {currentCount})");

                Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Request #{requestId} - Lock Released");
            }
            // CRITICAL SECTION END
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var example = new BasicLockExample();
            await example.RunSimulationAsync();
        }
    }
}
