
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class SideEffectSafety
{
    public static void Run()
    {
        var data = Enumerable.Range(1, 1000).ToArray();

        // --- THE WRONG WAY (Race Condition) ---
        int unsafeSum = 0;
        
        // Using Parallel.ForEach to explicitly expose the concurrency issue
        Parallel.ForEach(data, n => 
        {
            if (n % 2 == 0)
            {
                // RACE CONDITION: Multiple threads read unsafeSum, add value, 
                // and write back simultaneously, causing lost updates.
                unsafeSum += n * n; 
            }
        });
        Console.WriteLine($"Unsafe Sum (Likely Wrong): {unsafeSum}");

        // --- THE FIX (Using Lock) ---
        int safeSum = 0;
        object lockObj = new object();

        Parallel.ForEach(data, n => 
        {
            if (n % 2 == 0)
            {
                // Lock serializes access to the shared variable.
                // This works but kills parallel performance.
                lock (lockObj)
                {
                    safeSum += n * n;
                }
            }
        });
        Console.WriteLine($"Safe Sum (With Lock): {safeSum}");

        // --- THE CORRECT PLINQ WAY (Declarative / Pure) ---
        // No locks, no shared variables, no side effects.
        int correctSum = data
            .AsParallel()
            .Where(n => n % 2 == 0)
            .Select(n => n * n)
            .Sum(); // Aggregation handles merging safely.

        Console.WriteLine($"Correct PLINQ Sum: {correctSum}");
    }
}
