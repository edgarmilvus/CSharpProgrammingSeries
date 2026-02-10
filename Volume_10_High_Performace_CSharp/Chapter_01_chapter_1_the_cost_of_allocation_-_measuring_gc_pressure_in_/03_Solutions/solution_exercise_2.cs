
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class SimdSearch
{
    // Standard scalar search for comparison
    public static int FindTokenIndexScalar(ReadOnlySpan<int> vocabulary, int targetToken)
    {
        for (int i = 0; i < vocabulary.Length; i++)
        {
            if (vocabulary[i] == targetToken)
            {
                return i;
            }
        }
        return -1;
    }

    // SIMD Search Implementation
    public static int FindTokenIndexSimd(ReadOnlySpan<int> vocabulary, int targetToken)
    {
        int i = 0;
        int length = vocabulary.Length;
        
        // Vector width depends on the hardware (e.g., 256-bit AVX2 = 8 ints, 128-bit SSE = 4 ints)
        int vectorWidth = Vector<int>.Count;
        
        // Create a vector containing the target token repeated
        Vector<int> targetVector = new Vector<int>(targetToken);

        // Process chunks of vectorWidth
        int lastSIMDIndex = length - vectorWidth;
        
        // Handle Misalignment:
        // We iterate only up to the point where we can safely read a full vector.
        // The remaining elements (misalignment) are handled by the scalar loop afterwards.
        for (; i <= lastSIMDIndex; i += vectorWidth)
        {
            // Load vector from memory
            Vector<int> sourceVector = new Vector<int>(vocabulary.Slice(i, vectorWidth));
            
            // Compare vectors. The result is a mask of 0xFFFFFFFF (true) or 0 (false) per element.
            Vector<int> comparisonResult = Vector.Equals(sourceVector, targetVector);

            // Check if any element in the comparison result is true (non-zero).
            // Vector.IsAllZero returns true if all elements are zero.
            if (!Vector.IsAllZero(comparisonResult))
            {
                // We found a match in this chunk. Now find which specific index.
                // This is a fallback to scalar for the specific chunk to find the exact offset.
                for (int j = 0; j < vectorWidth; j++)
                {
                    if (vocabulary[i + j] == targetToken)
                    {
                        return i + j;
                    }
                }
            }
        }

        // Handle the remainder (tail) scalarly
        for (; i < length; i++)
        {
            if (vocabulary[i] == targetToken)
            {
                return i;
            }
        }

        return -1;
    }
}
