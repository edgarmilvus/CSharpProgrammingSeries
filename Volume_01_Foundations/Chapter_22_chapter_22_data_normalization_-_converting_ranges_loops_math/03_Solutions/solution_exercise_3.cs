
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic; // Required for List<T>

class DynamicScaler
{
    static void Main()
    {
        // 1. Get Source Range from User
        Console.Write("Enter the minimum value of your data: ");
        double sourceMin = double.Parse(Console.ReadLine());

        Console.Write("Enter the maximum value of your data: ");
        double sourceMax = double.Parse(Console.ReadLine());

        // Edge Case: Prevent division by zero
        if (sourceMin == sourceMax)
        {
            Console.WriteLine("Error: Minimum and Maximum values cannot be the same.");
            return; // Exit the program safely
        }

        // 2. Get Count of Data Points
        Console.Write("How many data points do you have? ");
        int count = int.Parse(Console.ReadLine());

        // 3. Initialize List to hold normalized values
        // We use List<double> because we don't know the size at compile time.
        List<double> normalizedValues = new List<double>();

        // Initialize Random generator (Standard C# library)
        Random rng = new Random();

        Console.WriteLine($"Generating and normalizing {count} values...");

        // 4. Loop to process data
        for (int i = 0; i < count; i++)
        {
            // Simulate a raw data point within the source range
            // rng.NextDouble() returns a value between 0.0 and 1.0
            double rawData = rng.NextDouble() * (sourceMax - sourceMin) + sourceMin;

            // Normalize to 0.0 - 1.0 range
            double normalized = (rawData - sourceMin) / (sourceMax - sourceMin);

            // Add to our list dynamically
            normalizedValues.Add(normalized);
        }

        // 5. Print Results
        Console.WriteLine("\n--- Normalized Results (0.0 to 1.0) ---");
        
        // Iterate through the list to print
        for (int i = 0; i < normalizedValues.Count; i++)
        {
            // Format to 4 decimal places using :F4
            Console.WriteLine($"Point {i + 1}: {normalizedValues[i]:F4}");
        }
    }
}
