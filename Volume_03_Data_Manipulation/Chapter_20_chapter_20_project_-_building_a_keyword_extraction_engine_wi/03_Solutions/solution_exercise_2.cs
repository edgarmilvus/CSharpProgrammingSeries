
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Numerics;

public class SimdTextProcessor
{
    public static int FilterAlphanumeric(ReadOnlySpan<char> source, Span<char> destination)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            // Fallback to scalar if SIMD is not supported
            return FilterScalar(source, destination);
        }

        int destIndex = 0;
        int i = 0;
        int width = Vector<ushort>.Count; // e.g., 16 chars on 256-bit AVX2

        // Process vector-wise
        while (i <= source.Length - width)
        {
            // Load 16 characters (assuming ushort vector) into a CPU register
            var vector = new Vector<ushort>(source.Slice(i));

            // In true SIMD, we would generate a bitmask here.
            // Since Vector<T> API is limited in C# without unsafe code, 
            // we extract elements to check validity.
            // Note: This loop inside the vector block is for demonstration of the API usage.
            // In raw SIMD assembly, we would use Compare and Blend instructions.
            
            for (int j = 0; j < width; j++)
            {
                char c = (char)vector[j];
                if (char.IsLetterOrDigit(c))
                {
                    if (destIndex < destination.Length)
                        destination[destIndex++] = c;
                }
            }

            i += width;
        }

        // Scalar fallback for remaining elements
        for (; i < source.Length; i++)
        {
            if (char.IsLetterOrDigit(source[i]))
            {
                if (destIndex < destination.Length)
                    destination[destIndex++] = source[i];
            }
        }

        return destIndex;
    }

    private static int FilterScalar(ReadOnlySpan<char> source, Span<char> destination)
    {
        int destIndex = 0;
        for (int i = 0; i < source.Length; i++)
        {
            if (char.IsLetterOrDigit(source[i]))
            {
                if (destIndex < destination.Length)
                    destination[destIndex++] = source[i];
            }
        }
        return destIndex;
    }
}
