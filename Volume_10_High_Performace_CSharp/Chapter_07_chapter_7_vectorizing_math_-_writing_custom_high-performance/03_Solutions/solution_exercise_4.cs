
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Numerics;

public static class HybridProcessor
{
    public static void ProcessTokenVector(float[] data)
    {
        // Step 1: Vectorized Norm Calculation (Reusing logic from Ex 1)
        float norm = CalculateNormVectorized(data);

        // Step 2: Scalar Tanh Application
        // We cannot easily vectorize MathF.Tanh with standard Vector<T>.
        // We apply it to the data array in-place.
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = MathF.Tanh(data[i]);
        }

        // Step 3: Vectorized Scaling by Norm
        // Now we multiply the entire array (which contains Tanh results) by the scalar norm.
        // This is a vector * scalar operation, which is highly efficient.
        Vector<float> scaleVec = new Vector<float>(norm);
        int count = Vector<float>.Count;
        int i = 0;

        unsafe
        {
            fixed (float* ptr = data)
            {
                for (; i <= data.Length - count; i += count)
                {
                    Vector<float> vec = Unsafe.Read<Vector<float>>(ptr + i);
                    vec *= scaleVec;
                    Unsafe.Write(ptr + i, vec);
                }
            }
        }

        // Scalar tail
        for (; i < data.Length; i++)
        {
            data[i] *= norm;
        }
    }

    private static float CalculateNormVectorized(float[] data)
    {
        double sumSq = 0.0;
        int count = Vector<float>.Count;
        int i = 0;

        unsafe
        {
            fixed (float* ptr = data)
            {
                for (; i <= data.Length - count; i += count)
                {
                    Vector<float> vec = Unsafe.Read<Vector<float>>(ptr + i);
                    sumSq += Vector.Sum(vec * vec);
                }
            }
        }

        for (; i < data.Length; i++)
        {
            sumSq += data[i] * data[i];
        }

        return (float)Math.Sqrt(sumSq);
    }

    public static void RunBenchmarkComparison()
    {
        int size = 1024;
        float[] data = new float[size];
        Random rng = new Random(42);
        for (int i = 0; i < size; i++) data[i] = (float)rng.NextDouble();

        // Warmup
        ProcessTokenVector(data);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int k = 0; k < 10000; k++)
        {
            // Reset data or copy to avoid side effects
            // (Benchmarking logic omitted for brevity, assuming data is reset)
            ProcessTokenVector(data);
        }
        sw.Stop();
        Console.WriteLine($"Hybrid Pipeline Time: {sw.ElapsedMilliseconds} ms");
        
        // Analysis: 
        // The pipeline is dominated by the Tanh calculation (scalar loop).
        // However, the vectorized norm and scaling parts are significantly faster 
        // than their scalar counterparts. The total time is roughly:
        // Time(Tanh) + Time(VectorNorm) + Time(VectorScale).
        // Since Tanh is expensive, the gain from vectorizing the other parts 
        // is visible but doesn't eliminate the bottleneck entirely.
    }
}
