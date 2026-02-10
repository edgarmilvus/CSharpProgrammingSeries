
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;

class DataSafety
{
    // Static method to normalize and clamp values
    // Parameters: value, source min/max, target min/max
    static double ClampNormalize(double value, double srcMin, double srcMax, double tgtMin, double tgtMax)
    {
        // 1. Normalize the value using the standard formula
        double normalized = (value - srcMin) / (srcMax - srcMin) * (tgtMax - tgtMin) + tgtMin;

        // 2. Check Lower Bound
        // If the value was below sourceMin, normalized will be < tgtMin
        if (normalized < tgtMin)
        {
            normalized = tgtMin;
        }

        // 3. Check Upper Bound
        // If the value was above sourceMax, normalized will be > tgtMax
        if (normalized > tgtMax)
        {
            normalized = tgtMax;
        }

        return normalized;
    }

    static void Main()
    {
        // Define ranges: Source 0-100, Target 10-20
        double srcMin = 0;
        double srcMax = 100;
        double tgtMin = 10;
        double tgtMax = 20;

        // Test data: Normal, Low (Out of bounds), High (Out of bounds)
        double[] sensorReadings = { 50, -20, 120, 0, 100 };

        Console.WriteLine("Testing Clamping Logic:");
        Console.WriteLine($"Target Range: {tgtMin} to {tgtMax}");
        Console.WriteLine("-----------------------");

        // Loop through test data
        foreach (double reading in sensorReadings)
        {
            // Call the helper method
            double result = ClampNormalize(reading, srcMin, srcMax, tgtMin, tgtMax);
            Console.WriteLine($"Input: {reading} -> Output: {result}");
        }
    }
}
