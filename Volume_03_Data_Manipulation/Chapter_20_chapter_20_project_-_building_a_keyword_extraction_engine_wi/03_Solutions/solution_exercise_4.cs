
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Numerics;

public class VectorizedScorer
{
    public static float CalculateSimilaritySimd(Span<float> vectorA, Span<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be of equal length.");

        int i = 0;
        int length = vectorA.Length;
        int vectorWidth = Vector<float>.Count; // e.g., 8 floats for AVX2 (256-bit)

        // Vector accumulator initialized to zero
        Vector<float> sum = Vector<float>.Zero;

        // Main SIMD loop
        // We process 'vectorWidth' elements per iteration
        for (; i <= length - vectorWidth; i += vectorWidth)
        {
            // Load vectors from memory into CPU registers
            var va = new Vector<float>(vectorA.Slice(i));
            var vb = new Vector<float>(vectorB.Slice(i));

            // Multiply element-wise and add to the accumulator
            // This compiles to vmulps + vaddps (SIMD instructions)
            sum += va * vb;
        }

        // Horizontal reduction: Sum the elements within the Vector<float> register
        float result = 0.0f;
        for (int j = 0; j < vectorWidth; j++)
        {
            result += sum[j];
        }

        // Scalar fallback for remaining elements
        for (; i < length; i++)
        {
            result += vectorA[i] * vectorB[i];
        }

        return result;
    }
}
