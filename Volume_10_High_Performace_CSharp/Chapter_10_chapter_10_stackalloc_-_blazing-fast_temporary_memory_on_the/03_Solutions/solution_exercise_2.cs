
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
using System.Runtime.InteropServices;

public static class SimdWhitespace
{
    public static void ComputeWhitespaceMask(ReadOnlySpan<byte> utf8Input, Span<bool> maskOutput)
    {
        if (utf8Input.Length != maskOutput.Length)
            throw new ArgumentException("Input and output spans must be of the same length.");

        int i = 0;
        int vectorSize = Vector<byte>.Count;

        // Define the space character vector
        Vector<byte> spaceVector = new Vector<byte>((byte)' ');
        
        // Optimization: We can handle multiple spaces, tabs, newlines by checking them individually
        // or using a lookup. For simplicity and performance in this specific exercise, 
        // we will focus on the Space character (0x20) as the primary delimiter, 
        // but we will handle the tail safely.
        // Note: To handle multiple specific values (Space, Tab, Newline) simultaneously 
        // with Vector<byte>, we would typically use a lookup table or conditional logic 
        // inside the loop. Here, we stick to the Space example for clarity of the SIMD pattern.

        // Vectorized loop
        int lastVectorIndex = utf8Input.Length - vectorSize;
        for (; i <= lastVectorIndex; i += vectorSize)
        {
            // Load vector from input
            Vector<byte> data = Vector.LoadUnsafe(ref Unsafe.Add(ref MemoryMarshal.GetReference(utf8Input), i));
            
            // Compare: Vector.Equals returns a vector where each byte is 0xFF if equal, 0x00 otherwise.
            Vector<byte> comparison = Vector.Equals(data, spaceVector);

            // Store results. Since Vector<byte> comparison produces 0xFF (true) or 0x00 (false),
            // we can cast this to boolean. 
            // Note: Vector<bool> is not directly supported for bitwise operations in the same way,
            // so we typically extract the result and store it.
            for (int j = 0; j < vectorSize; j++)
            {
                // Unsafe read/write for speed, assuming bounds are valid
                maskOutput[i + j] = comparison[j] != 0;
            }
        }

        // Scalar fallback for the tail
        for (; i < utf8Input.Length; i++)
        {
            // Handle Space, Tab, Newline, Carriage Return
            byte b = utf8Input[i];
            maskOutput[i] = (b == (byte)' ' || b == (byte)'\t' || b == (byte)'\n' || b == (byte)'\r');
        }
    }
}
