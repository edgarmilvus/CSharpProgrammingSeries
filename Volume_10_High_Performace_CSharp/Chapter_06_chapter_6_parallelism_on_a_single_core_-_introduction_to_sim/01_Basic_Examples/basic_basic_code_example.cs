
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
using System.Numerics; // Required for Vector<T>
using System.Runtime.CompilerServices; // For MethodImplOptions.AggressiveInlining

public class VectorSimilarity
{
    public static void Main()
    {
        // 1. Setup: Create two sample embedding vectors (dimension 128 is common in AI models)
        // In a real scenario, these would be loaded from a model or database.
        const int dimension = 128;
        float[] vectorA = new float[dimension];
        float[] vectorB = new float[dimension];

        // Populate with dummy data (e.g., random values between 0 and 1)
        Random rand = new Random(42);
        for (int i = 0; i < dimension; i++)
        {
            vectorA[i] = (float)rand.NextDouble();
            vectorB[i] = (float)rand.NextDouble();
        }

        Console.WriteLine($"Processing vectors of dimension: {dimension}");
        Console.WriteLine($"Hardware Vector<T> Count (Simd Length): {Vector<float>.Count}");
        Console.WriteLine($"Is Hardware Acceleration Supported: {Vector.IsHardwareAccelerated}");
        Console.WriteLine(new string('-', 40));

        // 2. Execution: Run Scalar and SIMD versions
        double scalarSimilarity = CalculateCosineSimilarityScalar(vectorA, vectorB);
        double simdSimilarity = CalculateCosineSimilaritySimd(vectorA, vectorB);

        // 3. Validation: Compare results
        Console.WriteLine($"Scalar Result: {scalarSimilarity:F10}");
        Console.WriteLine($"SIMD Result:   {simdSimilarity:F10}");
        Console.WriteLine($"Difference:    {Math.Abs(scalarSimilarity - simdSimilarity):e5}");
    }

    /// <summary>
    /// Standard scalar implementation of Cosine Similarity.
    /// Formula: dot(A, B) / (sqrt(sum(A^2)) * sqrt(sum(B^2)))
    /// </summary>
    public static double CalculateCosineSimilarityScalar(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must be the same length.");

        double dotProduct = 0.0;
        double magnitudeA = 0.0;
        double magnitudeB = 0.0;

        // Process one element at a time
        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    /// <summary>
    /// Optimized SIMD implementation using Vector<T>.
    /// </summary>
    public static double CalculateCosineSimilaritySimd(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must be the same length.");

        int i = 0;
        int lastBlockIndex = a.Length - Vector<float>.Count;

        // Accumulators for the vector operations
        // We use Vector<float> which holds as many floats as the hardware supports (e.g., 4, 8, or 16)
        Vector<float> dotProductVec = Vector<float>.Zero;
        Vector<float> magnitudeAVec = Vector<float>.Zero;
        Vector<float> magnitudeBVec = Vector<float>.Zero;

        // 1. Main Loop: Process chunks of data using SIMD
        for (; i <= lastBlockIndex; i += Vector<float>.Count)
        {
            // Load contiguous memory blocks into Vectors
            Vector<float> va = new Vector<float>(a, i);
            Vector<float> vb = new Vector<float>(b, i);

            // Perform operations on the entire vector at once
            dotProductVec += va * vb;
            magnitudeAVec += va * va;
            magnitudeBVec += vb * vb;
        }

        // 2. Horizontal Reduction: Sum the values within the vectors
        // Vector.Dot is a hardware-accelerated way to sum the elements of a vector
        double dotProduct = Vector.Dot(dotProductVec);
        double magnitudeA = Vector.Dot(magnitudeAVec);
        double magnitudeB = Vector.Dot(magnitudeBVec);

        // 3. Remainder Loop: Process any leftover elements (if length isn't a multiple of Vector<float>.Count)
        for (; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        // 4. Final Calculation
        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }
}
