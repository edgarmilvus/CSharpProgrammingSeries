
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Numerics; // Required for Vector<T> (SIMD)
using System.Runtime.CompilerServices; // Required for AggressiveInlining

public static class CosineSimilarityCalculator
{
    /// <summary>
    /// Calculates the cosine similarity between two vectors using standard loops.
    /// This serves as our baseline for performance comparison.
    /// </summary>
    /// <param name="vectorA">First vector (read-only span).</param>
    /// <param name="vectorB">Second vector (read-only span).</param>
    /// <returns>The cosine similarity score between -1.0 and 1.0.</returns>
    public static double CalculateStandard(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be of the same length.");

        double dotProduct = 0.0;
        double magnitudeA = 0.0;
        double magnitudeB = 0.0;

        // Standard sequential loop
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0.0;

        return dotProduct / (magnitudeA * magnitudeB);
    }

    /// <summary>
    /// Calculates cosine similarity using SIMD (Single Instruction, Multiple Data) 
    /// and Span<T> for zero-allocation memory management.
    /// </summary>
    public static double CalculateSimd(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be of the same length.");

        // 1. Define the vector type (Vector<float>) which maps to hardware registers 
        // (e.g., 128-bit SSE or 256-bit AVX on x64).
        int length = vectorA.Length;
        int vectorSize = Vector<float>.Count; 
        int i = 0;

        // 2. Initialize accumulators using SIMD vectors. 
        // These hold partial sums for the dot product and magnitudes.
        Vector<float> dotProductVec = Vector<float>.Zero;
        Vector<float> magnitudeAVec = Vector<float>.Zero;
        Vector<float> magnitudeBVec = Vector<float>.Zero;

        // 3. Process data in blocks matching the hardware vector width.
        for (; i <= length - vectorSize; i += vectorSize)
        {
            // Load a block of data from memory into CPU registers.
            // 'Unsafe' is used here for raw pointer access, avoiding bounds checks 
            // inside the tight loop for maximum performance.
            var aBlock = Unsafe.ReadUnaligned<Vector<float>>(ref Unsafe.As<float, byte>(ref vectorA[i]));
            var bBlock = Unsafe.ReadUnaligned<Vector<float>>(ref Unsafe.As<float, byte>(ref vectorB[i]));

            // 4. Perform SIMD operations:
            // - Multiply vectors element-wise in parallel.
            // - Add the results to the running accumulators.
            dotProductVec += aBlock * bBlock;
            magnitudeAVec += aBlock * aBlock;
            magnitudeBVec += bBlock * bBlock;
        }

        // 5. Horizontal Reduction:
        // SIMD registers are wide (e.g., 8 floats). We need to sum the values 
        // within the register to get a single scalar value.
        double dotProduct = 0.0;
        double magnitudeA = 0.0;
        double magnitudeB = 0.0;

        for (int j = 0; j < vectorSize; j++)
        {
            dotProduct += dotProductVec[j];
            magnitudeA += magnitudeAVec[j];
            magnitudeB += magnitudeBVec[j];
        }

        // 6. Process the remaining elements (the "tail") using standard scalar operations.
        for (; i < length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0.0;

        return dotProduct / (magnitudeA * magnitudeB);
    }

    public static void Main()
    {
        // Context: Comparing user queries against document embeddings in a search engine.
        // We have two vectors representing semantic meaning.
        
        // Generate dummy data (e.g., 1024-dimensional embeddings)
        int dimensions = 1024;
        float[] vecAData = new float[dimensions];
        float[] vecBData = new float[dimensions];
        
        var rng = new Random(42);
        for(int i = 0; i < dimensions; i++)
        {
            vecAData[i] = (float)rng.NextDouble();
            vecBData[i] = (float)rng.NextDouble();
        }

        // Convert to Spans (Zero-allocation view over existing memory)
        ReadOnlySpan<float> vecA = vecAData;
        ReadOnlySpan<float> vecB = vecBData;

        Console.WriteLine($"Calculating Cosine Similarity for {dimensions} dimensions...\n");

        // Run Standard Calculation
        var sw = System.Diagnostics.Stopwatch.StartNew();
        double resultStandard = CalculateStandard(vecA, vecB);
        sw.Stop();
        Console.WriteLine($"Standard Loop Result: {resultStandard:F6}");
        Console.WriteLine($"Time Taken: {sw.Elapsed.TotalMilliseconds:F2} ms\n");

        // Run SIMD Calculation
        sw.Restart();
        double resultSimd = CalculateSimd(vecA, vecB);
        sw.Stop();
        Console.WriteLine($"SIMD + Span Result:    {resultSimd:F6}");
        Console.WriteLine($"Time Taken: {sw.Elapsed.TotalMilliseconds:F2} ms");
        
        // Verify correctness
        if (Math.Abs(resultStandard - resultSimd) < 0.00001)
            Console.WriteLine("\nResults match! Optimization successful.");
        else
            Console.WriteLine("\nWarning: Results do not match.");
    }
}
