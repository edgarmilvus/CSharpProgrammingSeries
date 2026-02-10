
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
using System.Runtime.CompilerServices;

public class TokenNormalizer
{
    /// <summary>
    /// Checks if the span is aligned to the vector size in bytes.
    /// </summary>
    private static bool IsAligned<T>(Span<T> span) where T : struct
    {
        unsafe
        {
            // Get the address of the first element
            IntPtr ptr = Unsafe.As<T, IntPtr>(ref span[0]);
            
            // Alignment is determined by the vector width in bytes
            // Vector<T>.Count * sizeof(T)
            int alignment = Vector<T>.Count * Unsafe.SizeOf<T>();
            
            return (ptr.ToInt64() % alignment) == 0;
        }
    }

    public static void NormalizeTokens(ReadOnlySpan<short> tokens, Span<float> result, float minVal, float maxVal)
    {
        if (tokens.Length != result.Length)
            throw new ArgumentException("Spans must be same length.");

        int vectorWidthShort = Vector<short>.Count; // 8 on 64-bit
        int vectorWidthFloat = Vector<float>.Count; // 4 on 64-bit
        
        // Since we are converting short (2 bytes) to float (4 bytes), 
        // the memory layout changes. Alignment of the *input* is important for loading shorts.
        // Alignment of the *output* is important for storing floats.
        
        int i = 0;

        // 1. Handle Alignment (Prologue)
        // We check alignment of the input span. If unaligned, we process scalarly 
        // until we reach an aligned boundary.
        if (!IsAligned(tokens))
        {
            // Calculate bytes to alignment
            unsafe
            {
                IntPtr ptr = Unsafe.As<short, IntPtr>(ref tokens[0]);
                int alignment = Vector<short>.Count * sizeof(short);
                int bytesToAlign = (alignment - (ptr.ToInt64() % alignment)) % alignment;
                int elementsToAlign = bytesToAlign / sizeof(short);

                // Process scalar prologue
                for (int k = 0; k < elementsToAlign && i < tokens.Length; k++, i++)
                {
                    float val = tokens[i];
                    val = (val - minVal) / (maxVal - minVal);
                    result[i] = val;
                }
            }
        }

        // 2. Vectorized Bulk Processing
        // We need to match the vector widths. Vector<short> has 8 elements, Vector<float> has 4.
        // We can load 8 shorts, convert to 8 floats (using Vector.ConvertToSingle), 
        // but we need to store them in chunks of 4.
        
        // Note: Vector.ConvertToSingle requires Vector<long> or similar in some older implementations,
        // but in .NET 8, overloads exist. However, converting short->float directly via Vector<short> 
        // isn't a standard method. We usually convert Vector<short> -> Vector<int> -> Vector<float>.
        
        // Strategy: Process 2 Vector<short> blocks (8 shorts) to create 2 Vector<float> blocks (8 floats).
        
        int shortVectorCount = Vector<short>.Count; // 8
        int floatVectorCount = Vector<float>.Count; // 4

        // We loop while we have enough data for at least one Vector<float> (4 floats / 4 shorts)
        // But to be efficient with alignment, we process full Vector<short> blocks (8 shorts).
        
        while (i <= tokens.Length - shortVectorCount)
        {
            // Load 8 shorts
            var shortVec = new Vector<short>(tokens.Slice(i, shortVectorCount));

            // Convert to Floats. 
            // .NET 8 Vector API allows direct conversion or widening.
            // Since Vector<float>.Count is usually half of Vector<short>.Count on 64-bit,
            // we need to split the result.
            
            // We can use Widen to convert Vector<short> -> Vector<int> (2 vectors)
            Vector<int> intLow;
            Vector<int> intHigh;
            Vector.Widen(shortVec, out intLow, out intHigh);

            // Convert Int to Float
            Vector<float> floatLow = Vector.ConvertToSingle(intLow);
            Vector<float> floatHigh = Vector.ConvertToSingle(intHigh);

            // Apply Normalization: (val - min) / (max - min)
            // Broadcast scalars to vectors
            var minVec = new Vector<float>(minVal);
            var maxVec = new Vector<float>(maxVal);
            var rangeVec = Vector.Subtract(maxVec, minVec);

            // Calculate: (val - min) / range
            floatLow = Vector.Divide(Vector.Subtract(floatLow, minVec), rangeVec);
            floatHigh = Vector.Divide(Vector.Subtract(floatHigh, minVec), rangeVec);

            // Store results
            floatLow.CopyTo(result.Slice(i, floatVectorCount));
            floatHigh.CopyTo(result.Slice(i + floatVectorCount, floatVectorCount));

            i += shortVectorCount;
        }

        // 3. Tail Elements (Scalar Fallback)
        for (; i < tokens.Length; i++)
        {
            float val = tokens[i];
            val = (val - minVal) / (maxVal - minVal);
            result[i] = val;
        }
    }
}
