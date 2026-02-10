
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

// Struct to hold pre-computed magnitude
public readonly struct CachedEmbedding
{
    public ReadOnlySpan<float> Data { get; }
    public float Magnitude { get; }

    public CachedEmbedding(ReadOnlySpan<float> data)
    {
        Data = data;
        // Pre-compute magnitude once upon creation
        Magnitude = CalculateMagnitude(data);
    }

    private static float CalculateMagnitude(ReadOnlySpan<float> span)
    {
        double sumSq = 0;
        int count = Vector<float>.Count;
        int i = 0;
        int limit = span.Length - count;

        if (limit >= 0)
        {
            Vector<float> vecSq = Vector<float>.Zero;
            for (i = 0; i <= limit; i += count)
            {
                var v = new Vector<float>(span.Slice(i, count));
                vecSq += v * v;
            }
            sumSq += Vector.Sum(vecSq);
        }

        for (; i < span.Length; i++)
        {
            sumSq += span[i] * span[i];
        }

        return (float)Math.Sqrt(sumSq);
    }
}

public static class OptimizedCosineSimilarity
{
    // Static check moved outside the hot path
    private static readonly bool IsSimdSupported = Vector.IsHardwareAccelerated;
    private static readonly int VectorSize = Vector<float>.Count;

    public static float Calculate(CachedEmbedding left, CachedEmbedding right)
    {
        if (left.Data.Length != right.Data.Length)
            throw new ArgumentException("Embeddings must be same dimension.");

        // Safety check for zero vectors
        if (left.Magnitude == 0 || right.Magnitude == 0)
            return 0;

        double dotProduct = 0;
        
        // Branch Prediction Optimization: 
        // Since IsSimdSupported is static, the JIT will likely 
        // compile only the relevant path, removing the branch from the loop.
        if (IsSimdSupported)
        {
            int i = 0;
            int limit = left.Data.Length - VectorSize;
            
            if (limit >= 0)
            {
                Vector<float> vecDot = Vector<float>.Zero;
                for (i = 0; i <= limit; i += VectorSize)
                {
                    var vL = new Vector<float>(left.Data.Slice(i, VectorSize));
                    var vR = new Vector<float>(right.Data.Slice(i, VectorSize));
                    vecDot += vL * vR;
                }
                dotProduct += Vector.Sum(vecDot);
            }

            // Tail loop
            for (; i < left.Data.Length; i++)
            {
                dotProduct += left.Data[i] * right.Data[i];
            }
        }
        else
        {
            // Fallback scalar loop (omitted for brevity, similar to previous exercises)
            for (int i = 0; i < left.Data.Length; i++)
            {
                dotProduct += left.Data[i] * right.Data[i];
            }
        }

        return (float)(dotProduct / (left.Magnitude * right.Magnitude));
    }
}
