
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public static class DataNormalizer
{
    public static IEnumerable<IEnumerable<double>> MinMaxScale(IEnumerable<IEnumerable<double>> data)
    {
        // Defensive check for null or empty input
        if (data == null || !data.Any()) 
            yield break;

        // Materialize the data to allow multiple passes for min/max calculation.
        // This is a necessary trade-off: global statistics require full dataset knowledge.
        var materializedData = data.Select(row => row.ToList()).ToList();

        // Determine the number of features (columns) from the first row
        int numFeatures = materializedData.First().Count();

        // Calculate Min and Max for each column using a functional projection
        // We generate a range of indices and project each to a column's statistics.
        var columnStats = Enumerable.Range(0, numFeatures)
            .Select(colIndex => 
            {
                var columnValues = materializedData.Select(row => row[colIndex]);
                return new 
                { 
                    Min = columnValues.Min(), 
                    Max = columnValues.Max() 
                };
            })
            .ToList(); // Immediate execution to capture stats

        // Apply transformation using deferred execution (yield return)
        foreach (var row in materializedData)
        {
            // Project the row into a new scaled row
            var scaledRow = row.Select((value, index) => 
            {
                var stat = columnStats[index];
                double range = stat.Max - stat.Min;
                
                // Handle edge case: constant feature (range = 0)
                return range == 0 ? 0.0 : (value - stat.Min) / range;
            });
            
            yield return scaledRow;
        }
    }
}
