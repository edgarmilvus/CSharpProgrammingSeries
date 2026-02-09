
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PerformanceComparison
{
    public static void CompareLookups()
    {
        int size = 100000;
        var list = new List<int>();
        var hashSet = new HashSet<int>();

        // Populate
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
            hashSet.Add(i);
        }

        // Target
        int target = size - 1;

        // Measure List
        var sw = Stopwatch.StartNew();
        bool listContains = list.Contains(target);
        sw.Stop();
        Console.WriteLine($"List.Contains: {sw.ElapsedTicks} ticks (O(n))");

        // Measure HashSet
        sw.Restart();
        bool hashContains = hashSet.Contains(target);
        sw.Stop();
        Console.WriteLine($"HashSet.Contains: {sw.ElapsedTicks} ticks (O(1))");
    }
}
