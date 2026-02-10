
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace TokenSimdOptimization
{
    // Ensure the struct size is a multiple of Vector<float>.Size (usually 16 or 32 bytes)
    // 32 floats * 4 bytes = 128 bytes, which is a multiple of typical SIMD registers.
    public struct TokenEmbedding
    {
        public const int EmbeddingSize = 32; 
        public float[] Values;

        public TokenEmbedding(Random rng)
        {
            Values = new float[EmbeddingSize];
            for (int i = 0; i < EmbeddingSize; i++)
            {
                Values[i] = (float)rng.NextDouble();
            }
        }
    }

    public class SimilarityCalculator
    {
        // 1. Scalar Implementation
        public static float CalculateSimilarityScalar(ReadOnlySpan<TokenEmbedding> tokensA, ReadOnlySpan<TokenEmbedding> tokensB)
        {
            float dotProduct = 0;
            int count = tokensA.Length;

            for (int i = 0; i < count; i++)
            {
                var a = tokensA[i].Values;
                var b = tokensB[i].Values;

                for (int j = 0; j < TokenEmbedding.EmbeddingSize; j++)
                {
                    dotProduct += a[j] * b[j];
                }
            }
            return dotProduct;
        }

        // 2. SIMD Implementation
        public static float CalculateSimilaritySimd(ReadOnlySpan<TokenEmbedding> tokensA, ReadOnlySpan<TokenEmbedding> tokensB)
        {
            float dotProduct = 0;
            int count = tokensA.Length;
            int vectorSize = Vector<float>.Count; // e.g., 8 (AVX) or 4 (SSE)

            for (int i = 0; i < count; i++)
            {
                var a = tokensA[i].Values;
                var b = tokensB[i].Values;

                Vector<float> sumVector = Vector<float>.Zero;
                int j = 0;

                // Process chunks using SIMD
                int lastSimdIndex = TokenEmbedding.EmbeddingSize - (TokenEmbedding.EmbeddingSize % vectorSize);
                
                for (; j < lastSimdIndex; j += vectorSize)
                {
                    var va = new Vector<float>(a, j);
                    var vb = new Vector<float>(b, j);
                    sumVector += va * vb;
                }

                // Horizontal reduction of the vector
                float simdSum = 0;
                for (int k = 0; k < vectorSize; k++)
                {
                    simdSum += sumVector[k];
                }

                // Process remaining tail elements (scalar)
                for (; j < TokenEmbedding.EmbeddingSize; j++)
                {
                    simdSum += a[j] * b[j];
                }

                dotProduct += simdSum;
            }
            return dotProduct;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int count = 100_000;
            var rng = new Random(42);

            // Generate data
            var tokensA = new TokenEmbedding[count];
            var tokensB = new TokenEmbedding[count];
            for (int i = 0; i < count; i++)
            {
                tokensA[i] = new TokenEmbedding(rng);
                tokensB[i] = new TokenEmbedding(rng);
            }

            // Warmup
            SimilarityCalculator.CalculateSimilarityScalar(tokensA, tokensB);
            SimilarityCalculator.CalculateSimilaritySimd(tokensA, tokensB);

            // Benchmark Scalar
            var sw = Stopwatch.StartNew();
            float resultScalar = SimilarityCalculator.CalculateSimilarityScalar(tokensA, tokensB);
            long timeScalar = sw.ElapsedMilliseconds;

            // Benchmark SIMD
            sw.Restart();
            float resultSimd = SimilarityCalculator.CalculateSimilaritySimd(tokensA, tokensB);
            long timeSimd = sw.ElapsedMilliseconds;

            Console.WriteLine($"Scalar Result: {resultScalar:F4}, Time: {timeScalar}ms");
            Console.WriteLine($"SIMD Result:   {resultSimd:F4}, Time: {timeSimd}ms");
            Console.WriteLine($"Speedup: {timeScalar / (double)timeSimd:F2}x");
        }
    }
}
