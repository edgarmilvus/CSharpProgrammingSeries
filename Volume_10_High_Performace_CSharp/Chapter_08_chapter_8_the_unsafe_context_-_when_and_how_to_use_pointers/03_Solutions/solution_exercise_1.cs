
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

public static class UnsafeBufferOps
{
    public static void NormalizeL2Unsafe(float[] data)
    {
        // Guard clause for null or empty arrays
        if (data == null || data.Length == 0)
        {
            return;
        }

        // Use 'unsafe' to allow pointer operations
        unsafe
        {
            // 'fixed' pins the array in memory to prevent the GC from moving it
            // during the pointer operations. It returns a pointer to the first element.
            fixed (float* ptr = &data[0])
            {
                float* current = ptr;
                float sumSquares = 0.0f;

                // First pass: Calculate the sum of squares
                for (int i = 0; i < data.Length; i++)
                {
                    float val = *current;
                    sumSquares += val * val;
                    current++; // Increment pointer by sizeof(float)
                }

                // Edge Case: Check for zero norm to avoid division by zero
                if (sumSquares < float.Epsilon)
                {
                    return;
                }

                // Calculate L2 Norm (Square Root of Sum of Squares)
                float norm = MathF.Sqrt(sumSquares);

                // Reset pointer to the start of the array
                current = ptr;

                // Second pass: Normalize each element
                for (int i = 0; i < data.Length; i++)
                {
                    *current /= norm;
                    current++;
                }
            }
        }
    }
}
