
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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;

public class TokenizationBenchmark
{
    private const string InputText = "10 20 30 40 50 60 70 80 90 100";
    
    [Benchmark(Baseline = true)]
    public List<int> Naive_Linq_Tokenize()
    {
        return TokenizeNaive(InputText);
    }

    [Benchmark]
    public Span<int> Optimized_Span_Tokenize()
    {
        // We use a stack buffer of size 32. If the input exceeds this, 
        // the method will fallback to heap allocation (simulated here by returning a span over an array).
        // In a real constrained environment, you might throw or use a larger stack buffer.
        Span<int> buffer = stackalloc int[32];
        return TokenizeOptimized(InputText, buffer);
    }

    // Naive implementation to analyze
    public static List<int> TokenizeNaive(string input)
    {
        // Allocations: 
        // 1. Array of strings from Split()
        // 2. New string objects for each token (though some might be interned, Split creates new ones)
        // 3. List<int> wrapper and internal array
        // 4. Integer objects (boxing is avoided here since int is a value type, but the List<int> array is allocated)
        return input.Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(int.Parse)
                    .ToList();
    }

    // Optimized implementation
    public static Span<int> TokenizeOptimized(ReadOnlySpan<char> input, Span<int> outputBuffer)
    {
        int tokenCount = 0;
        int start = 0;
        
        // Iterate over the input characters without allocating substrings
        for (int i = 0; i <= input.Length; i++)
        {
            // Check for whitespace or end of string
            bool isEnd = i == input.Length;
            bool isWhitespace = !isEnd && char.IsWhiteSpace(input[i]);

            if (isWhitespace || isEnd)
            {
                if (i > start)
                {
                    // Slice the token (zero-allocation)
                    ReadOnlySpan<char> tokenSpan = input.Slice(start, i - start);
                    
                    // Parse the integer (zero-allocation for the parsing logic)
                    if (int.TryParse(tokenSpan, out int value))
                    {
                        if (tokenCount < outputBuffer.Length)
                        {
                            outputBuffer[tokenCount++] = value;
                        }
                        else
                        {
                            // Handle buffer overflow: In a real scenario, you might 
                            // resize or throw. Here we stop processing for the demo.
                            break;
                        }
                    }
                }
                start = i + 1;
            }
        }

        return outputBuffer.Slice(0, tokenCount);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<TokenizationBenchmark>();
    }
}
