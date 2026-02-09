
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HighPerformanceCSharp.SIMD
{
    /// <summary>
    /// Real-world context: AI Tokenization Optimization.
    /// Modern AI models process text by breaking it into "tokens" (sub-words).
    /// A common bottleneck is calculating the "similarity" or "distance" between 
    /// token embeddings (arrays of floating-point numbers) to find the closest match 
    /// in a vocabulary.
    /// 
    /// This application simulates processing a batch of 1024 token embeddings 
    /// against a target embedding to find the best match using Euclidean Distance.
    /// We compare a standard scalar implementation against a Vector<T> (SIMD) implementation.
    /// </summary>
    class Program
    {
        // Configuration for our simulated AI model context
        const int EmbeddingSize = 128; // Size of the vector representing a token
        const int VocabularySize = 1024; // Number of tokens to search through
        const int VectorSizeBytes = 32; // AVX2 vector size (256 bits = 32 bytes)

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing AI Token Embeddings...");
            Console.WriteLine($"Vector<T> Is Hardware Accelerated: {Vector.IsHardwareAccelerated}");
            Console.WriteLine($"Vector<float>.Count (elements per vector): {Vector<float>.Count}");
            Console.WriteLine(new string('-', 40));

            // 1. Setup: Generate random embedding data (simulating AI model output)
            // We use a fixed seed for reproducibility in this demo.
            Random rng = new Random(42);
            float[][] vocabulary = GenerateVocabulary(rng);
            float[] targetEmbedding = GenerateTarget(rng);

            // 2. Execution: Run Scalar Algorithm
            Console.WriteLine("Running Scalar Euclidean Distance Calculation...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int bestMatchScalar = FindBestMatchScalar(vocabulary, targetEmbedding);
            sw.Stop();
            long scalarTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"Best Match Index: {bestMatchScalar} | Time: {scalarTime}ms");

            // 3. Execution: Run SIMD Algorithm
            Console.WriteLine("Running SIMD Euclidean Distance Calculation...");
            sw.Restart();
            int bestMatchSimd = FindBestMatchSimd(vocabulary, targetEmbedding);
            sw.Stop();
            long simdTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"Best Match Index: {bestMatchSimd} | Time: {simdTime}ms");

            // 4. Validation and Comparison
            Console.WriteLine(new string('-', 40));
            if (bestMatchScalar == bestMatchSimd)
            {
                Console.WriteLine("SUCCESS: Algorithms found the same best match.");
                if (scalarTime > 0 && simdTime > 0)
                {
                    double speedup = (double)scalarTime / simdTime;
                    Console.WriteLine($"SIMD Speedup: {speedup:F2}x faster");
                }
            }
            else
            {
                Console.WriteLine("WARNING: Results mismatch. Check implementation.");
            }
        }

        // ---------------------------------------------------------
        // DATA GENERATION (Standard C# Arrays)
        // ---------------------------------------------------------

        static float[][] GenerateVocabulary(Random rng)
        {
            float[][] vocab = new float[VocabularySize][];
            for (int i = 0; i < VocabularySize; i++)
            {
                vocab[i] = new float[EmbeddingSize];
                for (int j = 0; j < EmbeddingSize; j++)
                {
                    // Random floats between 0 and 1
                    vocab[i][j] = (float)rng.NextDouble();
                }
            }
            return vocab;
        }

        static float[] GenerateTarget(Random rng)
        {
            float[] target = new float[EmbeddingSize];
            for (int i = 0; i < EmbeddingSize; i++)
            {
                target[i] = (float)rng.NextDouble();
            }
            return target;
        }

        // ---------------------------------------------------------
        // SCALAR IMPLEMENTATION (Baseline)
        // ---------------------------------------------------------

        /// <summary>
        /// Calculates Euclidean distance using standard scalar operations.
        /// Euclidean Distance = Sqrt(Sum((a[i] - b[i])^2))
        /// </summary>
        static int FindBestMatchScalar(float[][] vocabulary, float[] target)
        {
            int bestIndex = -1;
            double minDistance = double.MaxValue;

            // Iterate through every token in the vocabulary
            for (int i = 0; i < VocabularySize; i++)
            {
                double sumSqDiff = 0.0;
                float[] currentToken = vocabulary[i];

                // Inner loop: Iterate through each dimension of the embedding
                for (int j = 0; j < EmbeddingSize; j++)
                {
                    float diff = currentToken[j] - target[j];
                    sumSqDiff += diff * diff;
                }

                // Compare with current minimum
                if (sumSqDiff < minDistance)
                {
                    minDistance = sumSqDiff;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        // ---------------------------------------------------------
        // SIMD IMPLEMENTATION (Optimized)
        // ---------------------------------------------------------

        /// <summary>
        /// Calculates Euclidean distance using Vector<T> for SIMD parallelism.
        /// Key Concept: Processes multiple floats (e.g., 8 at a time on AVX2) in a single CPU instruction.
        /// </summary>
        static int FindBestMatchSimd(float[][] vocabulary, float[] target)
        {
            int bestIndex = -1;
            double minDistance = double.MaxValue;

            // Pre-load the target into a vector array for efficient access.
            // We treat the target as a span of vectors to align processing.
            Span<Vector<float>> targetVectors = MemoryMarshal.Cast<float, Vector<float>>(target.AsSpan());

            for (int i = 0; i < VocabularySize; i++)
            {
                float[] currentToken = vocabulary[i];
                Span<Vector<float>> tokenVectors = MemoryMarshal.Cast<float, Vector<float>>(currentToken.AsSpan());
                
                // Accumulator for the partial sums of squared differences.
                // We use Vector<float> to hold the running sum for each vector lane.
                Vector<float> partialSums = Vector<float>.Zero;

                // Process the data in Vector<float>.Count chunks
                int j = 0;
                int limit = tokenVectors.Length;

                // SIMD Loop
                for (; j < limit; j++)
                {
                    // Load vectors from memory
                    Vector<float> tokenVec = tokenVectors[j];
                    Vector<float> targetVec = targetVectors[j];

                    // Calculate difference
                    Vector<float> diff = tokenVec - targetVec;

                    // Square the difference (Element-wise multiplication)
                    Vector<float> squared = Vector.Multiply(diff, diff);

                    // Accumulate
                    partialSums = Vector.Add(partialSums, squared);
                }

                // Horizontal Sum: Reduce the Vector<float> to a single scalar value.
                // Note: Vector<T> does not have a built-in Sum() method in older .NET versions,
                // so we iterate through the underlying elements.
                double sumSqDiff = 0.0;
                for (int k = 0; k < Vector<float>.Count; k++)
                {
                    sumSqDiff += partialSums[k];
                }

                // Handle "Tail" elements (if EmbeddingSize is not perfectly divisible by Vector<float>.Count)
                // In this specific implementation, we assumed 128 is divisible by 8 (AVX).
                // However, for robustness, we should handle the remainder.
                // (For brevity in this specific script, we rely on alignment, but real code needs a tail loop).

                if (sumSqDiff < minDistance)
                {
                    minDistance = sumSqDiff;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }
    }
}
