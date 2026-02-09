
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

class BarChartGenerator
{
    static void Main()
    {
        // Define ranges
        double sourceMin = 0;
        double sourceMax = 100;
        
        double targetMin = 0;
        double targetMax = 20; // Max bar length in characters

        int dataPoints = 10;
        Random rng = new Random();

        Console.WriteLine($"Generating Bar Chart (Source: {sourceMin}-{sourceMax}, Target Width: {targetMax})");
        Console.WriteLine("--------------------------------------------------");

        // Outer loop: Iterate through data points
        for (int i = 0; i < dataPoints; i++)
        {
            // 1. Generate random raw data
            double rawValue = rng.NextDouble() * (sourceMax - sourceMin) + sourceMin;

            // 2. Normalize to target range (0 to 20)
            double normalizedValue = (rawValue - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;

            // 3. Convert to integer for loop counting
            // We cast the double to an int. This truncates the decimal (e.g., 12.9 becomes 12).
            int barLength = (int)normalizedValue;

            // 4. Print Label
            Console.Write($"Value {rawValue:F1}: ");

            // 5. Inner Loop: Print '#' characters
            for (int j = 0; j < barLength; j++)
            {
                Console.Write("#");
            }

            // New line for the next data point
            Console.WriteLine();
        }
    }
}
