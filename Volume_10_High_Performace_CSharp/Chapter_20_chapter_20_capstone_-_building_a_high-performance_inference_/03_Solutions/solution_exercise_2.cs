
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class LogitScaler
{
    /// <summary>
    /// Applies temperature scaling to logits using AVX2 intrinsics.
    /// </summary>
    public static void ApplyLogitScaling(Span<float> logits, float temperature)
    {
        if (temperature == 0) throw new DivideByZeroException("Temperature cannot be zero.");
        
        // Vector256<float>.Count is 8 on AVX2 (256-bit / 32-bit float)
        int vectorSize = Vector256<float>.Count;
        int i = 0;
        int length = logits.Length;

        // Load the temperature into a vector (broadcast)
        Vector256<float> tempVector = Vector256.Create(temperature);

        unsafe
        {
            fixed (float* ptr = logits)
            {
                // Process chunks of 8 floats at a time
                if (Avx.IsSupported)
                {
                    // Loop unrolling is often beneficial, but standard vector loop is shown here.
                    while (i <= length - vectorSize)
                    {
                        // Load 8 floats from memory
                        Vector256<float> loaded = Avx.LoadVector256(ptr + i);
                        
                        // Divide: result = loaded / tempVector
                        Vector256<float> result = Avx.Divide(loaded, tempVector);
                        
                        // Store result back to memory
                        Avx.Store(ptr + i, result);
                        
                        i += vectorSize;
                    }
                }
                
                // Handle remaining elements (tail) with scalar operations
                for (; i < length; i++)
                {
                    ptr[i] /= temperature;
                }
            }
        }
    }
}

// Benchmarking setup (Conceptual - requires BenchmarkDotNet NuGet package to run)
/*
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class ScalingBenchmark
{
    private float[] _logits;
    private const float Temp = 0.8f;

    [GlobalSetup]
    public void Setup()
    {
        _logits = new float[50000];
        var rng = new Random(42);
        for (int i = 0; i < _logits.Length; i++) _logits[i] = (float)rng.NextDouble();
    }

    [Benchmark]
    public void ScalarScaling()
    {
        for (int i = 0; i < _logits.Length; i++)
        {
            _logits[i] /= Temp;
        }
    }

    [Benchmark]
    public void SimdScaling()
    {
        LogitScaler.ApplyLogitScaling(_logits.AsSpan(), Temp);
    }
}

// To run: BenchmarkRunner.Run<ScalingBenchmark>();
*/
