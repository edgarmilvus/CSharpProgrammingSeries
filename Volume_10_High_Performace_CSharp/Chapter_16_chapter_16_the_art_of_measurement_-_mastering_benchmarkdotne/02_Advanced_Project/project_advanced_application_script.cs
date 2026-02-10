
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HighPerformanceAITokenProcessing
{
    // REAL-WORLD CONTEXT:
    // In Large Language Model (LLM) inference, "Token Processing" often involves
    // calculating the dot product of a large vector of token embeddings (input)
    // against a specific weight vector (e.g., a query vector in attention mechanisms).
    // This is computationally expensive. We will implement three versions:
    // 1. Naive Loop (Baseline)
    // 2. Span<T> (Memory Safety & Zero-Copy Access)
    // 3. SIMD (Hardware Acceleration)
    // We will simulate the benchmarking environment manually to demonstrate
    // the measurement principles before applying the actual BenchmarkDotNet library.

    public class TokenProcessor
    {
        // Configuration for our simulation
        private const int VectorSize = 1024; // Typical size for embedding dimensions
        private const int Iterations = 10000; // How many times we process a token batch

        // Data storage: Standard arrays
        private readonly float[] _tokenEmbeddings;
        private readonly float[] _queryWeights;

        public TokenProcessor()
        {
            // Initialize with random data to simulate real embeddings
            var rnd = new Random(42);
            _tokenEmbeddings = new float[VectorSize];
            _queryWeights = new float[VectorSize];

            for (int i = 0; i < VectorSize; i++)
            {
                _tokenEmbeddings[i] = (float)rnd.NextDouble();
                _queryWeights[i] = (float)rnd.NextDouble();
            }
        }

        // APPROACH 1: NAIVE IMPLEMENTATION
        // Uses standard arrays and a basic for-loop.
        // Pros: Easy to read, standard C#.
        // Cons: Bounds checking on every access, no hardware acceleration.
        public float CalculateAttentionScoreNaive()
        {
            float sum = 0.0f;
            for (int i = 0; i < VectorSize; i++)
            {
                // Implicit bounds checking occurs here
                sum += _tokenEmbeddings[i] * _queryWeights[i];
            }
            return sum;
        }

        // APPROACH 2: SPAN<T> IMPLEMENTATION
        // Uses Span<T> to represent contiguous memory regions.
        // Pros: Eliminates array overhead, allows bounds-check elision in some JIT scenarios,
        //       enables slicing without copying memory.
        // Cons: Still scalar operations (one number at a time).
        public float CalculateAttentionScoreSpan()
        {
            // Create spans over the existing arrays (Zero-copy)
            Span<float> embeddingsSpan = _tokenEmbeddings.AsSpan();
            Span<float> weightsSpan = _queryWeights.AsSpan();

            float sum = 0.0f;
            int length = embeddingsSpan.Length;

            // Using unsafe context or simply trusting the JIT can sometimes
            // optimize bounds checks away inside tight loops on Spans.
            // However, for strict safety, we access via indices.
            for (int i = 0; i < length; i++)
            {
                sum += embeddingsSpan[i] * weightsSpan[i];
            }
            return sum;
        }

        // APPROACH 3: SIMD IMPLEMENTATION
        // Uses System.Numerics.Vector<T> for SIMD (Single Instruction, Multiple Data).
        // Pros: Processes 4-8 floats per CPU cycle (depending on hardware/AVX support).
        // Cons: Requires handling remainders (vector size vs data size), alignment considerations.
        public float CalculateAttentionScoreSimd()
        {
            // Vector<T> is a hardware-accelerated type.
            // Vector<float>.Count gives the number of floats processed in one instruction (e.g., 8 on AVX2).
            int vectorSize = Vector<float>.Count;
            int i = 0;
            float sum = 0.0f;

            // Process chunks of data using Vector operations
            int limit = _tokenEmbeddings.Length - vectorSize;
            
            // We use local variables to pin references (reducing array bounds checking overhead)
            var embeddings = _tokenEmbeddings;
            var weights = _queryWeights;

            for (; i <= limit; i += vectorSize)
            {
                // Load vectors from memory
                var v1 = new Vector<float>(embeddings, i);
                var v2 = new Vector<float>(weights, i);

                // Multiply and add (SIMD operation)
                // This compiles to a single instruction on supported hardware (e.g., VMULPS + VADDPS)
                var result = v1 * v2;

                // Horizontal sum: Sum all elements in the vector
                // Note: Vector.Sum is efficient but involves some shuffling instructions.
                sum += Vector.Sum(result);
            }

            // Process the remaining elements (tail) using standard scalar operations
            for (; i < _tokenEmbeddings.Length; i++)
            {
                sum += embeddings[i] * weights[i];
            }

            return sum;
        }

        // BENCHMARK RUNNER SIMULATION
        // This method demonstrates how to measure execution time accurately
        // without external libraries, mimicking what BenchmarkDotNet does internally.
        public void RunMeasurements()
        {
            Console.WriteLine($"Starting measurements (Vector Size: {VectorSize}, Iterations: {Iterations})");
            Console.WriteLine("--------------------------------------------------------------------------------");

            // 1. Measure Naive Approach
            Stopwatch sw = Stopwatch.StartNew();
            float resultNaive = 0;
            for (int k = 0; k < Iterations; k++)
            {
                resultNaive = CalculateAttentionScoreNaive();
            }
            sw.Stop();
            long naiveTicks = sw.ElapsedTicks;
            Console.WriteLine($"[Naive Loop]      Time: {naiveTicks,6} ticks | Result: {resultNaive:F6}");

            // 2. Measure Span Approach
            sw.Restart();
            float resultSpan = 0;
            for (int k = 0; k < Iterations; k++)
            {
                resultSpan = CalculateAttentionScoreSpan();
            }
            sw.Stop();
            long spanTicks = sw.ElapsedTicks;
            Console.WriteLine($"[Span<T>]         Time: {spanTicks,6} ticks | Result: {resultSpan:F6}");

            // 3. Measure SIMD Approach
            sw.Restart();
            float resultSimd = 0;
            for (int k = 0; k < Iterations; k++)
            {
                resultSimd = CalculateAttentionScoreSimd();
            }
            sw.Stop();
            long simdTicks = sw.ElapsedTicks;
            Console.WriteLine($"[SIMD Vector]     Time: {simdTicks,6} ticks | Result: {resultSimd:F6}");

            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Analysis:");
            Console.WriteLine($"- Span vs Naive: {(naiveTicks > spanTicks ? "Faster" : "Slower")} (JIT optimizations vary)");
            Console.WriteLine($"- SIMD Speedup: {((double)naiveTicks / simdTicks):F2}x faster than Naive");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate the processor
            var processor = new TokenProcessor();

            // Run the measurement suite
            processor.RunMeasurements();

            // Keep console open
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
