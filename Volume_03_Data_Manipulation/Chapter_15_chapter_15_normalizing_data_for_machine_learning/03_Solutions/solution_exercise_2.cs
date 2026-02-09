
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public static class Standardizer
{
    public static IEnumerable<IEnumerable<double>> Standardize(IEnumerable<IEnumerable<double>> data)
    {
        if (data == null || !data.Any()) 
            return Enumerable.Empty<IEnumerable<double>>();

        // Materialize to allow multiple passes for statistics calculation
        var materializedData = data.Select(row => row.ToList()).ToList();
        
        // Guard against empty dataset after materialization
        if (materializedData.Count == 0) return Enumerable.Empty<IEnumerable<double>>();

        int numFeatures = materializedData.First().Count();

        // Use PLINQ to calculate statistics in parallel.
        // We project an index range [0, numFeatures) and process each column independently.
        var stats = Enumerable.Range(0, numFeatures)
            .AsParallel() // Enable parallel processing for independent column calculations
            .Select(colIndex =>
            {
                // Extract column values
                var values = materializedData.Select(row => row[colIndex]).ToList();
                
                // Calculate Mean
                double mean = values.Average();
                
                // Calculate Population Standard Deviation
                double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
                double stdDev = Math.Sqrt(sumOfSquares / values.Count);
                
                return new { Index = colIndex, Mean = mean, StdDev = stdDev };
            })
            .OrderBy(s => s.Index) // Re-order to align with original column indices
            .ToList();

        // Transform the data using the calculated stats
        return materializedData.Select(row => 
            row.Select((val, idx) => 
            {
                var stat = stats[idx];
                // Handle edge case: constant feature (stdDev = 0)
                return stat.StdDev == 0 ? 0.0 : (val - stat.Mean) / stat.StdDev;
            })
        );
    }
}
