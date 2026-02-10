
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Numerics;

public readonly struct TokenSequence
{
    public ReadOnlyMemory<float> Embeddings { get; }
    public ReadOnlyMemory<int> Mask { get; }
    public int SequenceLength { get; }
    public int EmbeddingDimension { get; }

    public TokenSequence(ReadOnlyMemory<float> embeddings, ReadOnlyMemory<int> mask, int seqLen, int dim)
    {
        Embeddings = embeddings;
        Mask = mask;
        SequenceLength = seqLen;
        EmbeddingDimension = dim;
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class MaskingBenchmark
{
    private TokenSequence _sequence;
    
    [Params(128, 512, 1024)]
    public int SequenceLength { get; set; }

    [Params(768)]
    public int EmbeddingDimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        float[] embeddings = new float[SequenceLength * EmbeddingDimension];
        int[] mask = new int[SequenceLength];

        for (int i = 0; i < embeddings.Length; i++) embeddings[i] = (float)rng.NextDouble();
        for (int i = 0; i < mask.Length; i++) mask[i] = rng.Next(0, 2); // 0 or 1

        _sequence = new TokenSequence(embeddings, mask, SequenceLength, EmbeddingDimension);
    }

    [Benchmark(Baseline = true)]
    public TokenSequence ApplyMaskNaive()
    {
        float[] result = new float[_sequence.SequenceLength * _sequence.EmbeddingDimension];
        var embeddingsSpan = _sequence.Embeddings.Span;
        var maskSpan = _sequence.Mask.Span;
        int dim = _sequence.EmbeddingDimension;

        for (int i = 0; i < _sequence.SequenceLength; i++)
        {
            int maskVal = maskSpan[i];
            int rowOffset = i * dim;
            for (int j = 0; j < dim; j++)
            {
                result[rowOffset + j] = embeddingsSpan[rowOffset + j] * maskVal;
            }
        }
        return new TokenSequence(result, _sequence.Mask, _sequence.SequenceLength, dim);
    }

    [Benchmark]
    public TokenSequence ApplyMaskOptimized()
    {
        float[] result = new float[_sequence.SequenceLength * _sequence.EmbeddingDimension];
        var embeddingsSpan = _sequence.Embeddings.Span;
        var maskSpan = _sequence.Mask.Span;
        int dim = _sequence.EmbeddingDimension;
        int vecCount = Vector<float>.Count;

        for (int i = 0; i < _sequence.SequenceLength; i++)
        {
            int maskVal = maskSpan[i];
            int rowOffset = i * dim;
            
            // If mask is 0, we can skip SIMD and just zero out (or rely on multiplication)
            // Multiplication by 0 is fast, but skipping might be faster if branch prediction is good.
            // Here we stick to the math: 0 * val = 0, 1 * val = val.
            
            var maskVector = new Vector<float>(maskVal);

            int j = 0;
            // SIMD block
            for (; j <= dim - vecCount; j += vecCount)
            {
                var data = new Vector<float>(embeddingsSpan.Slice(rowOffset + j, vecCount));
                var masked = data * maskVector;
                masked.CopyTo(result, rowOffset + j);
            }

            // Tail handling
            for (; j < dim; j++)
            {
                result[rowOffset + j] = embeddingsSpan[rowOffset + j] * maskVal;
            }
        }
        return new TokenSequence(result, _sequence.Mask, _sequence.SequenceLength, dim);
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<MaskingBenchmark>();
    }
}
