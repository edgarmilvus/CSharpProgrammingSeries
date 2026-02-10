
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Linq;

public class ParallelProcessingExercise
{
    public static void Run()
    {
        // 1. Generate Large Data
        int size = 1_000_000;
        var rand = new Random();
        
        // Generate array sequentially first
        var rawData = Enumerable.Range(0, size)
                                .Select(_ => rand.NextDouble() * 1000)
                                .ToArray();

        // 2. Calculate Statistics (Sequential)
        // Min/Max reduction is fast enough sequentially for this size.
        // Parallelizing reduction has overhead; usually reserved for very expensive computations.
        double min = rawData.Min();
        double max = rawData.Max();

        // 3. Parallel Processing
        var normalizedData = rawData
            .AsParallel() // Enable parallel processing
            .Select(x => 
            {
                // Pure function: inputs are x, min, max. No external state modification.
                return (max == min) ? 0.0 : (x - min) / (max - min);
            })
            .ToArray(); // Immediate execution triggers the parallel pipeline

        Console.WriteLine($"Processed {normalizedData.Length} elements.");
        Console.WriteLine($"First 5 normalized: {string.Join(", ", normalizedData.Take(5).Select(x => x.ToString("F4")))}");
        
        // 4. Order Preservation
        // In this specific exercise, order does not strictly matter for the math itself.
        // However, if we needed the output array to correspond exactly to the input indices,
        // we would chain .AsOrdered() after .AsParallel().
    }
}
