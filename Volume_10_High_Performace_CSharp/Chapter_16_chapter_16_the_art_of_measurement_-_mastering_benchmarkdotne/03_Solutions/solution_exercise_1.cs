
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
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;
using System;
using System.Linq;
using System.Collections.Generic;

[MemoryDiagnoser(displayGenColumns: true)]
[Config(typeof(Config))]
public class FixedTokenizerBenchmark
{
    private string _text;
    private char[] _separators;

    // Configuration to ensure consistent environment
    private class Config : ManualConfig
    {
        public Config()
        {
            // Use a specific runtime and disable warmup to focus on the setup issues
            AddJob(Job.Default
                .WithRuntime(CoreRuntime.Core80)
                .WithWarmupCount(1) // Minimal warmup to see the impact of setup
                .WithIterationCount(10));
            
            // Add columns for detailed analysis
            AddColumn(StatisticColumn.P95);
            AddColumn(StatisticColumn.P99);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        // Generate a realistic string. 
        // Note: We keep the allocation here, but we ensure it happens ONCE per benchmark run.
        _text = string.Join(" ", Enumerable.Range(0, 1000).Select(i => $"Token{i}"));
        _separators = new[] { ' ' };
    }

    [Benchmark(Baseline = true)]
    public string[] StringSplit()
    {
        // This allocates a new string array and new string objects for every token.
        return _text.Split(_separators, StringSplitOptions.None);
    }

    [Benchmark]
    public int SpanTokenization()
    {
        // This method counts tokens without allocating strings.
        ReadOnlySpan<char> span = _text.AsSpan();
        int count = 0;
        int start = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ' ')
            {
                if (i > start) count++;
                start = i + 1;
            }
        }
        if (start < span.Length) count++;
        return count;
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<FixedTokenizerBenchmark>();
    }
}
