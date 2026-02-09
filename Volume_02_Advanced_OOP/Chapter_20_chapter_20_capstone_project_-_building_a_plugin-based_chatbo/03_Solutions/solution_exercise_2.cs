
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

using System;
using System.Collections.Generic;
using System.Linq;

// 1. The Complex Data Structure
public class Tensor
{
    public double[] Values { get; }

    public Tensor(double[] values)
    {
        Values = values;
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", Values.Select(v => v.ToString("F2")))}]";
    }
}

// 2. Subsystem A: Tokenizer
public class Tokenizer
{
    public List<string> Tokenize(string input)
    {
        Console.WriteLine("   [Subsystem] Tokenizing...");
        return input.Split(new[] { ' ', ',', '.', '!' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
    }
}

// 3. Subsystem B: Vector Embedder
public class VectorEmbedder
{
    private const int VectorSize = 5;
    
    public Tensor Embed(List<string> tokens)
    {
        Console.WriteLine("   [Subsystem] Embedding tokens...");
        double[] vector = new double[VectorSize];
        
        foreach (var token in tokens)
        {
            // Mock embedding logic: distribute char codes into vector slots
            for (int i = 0; i < VectorSize; i++)
            {
                vector[i] += (i < token.Length) ? token[i] : 0;
            }
        }
        return new Tensor(vector);
    }
}

// 4. Subsystem C: Normalization Service
public class NormalizationService
{
    public Tensor Normalize(Tensor tensor)
    {
        Console.WriteLine("   [Subsystem] Normalizing vector...");
        double sumSquares = tensor.Values.Sum(v => v * v);
        double magnitude = Math.Sqrt(sumSquares);
        
        if (magnitude == 0) return tensor;

        double[] normalized = new double[tensor.Values.Length];
        for (int i = 0; i < tensor.Values.Length; i++)
        {
            normalized[i] = tensor.Values[i] / magnitude;
        }
        return new Tensor(normalized);
    }
}

// 5. The Facade
public class TensorProcessor
{
    private readonly Tokenizer _tokenizer;
    private readonly VectorEmbedder _embedder;
    private readonly NormalizationService _normalizer;

    public TensorProcessor()
    {
        _tokenizer = new Tokenizer();
        _embedder = new VectorEmbedder();
        _normalizer = new NormalizationService();
    }

    // The simplified interface
    public Tensor Process(string rawText)
    {
        Console.WriteLine($"Processing: '{rawText}'");
        var tokens = _tokenizer.Tokenize(rawText);
        var vector = _embedder.Embed(tokens);
        var normalized = _normalizer.Normalize(vector);
        return normalized;
    }
}

// 6. Usage
public class FacadeDemo
{
    public static void Main()
    {
        var processor = new TensorProcessor();
        Tensor result = processor.Process("Hello, Advanced OOP!");
        
        Console.WriteLine($"\nFinal Normalized Tensor: {result}");
    }
}
