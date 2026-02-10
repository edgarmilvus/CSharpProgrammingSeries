
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

// Source File: theory_theoretical_foundations_part4.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class ZScoreNormalizer
{
    // Using an online algorithm for mean and variance allows us to calculate
    // statistics in a single pass, though we often use LINQ aggregates for clarity.
    public static (double Mean, double StdDev) CalculateStatistics(IEnumerable<double> data)
    {
        // Immediate Execution: Materialize to avoid multiple enumeration
        // if we need to iterate more than once (which we do here for clarity).
        var dataList = data.ToList();

        // 1. Calculate Mean
        double mean = dataList.Average();

        // 2. Calculate Variance
        // PLINQ (.AsParallel()) is used here to distribute the summation 
        // across available CPU cores for large datasets.
        double sumOfSquares = dataList.AsParallel()
                                      .Select(x => (x - mean) * (x - mean))
                                      .Sum();

        double variance = sumOfSquares / dataList.Count;
        double stdDev = Math.Sqrt(variance);

        return (mean, stdDev);
    }

    public static IEnumerable<double> ApplyZScoreScaling(
        IEnumerable<double> data, 
        double mean, 
        double stdDev)
    {
        // Defensive coding: if standard deviation is 0, all values are the same.
        // Division by zero would occur. Return 0 for all (or handle as needed).
        if (stdDev == 0)
            return data.Select(_ => 0.0);

        // Deferred Execution: The calculation logic is defined here.
        return data.Select(x => (x - mean) / stdDev);
    }
}

public class ZScoreExample
{
    public void Run()
    {
        var data = new List<double> { 100, 200, 300, 400, 500 };

        // 1. Immediate Execution: Calculate stats using Parallel processing
        var stats = ZScoreNormalizer.CalculateStatistics(data);
        Console.WriteLine($"Mean: {stats.Mean}, StdDev: {stats.StdDev}");

        // 2. Deferred Execution: Define the normalization pipeline
        var normalizedQuery = ZScoreNormalizer.ApplyZScoreScaling(data, stats.Mean, stats.StdDev);

        // 3. Immediate Execution: Materialize for the AI model
        var normalizedData = normalizedQuery.ToList();
        
        // Output will be centered around 0
        normalizedData.ForEach(v => Console.WriteLine(v.ToString("F4")));
    }
}
