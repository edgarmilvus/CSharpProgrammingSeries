
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class ExecutionDemo
{
    public static void Main()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

        // DEFERRED EXECUTION: This is the blueprint.
        // No iteration happens here. The query variable just holds the plan.
        // The compiler does not generate any loops or computations.
        var query = numbers
            .Where(n => n % 2 == 0) // Filter for even numbers
            .Select(n => n * n);    // Square the result

        Console.WriteLine("Query defined. No execution yet.");

        // IMMEDIATE EXECUTION: Pressing the "Start" button.
        // .ToList() forces the iteration over the source collection,
        // applies the Where filter, applies the Select transformation,
        // and creates a new List<int> in memory.
        List<int> results = query.ToList();

        Console.WriteLine("Results computed:");
        foreach (var result in results)
        {
            Console.WriteLine(result); // Outputs: 4, 16, 36
        }
    }
}
