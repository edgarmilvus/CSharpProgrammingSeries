
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class TokenizerBenchmark
{
    public static void Run()
    {
        var vocabList = new List<string>();
        var vocabSet = new HashSet<string>();
        int vocabSize = 10000;

        // 1. Initialize
        for (int i = 0; i < vocabSize; i++)
        {
            string token = $"token_{i}";
            vocabList.Add(token);
            vocabSet.Add(token);
        }

        string target = "token_9999"; // The last element

        // 2. Benchmark List (Manual Iteration)
        var sw = Stopwatch.StartNew();
        bool foundInList = false;
        // Manual iteration to emphasize the O(N) scan
        foreach (var token in vocabList)
        {
            if (token == target)
            {
                foundInList = true;
                break; // Found it, but we had to scan almost the whole list
            }
        }
        long listTicks = sw.ElapsedTicks;
        
        // 3. Benchmark HashSet
        sw.Restart();
        bool foundInSet = vocabSet.Contains(target); // O(1) average
        long setTicks = sw.ElapsedTicks;

        Console.WriteLine($"List Lookup (Manual Scan): {listTicks} ticks. Found: {foundInList}");
        Console.WriteLine($"HashSet Lookup: {setTicks} ticks. Found: {foundInSet}");
        
        // 4. Analysis Output
        Console.WriteLine("\nAnalysis:");
        Console.WriteLine("The List lookup requires iterating through elements until the target is found.");
        Console.WriteLine("In the worst case (target at the end), this is O(N).");
        Console.WriteLine("The HashSet computes the hash of the target, maps it to a bucket, and retrieves the item.");
        Console.WriteLine("This is O(1) on average, assuming a good hash function.");
    }
}
