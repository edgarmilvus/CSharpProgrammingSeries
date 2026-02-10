
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class TokenProcessor
{
    // Hypothetical previous method signature
    public static unsafe void ScaleTokensUnsafe(float[] input, float[] output, float scale)
    {
        if (input == null || output == null || input.Length != output.Length)
            throw new ArgumentException("Buffers must be non-null and equal length.");

        int length = input.Length;
        int vectorSize = 8; // AVX handles 8 floats per 256-bit vector
        int remaining = length % vectorSize;

        unsafe
        {
            fixed (float* pIn = &input[0])
            fixed (float* pOut = &output[0])
            {
                float* currentIn = pIn;
                float* currentOut = pOut;
                int i = 0;

                // Main SIMD Loop (AVX2)
                if (Avx2.IsSupported)
                {
                    // Broadcast scale into a 256-bit vector (all 8 elements are 'scale')
                    Vector256<float> scaleVec = Vector256.Create(scale);

                    // Loop until we can no longer fill a full vector
                    for (; i <= length - vectorSize; i += vectorSize)
                    {
                        // Load 256 bits (8 floats) from the input address
                        Vector256<float> dataVec = Avx.LoadVector256(currentIn);
                        
                        // Perform multiplication: result = data * scale
                        Vector256<float> resultVec = Avx.Multiply(dataVec, scaleVec);
                        
                        // Store the result to the output address
                        Avx.Store(currentOut, resultVec);

                        // Advance pointers by 8 floats (32 bytes)
                        currentIn += vectorSize;
                        currentOut += vectorSize;
                    }
                }

                // Scalar Tail Loop
                // Handles elements that don't fit into a full SIMD vector
                // or if AVX is not supported on the current hardware
                for (; i < length; i++)
                {
                    *currentOut = *currentIn * scale;
                    currentIn++;
                    currentOut++;
                }
            }
        }
    }
}
