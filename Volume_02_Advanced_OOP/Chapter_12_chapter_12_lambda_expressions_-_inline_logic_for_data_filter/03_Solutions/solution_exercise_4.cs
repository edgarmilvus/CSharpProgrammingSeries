
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

public class AdvancedTensorRefactor
{
    public static void Main()
    {
        // 3D Tensor: 2 Frames (Depth), 2x2 Pixels (Height x Width)
        var imageTensor = new List<List<List<float>>>
        {
            new List<List<float>> 
            { 
                new List<float> { 10.5f, 20.1f }, 
                new List<float> { 5.0f, 95.0f } 
            },
            new List<List<float>> 
            { 
                new List<float> { 12.0f, 25.0f }, 
                new List<float> { 8.0f, 100.0f } 
            }
        };

        float threshold = 15.0f;

        var histogram = ProcessImageTensor(imageTensor, threshold);

        Console.WriteLine("Intensity Histogram (Bucket -> Count):");
        foreach (var kvp in histogram)
        {
            Console.WriteLine($"Intensity ~{kvp.Key}: {kvp.Value}");
        }
    }

    public static Dictionary<int, int> ProcessImageTensor(List<List<List<float>>> tensor, float threshold)
    {
        if (tensor == null || !tensor.Any()) return new Dictionary<int, int>();

        // 1. Flatten 3D to 1D (Depth -> Height -> Width)
        // 2. Filter by threshold
        // 3. Group by intensity bucket (rounded integer)
        return tensor
            .SelectMany(depth => depth)             // Flatten Level 1 (Height)
            .SelectMany(height => height)           // Flatten Level 2 (Width -> Pixel Value)
            .Where(pixelValue => pixelValue > threshold) // Filter logic
            .GroupBy(pixelValue => (int)Math.Round(pixelValue)) // Bucket logic
            .ToDictionary(group => group.Key, group => group.Count());
    }
}
