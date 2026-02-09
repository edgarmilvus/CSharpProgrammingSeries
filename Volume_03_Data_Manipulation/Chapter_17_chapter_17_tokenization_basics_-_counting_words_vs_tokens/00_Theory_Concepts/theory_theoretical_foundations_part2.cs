
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class VectorizedFilter
{
    public static int CountAlphanumeric(ReadOnlySpan<char> text)
    {
        int count = 0;
        int i = 0;
        
        // Determine the vector size based on hardware support.
        // Vector<char>.Count is usually 8 (for 128-bit registers holding 16-bit chars).
        int vectorLength = Vector<char>.Count;
        int lastIndex = text.Length - vectorLength;

        // Create a vector of the delimiter/space character to compare against.
        Vector<char> spaceVector = new Vector<char>(' ');
        
        // Process in chunks
        if (i <= lastIndex)
        {
            ref char startRef = ref text[i];
            // We take a reference to the start of the current chunk and treat it as a Vector
            ref Vector<char> currentVector = ref Unsafe.As<char, Vector<char>>(ref startRef);

            for (; i <= lastIndex; i += vectorLength)
            {
                // Load a vector from memory
                Vector<char> data = currentVector;
                
                // Compare all elements in the vector against the space.
                // This returns a mask where non-spaces are 0xFFFF (true).
                Vector<char> comparison = Vector.Equals(data, spaceVector);
                
                // Count the number of non-space characters (Vector<T> doesn't have a direct Count method for conditions,
                // so we often use a mask or compare against zero).
                // Simplified logic for demonstration:
                if (Vector.IsHardwareAccelerated)
                {
                    // In real scenarios, we use BitOperations.PopCount on the mask.
                    // This is a simplified abstraction of the SIMD logic.
                    // We are essentially checking 8 characters at once.
                    if (!Vector.EqualsAll(data, spaceVector))
                    {
                        // Logic to count valid alphanumeric chars in the vector
                        // This requires deeper intrinsics usage (e.g., Avx2.MoveMask)
                        // but illustrates the concept of batch processing.
                    }
                }
            }
        }

        // Process remaining elements (tail) scalarly
        for (; i < text.Length; i++)
        {
            if (char.IsLetterOrDigit(text[i])) count++;
        }

        return count;
    }
}
