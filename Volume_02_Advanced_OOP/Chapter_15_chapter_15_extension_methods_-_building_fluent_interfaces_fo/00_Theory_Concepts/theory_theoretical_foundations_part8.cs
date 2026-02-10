
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

// Source File: theory_theoretical_foundations_part8.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public static class AIChainExtensions
{
    // Extension method for filtering (lazy)
    public static IEnumerable<T> Filter<T>(
        this IEnumerable<T> source, 
        Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
                yield return item;
        }
    }

    // Extension method for transformation (lazy)
    public static IEnumerable<TResult> Transform<T, TResult>(
        this IEnumerable<T> source, 
        Func<T, TResult> transformer)
    {
        foreach (var item in source)
        {
            yield return transformer(item);
        }
    }
}

// Usage in an AI data processing chain
class Program
{
    static void Main()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 };

        // Build a lazy chain
        var chain = data
            .Filter(x => x > 2)          // Filter: 3, 4, 5
            .Transform(x => x * x);      // Square: 9, 16, 25

        // Execution happens here when iterating
        foreach (var item in chain)
        {
            Console.WriteLine(item); // Output: 9, 16, 25
        }

        // No execution occurred until the foreach loop
    }
}
