
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace TokenPipelineOptimization
{
    public class VectorizedTokenNormalizer
    {
        // ORIGINAL APPROACH (High Allocation)
        public string NormalizeLegacy(ReadOnlySpan<char> token)
        {
            // Simulated SIMD logic (placeholder for actual vectorization)
            var vectorResult = SimdOperations.Process(token);

            // Formatting logic using string.Create
            // This allocates a new string on the heap every call
            return string.Create(token.Length, vectorResult, (span, state) =>
            {
                token.CopyTo(span);
                // Simulate formatting
                span[0] = char.ToUpper(span[0]);
            });
        }

        // OPTIMIZED APPROACH (Low Allocation)
        public int NormalizeOptimized(ReadOnlySpan<char> token, Span<char> output)
        {
            if (output.Length < token.Length)
                throw new ArgumentException("Output buffer too small");

            // 1. Maintain SIMD Logic
            var vectorResult = SimdOperations.Process(token);

            // 2. Zero Allocation Writing
            // We write directly to the provided output span.
            // No heap allocation occurs here.
            token.CopyTo(output);
            if (output.Length > 0)
            {
                output[0] = char.ToUpper(output[0]);
            }

            return token.Length;
        }
    }

    // Mock SIMD operations for the example
    public static class SimdOperations
    {
        public static Vector128<int> Process(ReadOnlySpan<char> token)
        {
            // Placeholder for actual SIMD logic
            return Vector128<int>.Zero;
        }
    }

    // Benchmark Harness (Simplified)
    public class Benchmark
    {
        public static void Run()
        {
            var normalizer = new VectorizedTokenNormalizer();
            var token = "simd_vectorized_token_data";
            int iterations = 1_000_000;

            // --- Legacy Test ---
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                string _ = normalizer.NormalizeLegacy(token);
            }
            sw.Stop();
            Console.WriteLine($"Legacy (String.Create): {sw.ElapsedMilliseconds}ms. Allocations: High (1M strings).");

            // --- Optimized Test ---
            // Use ArrayPool to reuse buffers
            var buffer = ArrayPool<char>.Shared.Rent(token.Length);
            try
            {
                sw.Restart();
                for (int i = 0; i < iterations; i++)
                {
                    // Write directly to the rented buffer
                    int len = normalizer.NormalizeOptimized(token, buffer.AsSpan(0, token.Length));
                    
                    // If we needed a string, we would create it HERE, but often
                    // downstream processing can work on Span<char> directly.
                    // For the sake of the exercise, we assume we just needed the normalization result in memory.
                }
                sw.Stop();
                Console.WriteLine($"Optimized (Span + Pool): {sw.ElapsedMilliseconds}ms. Allocations: Minimal (Buffer rent/return).");
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }
}
