
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

public class TensorNormalization
{
    public static void Main()
    {
        List<List<float>> rawData = new List<List<float>>
        {
            new List<float> { 10, 20, 30 },
            new List<float> { 40, 50, 60 }
        };

        // Assume we know the global min and max for the dataset
        (float min, float max) range = (10, 60);

        var normalized = NormalizeTensor(rawData, range);

        Console.WriteLine("Normalized Data:");
        foreach (var val in normalized)
        {
            Console.WriteLine($"{val:F4}");
        }
    }

    public static List<float> NormalizeTensor(List<List<float>> tensor, (float min, float max) range)
    {
        if (tensor == null || !tensor.Any()) return new List<float>();

        float rangeSpan = range.max - range.min;

        // If range is zero, we cannot normalize (avoid divide by zero)
        if (rangeSpan == 0)
        {
            // Return a list of 0s or 1s, or throw exception depending on policy
            return tensor.SelectMany(r => r.Select(_ => 0.0f)).ToList();
        }

        return tensor
            .SelectMany(row => row)
            .Select(val => (val - range.min) / rangeSpan) // Inline projection
            .ToList();
    }
}
