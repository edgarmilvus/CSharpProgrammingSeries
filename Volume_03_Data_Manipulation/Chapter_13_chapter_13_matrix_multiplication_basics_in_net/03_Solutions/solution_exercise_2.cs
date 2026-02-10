
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class ScalingExercise
{
    public static void Run()
    {
        // 1. Define Data
        var data = new List<(double Feature, double Target)>
        {
            (1000, 200000), (1500, 300000), (2000, 400000), 
            (1200, 250000), (1800, 350000)
        };

        // 2. Calculate Statistics (Immediate Execution)
        // These calls execute immediately to compute scalar values.
        double fMin = data.Min(d => d.Feature);
        double fMax = data.Max(d => d.Feature);
        double tMean = data.Average(d => d.Target);
        
        // Calculate Standard Deviation manually (LINQ doesn't have a built-in StdDev)
        // Variance = Average of squared differences from the Mean
        double tVariance = data.Average(d => Math.Pow(d.Target - tMean, 2));
        double tStdDev = Math.Sqrt(tVariance);

        // 3. Transform Data (Pure Functional)
        // We project into a new anonymous type. The original 'data' list remains untouched.
        var normalizedData = data.Select(d => 
        {
            // 4. Edge Case Handling
            // Prevent division by zero if all feature values are identical.
            double normFeature = (fMax == fMin) 
                ? 0.0 
                : (d.Feature - fMin) / (fMax - fMin);

            // Prevent division by zero if target variance is zero.
            double stdTarget = (tStdDev == 0) 
                ? 0.0 
                : (d.Target - tMean) / tStdDev;

            return new { Raw = d, NormFeature = normFeature, StdTarget = stdTarget };
        });

        foreach (var item in normalizedData)
        {
            Console.WriteLine($"Raw: ({item.Raw.Feature}, {item.Raw.Target}) -> " +
                              $"NormF: {item.NormFeature:F4}, StdT: {item.StdTarget:F4}");
        }
    }
}
