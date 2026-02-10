
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class ExecutionDemo
{
    public static void ShowDifference()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5 };

        // DEFERRED EXECUTION
        // No processing happens here. 'query' is just an object describing what to do.
        // It holds a reference to 'numbers' and a lambda, but hasn't iterated yet.
        var query = numbers.Where(n => n % 2 == 0).Select(n => n * 2);

        // The data changes AFTER the query definition.
        numbers.Add(6); 

        // IMMEDIATE EXECUTION
        // .ToList() forces iteration. It iterates over { 1, 2, 3, 4, 5, 6 }.
        // The result includes the newly added '6'.
        List<int> results = query.ToList(); 

        // Output: 4, 6, 8, 10, 12 (Notice 6*2=12 is included)
        Console.WriteLine(string.Join(", ", results));
    }
}
