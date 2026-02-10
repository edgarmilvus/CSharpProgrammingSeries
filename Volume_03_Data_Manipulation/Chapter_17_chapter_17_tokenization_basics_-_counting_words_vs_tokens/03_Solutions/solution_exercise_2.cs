
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
using System.Runtime.Intrinsics.X86;

public class SimdTokenizer
{
    public static void FindDelimitersSimd(char[] buffer, bool[] delimiters)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            Console.WriteLine("SIMD not supported on this hardware. Falling back to scalar.");
            // Fallback scalar loop
            for (int i = 0; i < buffer.Length; i++)
            {
                delimiters[i] = !char.IsLetterOrDigit(buffer[i]);
            }
            return;
        }

        int i = 0;
        int vectorSize = Vector<ushort>.Count; // Typically 8 for AVX2 (256-bit) or 4 for SSE (128-bit)
        int lastIndex = buffer.Length - vectorSize;

        // Process in vector-sized blocks
        for (; i <= lastIndex; i += vectorSize)
        {
            // Load vector from memory. char is 2 bytes, so we use ushort.
            var vector = new Vector<ushort>(buffer, i);

            // We need to check if characters are NOT letters/digits.
            // Vector<T> doesn't have direct IsLetterOrDigit, so we check ASCII ranges.
            // A: 65, Z: 90, a: 97, z: 122, 0: 48, 9: 57
            
            // Create comparison vectors (broadcasts value to all elements)
            var lowerBoundA = new Vector<ushort>(65);
            var upperBoundZ = new Vector<ushort>(90);
            var lowerBoundA2 = new Vector<ushort>(97);
            var upperBoundZ2 = new Vector<ushort>(122);
            var lowerBound0 = new Vector<ushort>(48);
            var upperBound9 = new Vector<ushort>(57);

            // Vectorized comparison (Single Instruction, Multiple Data)
            var isUpper = Vector.GreaterThanOrEqual(vector, lowerBoundA) & Vector.LessThanOrEqual(vector, upperBoundZ);
            var isLower = Vector.GreaterThanOrEqual(vector, lowerBoundA2) & Vector.LessThanOrEqual(vector, upperBoundZ2);
            var isDigit = Vector.GreaterThanOrEqual(vector, lowerBound0) & Vector.LessThanOrEqual(vector, upperBound9);

            var isAlphanumeric = isUpper | isLower | isDigit;
            
            // Convert boolean vector to boolean array.
            // Note: Vector<bool> is not directly indexable. We extract the underlying bits
            // or iterate the vector elements. For clarity and portability, we copy to a stack buffer.
            
            Span<ushort> vecSpan = stackalloc ushort[vectorSize];
            vector.CopyTo(vecSpan);

            // Since we performed logical operations on the vector, we need to re-evaluate the logic
            // on the extracted elements to populate the boolean array, or use unsafe bit manipulation.
            // For this exercise, we re-evaluate the logic on the extracted scalars to ensure correctness.
            for (int j = 0; j < vectorSize; j++)
            {
                ushort val = vecSpan[j];
                bool isAlpha = (val >= 65 && val <= 90) || 
                               (val >= 97 && val <= 122) || 
                               (val >= 48 && val <= 97);
                
                delimiters[i + j] = !isAlpha;
            }
        }

        // Handle remaining elements (tail) that don't fit in a vector
        for (; i < buffer.Length; i++)
        {
            delimiters[i] = !char.IsLetterOrDigit(buffer[i]);
        }
    }

    public static void Run()
    {
        string text = "AI, Data, Vectorization! 2024";
        char[] buffer = text.ToCharArray();
        bool[] delimiters = new bool[buffer.Length];

        FindDelimitersSimd(buffer, delimiters);

        Console.WriteLine("SIMD Delimiter Detection (1=Delimiter):");
        for (int i = 0; i < buffer.Length; i++)
        {
            Console.Write(delimiters[i] ? "1" : "0");
        }
        Console.WriteLine();
    }
}
