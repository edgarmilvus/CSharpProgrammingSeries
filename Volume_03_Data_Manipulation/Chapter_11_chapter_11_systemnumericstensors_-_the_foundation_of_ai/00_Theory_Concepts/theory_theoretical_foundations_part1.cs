
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
        IEnumerable<int> numbers = new[] { 1, 2, 3, 4, 5 };

        // STATION 1: The Assembly Line (Deferred)
        // No code runs here. We are just defining the logic.
        var query = numbers
            .Where(n => n % 2 == 0) // Filter even numbers
            .Select(n => n * 10);   // Multiply by 10

        // STATION 2: Triggering the Assembly (Immediate)
        // The query executes now. The result is stored in a list.
        List<int> result = query.ToList(); 
        // Output: [20, 40, 60, 80, 100]
        
        // If we iterate over 'query' again, it re-runs the logic.
        foreach(var item in query) 
        {
            // This works, but if the source was a database query, 
            // it would hit the database again.
        }
    }
}
