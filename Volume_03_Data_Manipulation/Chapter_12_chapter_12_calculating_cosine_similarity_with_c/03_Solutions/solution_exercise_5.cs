
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

public class ParallelPreprocessing
{
    public static void Main()
    {
        // 1. Generate dummy data
        var rawData = Enumerable.Range(1, 100000)
            .Select(i => $"Sample Data {i} - Random Text Here!")
            .ToList();

        Console.WriteLine("Starting Parallel Processing...");

        // 2. Parallel Pipeline
        var processedData = rawData
            .AsParallel() // Enable parallelization
            .WithDegreeOfParallelism(Environment.ProcessorCount) // Optional: Tune concurrency
            .Where(s => s.Length > 20) // Filter
            .Select(s => 
            {
                // Simulate CPU-intensive work (e.g., complex regex or normalization)
                Thread.Sleep(1); 
                return s.ToLowerInvariant();
            })
            .AsOrdered() // Maintain original sequence order if needed
            .ToList(); // Immediate execution (blocks until all threads complete)

        Console.WriteLine($"Processed {processedData.Count} items.");
    }
}
