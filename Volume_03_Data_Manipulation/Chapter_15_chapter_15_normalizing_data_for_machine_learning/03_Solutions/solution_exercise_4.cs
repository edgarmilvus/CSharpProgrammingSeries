
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class PipelineRefactor
{
    public static IEnumerable<IEnumerable<double>> ProcessData(IEnumerable<IEnumerable<double>> rawData)
    {
        // Step 1: Cleaning (Deferred Execution)
        // Filter out null rows and NaN values without materializing yet.
        var cleanedData = rawData
            .Where(row => row != null) 
            .Select(row => row.Where(val => !double.IsNaN(val))) 
            .Where(row => row.Any());

        // Step 2: Materialization for Statistics (Immediate Execution)
        // Standardization requires global statistics, breaking pure streaming.
        // We must materialize the cleaned data to iterate over it multiple times.
        var materializedCleanData = cleanedData.ToList();

        if (materializedCleanData.Count == 0) 
            return Enumerable.Empty<IEnumerable<double>>();

        int numFeatures = materializedCleanData.First().Count();

        // Step 3: Calculate Statistics (The "Fit" Phase)
        // Using PLINQ to parallelize column-wise statistics calculation.
        var stats = Enumerable.Range(0, numFeatures)
            .AsParallel()
            .Select(idx => 
            {
                // Extract column values
                var col = materializedCleanData.Select(r => r.ElementAt(idx)).ToList();
                
                // Calculate Mean
                double mean = col.Average();
                
                // Calculate Population Standard Deviation
                double stdDev = Math.Sqrt(col.Sum(v => Math.Pow(v - mean, 2)) / col.Count);
                
                return new { Mean = mean, StdDev = stdDev };
            })
            .OrderBy(s => s.Index) // Ensure order matches column indices
            .ToList();

        // Step 4: Transform (The "Transform" Phase)
        // Project the materialized data using the calculated stats.
        return materializedCleanData.Select(row => 
            row.Select((val, idx) => 
            {
                var s = stats[idx];
                return s.StdDev == 0 ? 0.0 : (val - s.Mean) / s.StdDev;
            })
        );
    }
}
