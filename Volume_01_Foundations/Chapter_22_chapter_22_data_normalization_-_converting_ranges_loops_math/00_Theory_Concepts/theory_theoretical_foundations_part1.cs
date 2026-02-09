
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;

public class RangeConverter
{
    public static void Main()
    {
        // Simulating raw sensor data (0 to 1023) using an Array (Chapter 11)
        int[] sensorReadings = new int[] { 0, 256, 512, 768, 1023 };

        // Define our ranges
        // Input range (Raw Sensor)
        int minInput = 0;
        int maxInput = 1023;

        // Output range (Percentage)
        double minOutput = 0.0;
        double maxOutput = 100.0;

        Console.WriteLine("Converting Sensor Data to Percentage:");
        Console.WriteLine("-------------------------------------");

        // Iterate through the array using a foreach loop (Chapter 12)
        foreach (int rawValue in sensorReadings)
        {
            // MATH CALCULATION (Chapter 4 & 21)
            
            // Step 1: Calculate the size of the input and output ranges
            // We cast to double to ensure we don't lose precision during division
            double inputRangeSize = (double)maxInput - (double)minInput;
            double outputRangeSize = maxOutput - minOutput;

            // Step 2: Normalize the input value (0.0 to 1.0)
            // We cast rawValue to double to perform floating-point division
            double normalizedValue = ((double)rawValue - (double)minInput) / inputRangeSize;

            // Step 3: Scale and Shift to the output range
            double finalValue = (normalizedValue * outputRangeSize) + minOutput;

            // Output the result (Chapter 3: String Interpolation)
            Console.WriteLine($"Raw: {rawValue} -> Mapped: {finalValue:F2}%");
        }
    }
}
