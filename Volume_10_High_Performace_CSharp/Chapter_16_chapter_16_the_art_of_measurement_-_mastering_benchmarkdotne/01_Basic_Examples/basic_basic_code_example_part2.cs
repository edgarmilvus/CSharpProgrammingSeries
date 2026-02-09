
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

# Source File: basic_basic_code_example_part2.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Buffers;

namespace TokenProcessingBenchmarks
{
    // [MemoryDiagnoser] is a crucial attribute that tells BenchmarkDotNet 
    // to track memory allocations (GC Gen 0, Gen 1, Gen 2, and total bytes).
    [MemoryDiagnoser]
    public class TokenizerBenchmarks
    {
        // A constant input string to ensure we are benchmarking the logic, 
        // not the time it takes to generate random data.
        private const string InputText = "The quick brown fox jumps over the lazy dog. AI models process tokens.";
        
        // We will benchmark a specific length, but let's make it a parameter to be flexible.
        [Params(100, 1000, 10000)]
        public int StringLength { get; set; }

        private string _testString = "";

        // [GlobalSetup] runs once before any benchmark iterations begin.
        // It prepares the environment so setup time isn't included in the measurement.
        [GlobalSetup]
        public void Setup()
        {
            // Create a string of the specific length required for the current run.
            if (InputText.Length >= StringLength)
            {
                _testString = InputText.Substring(0, StringLength);
            }
            else
            {
                // Repeat the input text until we reach the desired length.
                int repeatCount = (int)Math.Ceiling((double)StringLength / InputText.Length);
                _testString = string.Concat(Enumerable.Repeat(InputText, repeatCount));
                _testString = _testString.Substring(0, StringLength);
            }
        }

        /// <summary>
        /// The "Naive" approach: Allocates a new integer array (heap allocation)
        /// every time it runs. This creates GC pressure.
        /// </summary>
        [Benchmark(Baseline = true)]
        public int[] NaiveAllocation()
        {
            // 1. Allocate a new array on the Heap.
            int[] tokens = new int[_testString.Length];

            // 2. Iterate and fill.
            for (int i = 0; i < _testString.Length; i++)
            {
                // Simulate a simple mapping (e.g., char code to int)
                tokens[i] = _testString[i];
            }

            // 3. Return the array (kept alive by the caller).
            return tokens;
        }

        /// <summary>
        /// The "Optimized" approach: Uses Span<T> to operate on a stack-allocated
        /// buffer or a shared buffer, minimizing GC pressure.
        /// </summary>
        [Benchmark]
        public int SpanOptimization()
        {
            // 1. Rent a buffer from the ArrayPool. 
            // This reuses existing arrays from a shared pool instead of allocating new ones.
            // It is effectively "zero allocation" for the array itself after the pool warms up.
            int[] rentedArray = ArrayPool<int>.Shared.Rent(_testString.Length);
            
            try
            {
                // 2. Create a Span<T> view over the rented array.
                // Span is a ref struct, meaning it lives on the Stack, not the Heap.
                // This allows us to manipulate memory safely without heap allocations.
                Span<int> tokens = rentedArray.AsSpan(0, _testString.Length);

                for (int i = 0; i < _testString.Length; i++)
                {
                    tokens[i] = _testString[i];
                }

                // In a real scenario, we might return a ReadOnlySpan<int> or copy to a result.
                // For the benchmark, we just return the sum to ensure the JIT 
                // doesn't optimize away the entire loop (dead code elimination).
                int sum = 0;
                foreach(var t in tokens) sum += t;
                return sum;
            }
            finally
            {
                // 3. CRITICAL: Return the array to the pool so it can be reused.
                // If we forget this, we lose the benefit of the pool and might cause a leak.
                ArrayPool<int>.Shared.Return(rentedArray);
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // This line triggers BenchmarkDotNet to compile, run, and analyze the benchmarks.
            var summary = BenchmarkRunner.Run<TokenizerBenchmarks>();
        }
    }
}
