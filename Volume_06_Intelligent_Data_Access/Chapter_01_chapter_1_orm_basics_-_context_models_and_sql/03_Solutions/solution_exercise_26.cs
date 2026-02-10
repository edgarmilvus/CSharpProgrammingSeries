
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

// Source File: solution_exercise_26.cs
// Description: Solution for Exercise 26
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// 1. Bias Detection Service
public class BiasDetector
{
    // Define bias axes (e.g., Gender, Race)
    // These are "direction vectors" in the embedding space
    private static readonly Dictionary<string, float[]> _biasAxes = new()
    {
        { "Gender", new float[] { 0.1f, -0.2f, 0.5f } } // Mock direction
    };

    public double MeasureBias(float[] vector, string axis)
    {
        if (!_biasAxes.ContainsKey(axis)) return 0;

        var axisVector = _biasAxes[axis];
        // Calculate projection of vector onto the bias axis
        // This measures how much the vector aligns with the bias direction
        return DotProduct(vector, axisVector);
    }

    private double DotProduct(float[] a, float[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
        return sum;
    }
}

// 2. Fairness-Aware Search
public class FairSearchService
{
    private readonly BiasDetector _detector;

    public List<Document> SearchWithFairness(float[] queryVector, List<Document> candidates)
    {
        // 1. Calculate scores
        var scored = candidates.Select(c => new
        {
            Doc = c,
            Relevance = CalculateSimilarity(c.Vector, queryVector),
            Bias = _detector.MeasureBias(c.Vector, "Gender")
        }).ToList();

        // 2. Re-rank to minimize bias
        // If bias score is high, penalize the relevance score
        var fairResults = scored
            .OrderByDescending(x => x.Relevance - Math.Abs(x.Bias) * 0.5) // Penalty factor
            .Select(x => x.Doc)
            .ToList();

        return fairResults;
    }

    private double CalculateSimilarity(float[] a, float[] b) => 0.9; // Mock
}

// 3. Debiasing (Pre-processing)
public class Debiaser
{
    public float[] DebiasVector(float[] vector, float[] biasDirection)
    {
        // Subtract the bias component from the vector
        // v' = v - (v · u) * u
        var projection = DotProduct(vector, biasDirection);
        var result = new float[vector.Length];
        
        for (int i = 0; i < vector.Length; i++)
        {
            result[i] = vector[i] - (float)(projection * biasDirection[i]);
        }
        return result;
    }

    private float DotProduct(float[] a, float[] b) => a.Zip(b, (x, y) => x * y).Sum();
}
