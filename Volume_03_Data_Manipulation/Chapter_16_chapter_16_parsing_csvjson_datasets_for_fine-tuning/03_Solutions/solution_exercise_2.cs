
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

public class VectorSimilarity
{
    public static float CalculateCosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must be of the same dimension.");

        int length = a.Length;
        int vectorSize = Vector<float>.Count; 
        // Vector<float>.Count depends on hardware (e.g., 8 for AVX2, 4 for SSE)
        
        int i = 0;
        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        // SIMD Accumulators
        Vector<float> vDot = Vector<float>.Zero;
        Vector<float> vSumA = Vector<float>.Zero;
        Vector<float> vSumB = Vector<float>.Zero;

        // Process chunks using SIMD
        for (; i <= length - vectorSize; i += vectorSize)
        {
            var va = new Vector<float>(a.Slice(i, vectorSize));
            var vb = new Vector<float>(b.Slice(i, vectorSize));

            // Dot Product accumulation
            vDot += va * vb;

            // Magnitude accumulation (squared)
            vSumA += va * va;
            vSumB += vb * vb;
        }

        // Reduce SIMD vectors to scalars
        // This loop is small (max 8 iterations) and unrolls automatically
        for (int j = 0; j < vectorSize; j++)
        {
            dotProduct += vDot[j];
            magnitudeA += vSumA[j];
            magnitudeB += vSumB[j];
        }

        // Process the remainder (scalar fallback)
        // Handles cases where vector length is not a multiple of Vector<float>.Count
        for (; i < length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        // Calculate final norms
        magnitudeA = MathF.Sqrt(magnitudeA);
        magnitudeB = MathF.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (magnitudeA * magnitudeB);
    }

    public static void Main()
    {
        // Simulating 128-dimensional embeddings
        // Using stackalloc for zero heap allocation
        Span<float> embeddingA = stackalloc float[128];
        Span<float> embeddingB = stackalloc float[128];

        // Fill with dummy data
        for (int i = 0; i < 128; i++)
        {
            embeddingA[i] = (float)Math.Sin(i);
            embeddingB[i] = (float)Math.Cos(i);
        }

        float similarity = CalculateCosineSimilarity(embeddingA, embeddingB);
        Console.WriteLine($"Cosine Similarity: {similarity}");
    }
}
