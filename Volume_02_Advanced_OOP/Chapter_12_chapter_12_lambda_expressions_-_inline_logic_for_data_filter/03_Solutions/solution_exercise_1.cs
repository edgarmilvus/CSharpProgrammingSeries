
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

public class TensorFiltering
{
    public static void Main()
    {
        // Simulating a 3x4 LIDAR sensor matrix
        List<List<float>> lidarData = new List<List<float>>
        {
            new List<float> { 0.2f, 15.5f, 120.0f, 45.0f },
            new List<float> { 0.4f, 50.0f, 0.1f, 99.9f },
            new List<float> { 5.0f, 10.0f, 20.0f, 30.0f }
        };

        // Define thresholds
        float minThreshold = 1.0f;
        float maxThreshold = 100.0f;

        List<float> validReadings = FilterTensor(lidarData, minThreshold, maxThreshold);

        Console.WriteLine("Filtered Sensor Readings:");
        foreach (var reading in validReadings)
        {
            Console.WriteLine(reading);
        }
    }

    public static List<float> FilterTensor(List<List<float>> tensor, float min, float max)
    {
        // Edge case: Null or empty tensor
        if (tensor == null || !tensor.Any())
            return new List<float>();

        // Flatten the 2D structure and apply the lambda filter
        return tensor
            .SelectMany(row => row) // Flattens List<List<T>> to IEnumerable<T>
            .Where(val => val > min && val < max) // Lambda expression for inline logic
            .ToList();
    }
}
