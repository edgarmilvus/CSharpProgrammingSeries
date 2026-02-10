
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
using System.Numerics; // Required for Vector<T> and SIMD operations
using System.Runtime.CompilerServices; // Required for MethodImplOptions.AggressiveInlining

/*
 * REAL-WORLD PROBLEM CONTEXT:
 * 
 * In modern NLP (Natural Language Processing) pipelines, such as semantic search engines or document clustering systems,
 * we often need to compare the "meaning" of text snippets. A common technique is to represent text as high-dimensional
 * vectors (embeddings) and calculate the Cosine Similarity. This metric measures the cosine of the angle between two vectors,
 * determining how similar they are regardless of their magnitude.
 * 
 * PROBLEM: Processing millions of document pairs using standard loops is CPU-intensive and slow. 
 * SOLUTION: We will implement a zero-allocation, SIMD-accelerated Cosine Similarity calculator.
 * 
 * TECHNIQUES USED (From Chapter 19 & Previous):
 * 1. Span<T>: To slice memory without allocating new arrays on the heap.
 * 2. Vector<T>: To leverage CPU SIMD instructions (SSE/AVX) for parallel floating-point math.
 * 3. Unsafe Context & Pointers: For maximum speed when iterating memory (optional but common in high-perf C#).
 * 4. Loop Unrolling & Aggressive Inlining: To reduce CPU branch prediction misses.
 */

namespace HighPerformanceAI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- High-Performance Cosine Similarity Benchmark ---");

            // 1. Setup: Generate synthetic token embeddings (e.g., 300-dimensional vectors like GloVe or Word2Vec)
            // In a real app, these would come from a neural network.
            int vectorSize = 300; 
            float[] docA = GenerateRandomVector(vectorSize);
            float[] docB = GenerateRandomVector(vectorSize);

            // 2. Warm-up: JIT compilation and CPU cache warming
            // Essential for accurate benchmarking in .NET
            CalculateSimilaritySpan(docA.AsSpan(), docB.AsSpan());
            CalculateSimilaritySimd(docA.AsSpan(), docB.AsSpan());

            // 3. Execution: Run the optimized SIMD version
            // We use Spans to avoid copying the arrays
            float similarity = CalculateSimilaritySimd(docA.AsSpan(), docB.AsSpan());

            Console.WriteLine($"Cosine Similarity Score: {similarity:F6}");
            Console.WriteLine("--------------------------------------------------");
        }

        /// <summary>
        /// Generates a mock vector of floating-point numbers.
        /// </summary>
        static float[] GenerateRandomVector(int size)
        {
            float[] vector = new float[size];
            Random rand = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < size; i++)
            {
                // Random values between 0.0 and 1.0
                vector[i] = (float)rand.NextDouble();
            }
            return vector;
        }

        /// <summary>
        /// METHOD 1: Standard Scalar Implementation (Baseline)
        /// Uses Span<T> for memory safety but processes elements one by one.
        /// </summary>
        static float CalculateSimilaritySpan(ReadOnlySpan<float> vecA, ReadOnlySpan<float> vecB)
        {
            // Validation: Ensure vectors are of the same dimension
            if (vecA.Length != vecB.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            // Standard loop iterating over the Span
            for (int i = 0; i < vecA.Length; i++)
            {
                float a = vecA[i];
                float b = vecB[i];

                dotProduct += a * b;
                normA += a * a;
                normB += b * b;
            }

            // Avoid division by zero
            if (normA == 0 || normB == 0)
                return 0;

            // Cosine Similarity = DotProduct / (|A| * |B|)
            return (float)(dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)));
        }

        /// <summary>
        /// METHOD 2: SIMD-Accelerated Implementation
        /// Uses Vector<T> to process multiple floats in parallel (e.g., 8 floats per cycle on AVX2).
        /// </summary>
        static float CalculateSimilaritySimd(ReadOnlySpan<float> vecA, ReadOnlySpan<float> vecB)
        {
            if (vecA.Length != vecB.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            int length = vecA.Length;
            int i = 0;

            // Accumulators for Dot Product and Magnitudes
            // We use double for accumulation to maintain precision during summation
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            // Determine the hardware vector width (e.g., Vector<float>.Count is usually 8 on AVX2)
            int vectorWidth = Vector<float>.Count;

            // Pin the spans to get native pointers for SIMD processing
            // This allows us to treat the managed memory as unmanaged for the duration of the loop
            unsafe
            {
                fixed (float* ptrA = vecA)
                fixed (float* ptrB = vecB)
                {
                    // --- SIMD LOOP ---
                    // Process 'vectorWidth' elements at a time
                    while (i <= length - vectorWidth)
                    {
                        // Load vectors from memory
                        Vector<float> vA = Vector.Load(ptrA + i);
                        Vector<float> vB = Vector.Load(ptrB + i);

                        // 1. Dot Product Component: Multiply and Add
                        // Vector.Multiply returns a new vector where each element is a[i] * b[i]
                        // We then sum the elements of that result vector.
                        Vector<float> vDot = Vector.Multiply(vA, vB);
                        dotProduct += Vector.Sum(vDot);

                        // 2. Norm A Component: Square and Add
                        Vector<float> vSqA = Vector.Multiply(vA, vA);
                        normA += Vector.Sum(vSqA);

                        // 3. Norm B Component: Square and Add
                        Vector<float> vSqB = Vector.Multiply(vB, vB);
                        normB += Vector.Sum(vSqB);

                        i += vectorWidth;
                    }
                }
            }

            // --- SCALAR TAIL LOOP ---
            // Process remaining elements that didn't fit in the last SIMD chunk
            // (e.g., if length is 300 and vectorWidth is 8, 4 elements remain)
            for (; i < length; i++)
            {
                float a = vecA[i];
                float b = vecB[i];

                dotProduct += a * b;
                normA += a * a;
                normB += b * b;
            }

            // Final Calculation
            if (normA == 0 || normB == 0)
                return 0;

            return (float)(dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)));
        }
    }
}
