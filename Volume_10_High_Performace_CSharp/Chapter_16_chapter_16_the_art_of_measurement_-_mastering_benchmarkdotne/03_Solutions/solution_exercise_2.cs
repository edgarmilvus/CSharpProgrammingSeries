
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class NormalizationBenchmark
{
    private float[] _embedding;
    
    [Params(768)]
    public int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _embedding = new float[Dimension];
        for (int i = 0; i < _embedding.Length; i++)
        {
            _embedding[i] = (float)rng.NextDouble();
        }
    }

    [Benchmark]
    public void NormalizeScalar()
    {
        // 1. Calculate L2 Norm (Sum of Squares)
        float sumSq = 0f;
        for (int i = 0; i < _embedding.Length; i++)
        {
            sumSq += _embedding[i] * _embedding[i];
        }
        float norm = MathF.Sqrt(sumSq);

        // 2. Normalize
        if (norm > 0)
        {
            for (int i = 0; i < _embedding.Length; i++)
            {
                _embedding[i] /= norm;
            }
        }
    }

    [Benchmark]
    public void NormalizeSimd()
    {
        // 1. Calculate L2 Norm using Vector<T>
        Vector<float> sumSqVector = Vector<float>.Zero;
        int i = 0;
        int lastBlockIndex = _embedding.Length - Vector<float>.Count;

        // Main SIMD loop for summation
        for (; i <= lastBlockIndex; i += Vector<float>.Count)
        {
            var vector = new Vector<float>(_embedding, i);
            sumSqVector += vector * vector;
        }

        // Horizontal sum (reduce)
        float sumSq = 0f;
        for (int j = 0; j < Vector<float>.Count; j++)
        {
            sumSq += sumSqVector[j];
        }

        // Handle tail for summation (if dimension not multiple of Vector size)
        for (; i < _embedding.Length; i++)
        {
            sumSq += _embedding[i] * _embedding[i];
        }

        float norm = MathF.Sqrt(sumSq);

        // 2. Normalize using SIMD
        if (norm > 0)
        {
            i = 0;
            var normVector = new Vector<float>(norm);
            for (; i <= lastBlockIndex; i += Vector<float>.Count)
            {
                var vector = new Vector<float>(_embedding, i);
                (vector / normVector).CopyTo(_embedding, i);
            }

            // Handle tail for division
            for (; i < _embedding.Length; i++)
            {
                _embedding[i] /= norm;
            }
        }
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<NormalizationBenchmark>();
    }
}
