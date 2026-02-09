
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

public class DeferredExecutionDemo
{
    public static IEnumerable<int> GenerateData()
    {
        Console.WriteLine(">> Source Data Generated");
        return Enumerable.Range(1, 100);
    }

    public static void Run()
    {
        // 1. Define the pipeline (Deferred)
        // AsParallel() sets up the query structure but does not iterate the source yet.
        var pipeline = GenerateData()
            .AsParallel()
            .Where(n => n > 50)
            .Select(n => n * n);

        Console.WriteLine("Pipeline defined. No data processed yet.");

        // 2. Execute (Immediate)
        // The .ToList() call forces the iteration of the source, 
        // triggering GenerateData() and the subsequent processing.
        Console.WriteLine("Executing pipeline...");
        var result = pipeline.ToList(); 

        Console.WriteLine($"Result count: {result.Count}");
    }
}
