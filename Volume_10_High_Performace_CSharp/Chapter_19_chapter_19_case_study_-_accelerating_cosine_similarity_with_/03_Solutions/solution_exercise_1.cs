
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public static class VectorizedMath
{
    public static float DotProductManual(float[] left, float[] right)
    {
        if (left == null || right == null)
            throw new ArgumentNullException("Input arrays cannot be null.");
        if (left.Length != right.Length)
            throw new ArgumentException("Input arrays must be of the same length.");

        int count = Vector<float>.Count;
        int i = 0;
        float sum = 0;

        // Main loop: Process in chunks of Vector<float>.Count
        // We calculate the limit to avoid reading past the end of the array
        int lastSIMDIndex = left.Length - count;
        
        if (lastSIMDIndex >= 0)
        {
            Vector<float> sumVector = Vector<float>.Zero;

            // Manual unrolling or stride-based iteration
            for (i = 0; i <= lastSIMDIndex; i += count)
            {
                var leftVec = new Vector<float>(left, i);
                var rightVec = new Vector<float>(right, i);
                sumVector += leftVec * rightVec;
            }

            // Horizontal reduction of the vector
            sum = Vector.Sum(sumVector);
        }

        // Tail loop: Process remaining elements that didn't fit in a full vector
        for (; i < left.Length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }
}
