
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[MemoryDiagnoser]
public class RmsBenchmark
{
    private float[] _managedBuffer;
    private float* _nativeBuffer;
    private GCHandle _pinnedHandle;

    [Params(10000, 1000000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize managed buffer with random data
        _managedBuffer = new float[N];
        var rnd = new Random(42);
        for (int i = 0; i < N; i++)
        {
            _managedBuffer[i] = (float)rnd.NextDouble();
        }

        // Allocate native memory
        _nativeBuffer = (float*)NativeMemory.Alloc((nuint)N * sizeof(float));
        for (int i = 0; i < N; i++)
        {
            _nativeBuffer[i] = _managedBuffer[i];
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // Free native memory
        NativeMemory.Free(_nativeBuffer);
    }

    [Benchmark(Baseline = true)]
    public float SafeArrayAccess()
    {
        double sumSq = 0;
        for (int i = 0; i < N; i++)
        {
            float val = _managedBuffer[i];
            sumSq += val * val;
        }
        return (float)Math.Sqrt(sumSq / N);
    }

    [Benchmark]
    public unsafe float PinnedFixedBuffer()
    {
        double sumSq = 0;
        fixed (float* ptr = _managedBuffer)
        {
            for (int i = 0; i < N; i++)
            {
                float val = ptr[i];
                sumSq += val * val;
            }
        }
        return (float)Math.Sqrt(sumSq / N);
    }

    [Benchmark]
    public unsafe float NativeHeapAllocation()
    {
        double sumSq = 0;
        for (int i = 0; i < N; i++)
        {
            float val = _nativeBuffer[i];
            sumSq += val * val;
        }
        return (float)Math.Sqrt(sumSq / N);
    }

    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RmsBenchmark>();
    }
}
