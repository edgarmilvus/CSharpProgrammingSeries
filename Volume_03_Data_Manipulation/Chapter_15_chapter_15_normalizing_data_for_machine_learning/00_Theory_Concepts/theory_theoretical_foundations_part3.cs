
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class DataPoint
{
    public double FeatureA { get; set; }
    public double FeatureB { get; set; }
}

public class Normalizer
{
    // Step 1: Immediate Execution to gather global statistics
    public static (double Min, double Max) GetMinMax(IEnumerable<double> data)
    {
        // .Min() and .Max() are immediate execution operators.
        // They iterate the entire source to calculate the result.
        return (data.Min(), data.Max());
    }

    // Step 2: Deferred Execution to apply the transformation
    public static IEnumerable<double> ApplyMinMaxScaling(
        IEnumerable<double> data, 
        double min, 
        double max)
    {
        double range = max - min;
        
        // If range is 0, we cannot scale (avoid division by zero).
        // This check ensures the pipeline is robust.
        if (range == 0) 
            return data; // Return original data unchanged

        // The Select query is defined here but NOT executed yet.
        return data.Select(x => (x - min) / range);
    }
}

public class AiPipeline
{
    public void BuildPipeline()
    {
        // Simulate a large dataset (e.g., raw embeddings)
        var rawData = new List<double> { 10.0, 20.0, 30.0, 40.0, 50.0 };

        // --- PHASE 1: STATISTICAL ANALYSIS (Immediate) ---
        // We must iterate the data once to find the bounds.
        // This is the "Cost" of the pipeline setup.
        var stats = Normalizer.GetMinMax(rawData);
        Console.WriteLine($"Min: {stats.Min}, Max: {stats.Max}");

        // --- PHASE 2: TRANSFORMATION DEFINITION (Deferred) ---
        // We define the scaling logic. 
        // NO computation happens here. The variable 'scaledData' is just a query definition.
        var scaledData = Normalizer.ApplyMinMaxScaling(rawData, stats.Min, stats.Max);

        // At this point, memory usage is negligible. We haven't created new numbers yet.

        // --- PHASE 3: CONSUMPTION (Immediate) ---
        // We need to pass data to the AI model. We force execution here.
        // .ToList() iterates the query, applies the math, and stores results in memory.
        var trainingSet = scaledData.ToList();

        // Now 'trainingSet' contains: [0.0, 0.25, 0.5, 0.75, 1.0]
        foreach (var val in trainingSet)
        {
            Console.WriteLine($"Scaled Value: {val}");
        }
    }
}
