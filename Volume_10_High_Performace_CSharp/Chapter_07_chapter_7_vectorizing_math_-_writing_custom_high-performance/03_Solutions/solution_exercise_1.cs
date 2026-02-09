
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
using System.Runtime.InteropServices;

public static class VectorOps
{
    // Baseline scalar implementation
    public static void NormalizeScalar(float[] data)
    {
        if (data == null || data.Length == 0) return;

        double sumSq = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            sumSq += data[i] * data[i];
        }

        // Edge case: handle zero vector to prevent division by zero
        if (sumSq < 1e-10f) return;

        float scale = (float)(1.0 / Math.Sqrt(sumSq));
        for (int i = 0; i < data.Length; i++)
        {
            data[i] *= scale;
        }
    }

    // Vectorized implementation using System.Numerics.Vector<T>
    public static void NormalizeVectorized(float[] data)
    {
        if (data == null || data.Length == 0) return;

        int count = Vector<float>.Count;
        double sumSq = 0.0;
        int i = 0;

        // Main vectorized loop
        // Note: Vector.Dot is preferred if available, but manual multiplication 
        // is used here to demonstrate explicit vector operations.
        unsafe
        {
            fixed (float* ptr = data)
            {
                // Process chunks of 'count' floats
                for (; i <= data.Length - count; i += count)
                {
                    Vector<float> vec = Unsafe.Read<Vector<float>>(ptr + i);
                    Vector<float> squared = vec * vec;
                    
                    // Horizontal sum of the vector elements
                    sumSq += Vector.Sum(squared);
                }
            }
        }

        // Scalar tail processing (for remaining elements)
        for (; i < data.Length; i++)
        {
            sumSq += data[i] * data[i];
        }

        // Edge case: handle zero vector
        if (sumSq < 1e-10f) return;

        float scale = (float)(1.0 / Math.Sqrt(sumSq));
        Vector<float> scaleVec = new Vector<float>(scale);

        // Apply scaling vectorized
        unsafe
        {
            fixed (float* ptr = data)
            {
                for (i = 0; i <= data.Length - count; i += count)
                {
                    Vector<float> vec = Unsafe.Read<Vector<float>>(ptr + i);
                    vec *= scaleVec;
                    Unsafe.Write(ptr + i, vec);
                }
            }
        }

        // Scale the tail
        for (; i < data.Length; i++)
        {
            data[i] *= scale;
        }
    }

    // Helper to align memory (Conceptual implementation)
    // In production, you might use ArrayPool or native allocation for alignment.
    public static float[] CreateAlignedArray(int length)
    {
        // .NET arrays are generally aligned to 16-byte boundaries on 64-bit systems,
        // which is sufficient for Vector<T> (typically 128-bit or 256-bit).
        // For stricter alignment (e.g., 32-byte for AVX), use:
        // return NativeMemory.Alloc<float>(length); // Requires .NET 7+
        return new float[length];
    }

    // Benchmarking helper
    public static void RunBenchmark()
    {
        int size = 1024;
        float[] dataScalar = new float[size];
        float[] dataVector = new float[size];
        Random rng = new Random(42);
        
        // Initialize with random data
        for (int i = 0; i < size; i++)
        {
            float val = (float)rng.NextDouble();
            dataScalar[i] = val;
            dataVector[i] = val;
        }

        // Warmup
        NormalizeScalar(dataScalar);
        NormalizeVectorized(dataVector);

        // Benchmark Scalar
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            Array.Copy(dataVector, dataScalar, size); // Reset data
            NormalizeScalar(dataScalar);
        }
        sw.Stop();
        Console.WriteLine($"Scalar Time: {sw.ElapsedMilliseconds} ms");

        // Benchmark Vectorized
        sw.Restart();
        for (int i = 0; i < 10000; i++)
        {
            Array.Copy(dataScalar, dataVector, size); // Reset data
            NormalizeVectorized(dataVector);
        }
        sw.Stop();
        Console.WriteLine($"Vectorized Time: {sw.ElapsedMilliseconds} ms");
    }
}
