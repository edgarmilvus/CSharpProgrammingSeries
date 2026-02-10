
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class IntrinsicsSimulator
{
    public static int[] AddQuantizedIds(int[] idsA, int[] idsB)
    {
        if (idsA.Length != idsB.Length) throw new ArgumentException("Arrays must be same length");
        int[] result = new int[idsA.Length];

        // 1. AVX2 Implementation (256-bit)
        if (Avx2.IsSupported)
        {
            int i = 0;
            int vectorSize = 256 / 8; // 256 bits / 8 bits per byte = 32 bytes
            int intCount = vectorSize / sizeof(int); // 32 bytes / 4 bytes per int = 8 ints

            unsafe
            {
                fixed (int* aPtr = idsA)
                fixed (int* bPtr = idsB)
                fixed (int* rPtr = result)
                {
                    // Process 8 integers at a time using AVX2
                    for (; i <= idsA.Length - intCount; i += intCount)
                    {
                        // Load 256-bit vectors
                        Vector256<int> vecA = Avx.LoadVector256(aPtr + i);
                        Vector256<int> vecB = Avx.LoadVector256(bPtr + i);
                        
                        // Add
                        Vector256<int> sum = Avx2.Add(vecA, vecB);
                        
                        // Store
                        Avx.Store(rPtr + i, sum);
                    }
                }
            }

            // Scalar tail
            for (; i < idsA.Length; i++)
            {
                result[i] = idsA[i] + idsB[i];
            }
        }
        // 2. Generic Vector<T> Fallback (128-bit or 256-bit depending on hardware)
        else if (Vector.IsHardwareAccelerated)
        {
            int i = 0;
            int vectorCount = Vector<int>.Count;

            unsafe
            {
                fixed (int* aPtr = idsA)
                fixed (int* bPtr = idsB)
                fixed (int* rPtr = result)
                {
                    for (; i <= idsA.Length - vectorCount; i += vectorCount)
                    {
                        Vector<int> vecA = Unsafe.Read<Vector<int>>(aPtr + i);
                        Vector<int> vecB = Unsafe.Read<Vector<int>>(bPtr + i);
                        Vector<int> sum = vecA + vecB;
                        Unsafe.Write(rPtr + i, sum);
                    }
                }
            }

            for (; i < idsA.Length; i++)
            {
                result[i] = idsA[i] + idsB[i];
            }
        }
        // 3. Scalar Fallback
        else
        {
            for (int i = 0; i < idsA.Length; i++)
            {
                result[i] = idsA[i] + idsB[i];
            }
        }

        return result;
    }

    /*
     * PERFORMANCE ANALYSIS & TRADE-OFFS
     * 
     * 1. Portability vs. Performance:
     *    - Vector<T>: High portability. The JIT compiles this to the best available 
     *      instruction set (SSE, AVX, AVX2) at runtime. It runs everywhere, but might 
     *      not use the absolute latest instructions (like AVX512) unless specifically targeted.
     *    - Intrinsics (Avx2): Low portability. The code is compiled directly to specific 
     *      machine instructions. It fails to load or throws a runtime exception if the CPU 
     *      doesn't support AVX2. However, it offers precise control over instruction selection 
     *      (e.g., using specific shuffle or permute instructions not available in Vector<T>).
     * 
     * 2. Impact of AVX512:
     *    - AVX512 doubles the register width (512 bits) compared to AVX2 (256 bits).
     *    - For token processing, this means processing 16 floats or 16 ints simultaneously (vs 8).
     *    - This significantly increases throughput, reducing loop iterations by half.
     *    - However, AVX512 often causes CPU frequency throttling (thermal limits) on consumer 
     *      chips, which can negate the throughput gains in sustained workloads. Server CPUs 
     *      handle this better.
     * 
     * 3. Dangers of Intrinsics:
     *    - If you run AVX512 code on a CPU that lacks support, the application will crash 
     *      (typically with a System.NotSupportedException or an illegal instruction exception).
     *    - This requires careful runtime feature detection (IsSupported checks) and separate 
     *      code paths, increasing code complexity and binary size.
     *    - Vector<T> abstracts this away, ensuring the code runs safely on older hardware 
     *      (albeit slower).
     */
}
