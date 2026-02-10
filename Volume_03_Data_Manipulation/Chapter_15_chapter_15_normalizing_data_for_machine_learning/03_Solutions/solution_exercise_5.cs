
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

public static class DataShuffler
{
    // Static Random instance to avoid seeding issues with frequent instantiation
    private static readonly Random _rng = new Random();

    public static IEnumerable<IEnumerable<double>> Shuffle(IEnumerable<IEnumerable<double>> data)
    {
        // 1. Materialize the data. Shuffling requires random access, which IEnumerable does not provide.
        var list = data.ToList();
        int n = list.Count;

        if (n == 0) return Enumerable.Empty<IEnumerable<double>>();

        // 2. Generate a list of indices [0, 1, 2, ..., N-1]
        var indices = Enumerable.Range(0, n).ToList();

        // 3. Perform Fisher-Yates shuffle on the indices list.
        // This is an imperative algorithm, but encapsulated within a pure function.
        for (int i = n - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            // Swap indices[i] and indices[j]
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        // 4. Project the original data using the shuffled indices.
        // This creates a new sequence view of the data.
        return indices.Select(idx => list[idx]);
    }
}
