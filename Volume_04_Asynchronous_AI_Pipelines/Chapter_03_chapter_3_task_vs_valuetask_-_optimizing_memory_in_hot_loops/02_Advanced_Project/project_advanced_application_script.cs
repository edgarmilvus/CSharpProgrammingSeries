
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
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace AsyncMemoryOptimization
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting LLM Token Stream Processor...");
            Console.WriteLine("Simulating high-throughput token generation with memory optimization.\n");

            // Scenario: Processing a stream of tokens from an LLM
            // We simulate a "hot loop" where tokens arrive rapidly.
            // Goal: Minimize heap allocations (Task objects) to reduce GC pressure.

            var processor = new TokenProcessor();

            // 1. Baseline: Using Task (Allocates on heap)
            Console.WriteLine("--- Baseline: Using Task (Heap Allocations) ---");
            long baselineMemory = GC.GetTotalMemory(true);
            var sw = Stopwatch.StartNew();

            await processor.ProcessTokensWithTaskAsync(1000); // Process 1000 tokens

            sw.Stop();
            long endMemory = GC.GetTotalMemory(true);
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms | Memory Delta: {endMemory - baselineMemory} bytes");
            Console.WriteLine($"Gen 0 Collections: {GC.CollectionCount(0)}\n");

            // Force GC to clean up for next test
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // 2. Optimized: Using ValueTask (Stack or Heap based on completion state)
            Console.WriteLine("--- Optimized: Using ValueTask (Minimized Allocations) ---");
            long optMemory = GC.GetTotalMemory(true);
            var swOpt = Stopwatch.StartNew();

            await processor.ProcessTokensWithValueTaskAsync(1000);

            swOpt.Stop();
            long endOptMemory = GC.GetTotalMemory(true);
            Console.WriteLine($"Time: {swOpt.ElapsedMilliseconds}ms | Memory Delta: {endOptMemory - optMemory} bytes");
            Console.WriteLine($"Gen 0 Collections: {GC.CollectionCount(0)}");

            Console.WriteLine("\nOptimization Complete.");
        }
    }

    // Simulates an external LLM API that yields tokens one by one.
    public class LLMTokenStream
    {
        private readonly string[] _tokens = { "The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog." };
        private readonly Random _random = new Random();

        // Simulates an async I/O operation (network request)
        public async Task<string> GetNextTokenAsync()
        {
            // Simulate network latency (1-5ms)
            await Task.Delay(_random.Next(1, 5));
            return _tokens[_random.Next(0, _tokens.Length)];
        }

        // Optimized synchronous completion path simulation
        // In real scenarios, if data is buffered locally, we can return synchronously.
        public ValueTask<string> GetNextTokenOptimizedAsync()
        {
            // Simulate 80% chance of synchronous completion (data already in buffer)
            if (_random.Next(0, 10) < 8)
            {
                // Returns immediately. No Task allocation. Wraps result in ValueTask (stack).
                return new ValueTask<string>(_tokens[_random.Next(0, _tokens.Length)]);
            }

            // 20% chance of async I/O (network fetch)
            // Returns a Task, which ValueTask can wrap implicitly.
            return new ValueTask<string>(GetNextTokenAsync());
        }
    }

    public class TokenProcessor
    {
        private readonly LLMTokenStream _stream = new LLMTokenStream();

        // --- PATTERN 1: Baseline Implementation ---
        // Returns Task<T>. Every call in a loop allocates a new Task object on the heap.
        // This creates significant pressure on the Garbage Collector in hot loops.
        public async Task ProcessTokensWithTaskAsync(int tokenCount)
        {
            for (int i = 0; i < tokenCount; i++)
            {
                // GetNextTokenAsync ALWAYS returns a Task.
                // Even if the result is ready immediately, a Task object is allocated.
                string token = await _stream.GetNextTokenAsync();
                ConsumeToken(token);
            }
        }

        // --- PATTERN 2: Optimized Implementation ---
        // Returns ValueTask<T>. Can avoid heap allocations if the operation completes synchronously.
        public async Task ProcessTokensWithValueTaskAsync(int tokenCount)
        {
            for (int i = 0; i < tokenCount; i++)
            {
                // GetNextTokenOptimizedAsync returns ValueTask.
                // If the result is ready immediately (buffer hit), no heap allocation occurs.
                // The result is stored on the stack (inside the ValueTask struct).
                string token = await _stream.GetNextTokenOptimizedAsync();
                ConsumeToken(token);
            }
        }

        // Simulates processing the token (e.g., appending to response buffer)
        private void ConsumeToken(string token)
        {
            // Minimal work to focus on allocation overhead
            // In a real app, this would be string concatenation or writing to a stream.
        }
    }
}
