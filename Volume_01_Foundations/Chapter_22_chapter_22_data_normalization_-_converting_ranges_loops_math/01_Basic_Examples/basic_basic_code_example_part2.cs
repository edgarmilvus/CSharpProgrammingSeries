
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

// Source File: basic_basic_code_example_part2.cs
// Description: Basic Code Example
// ==========================================

using System;

public class SmartConverter
{
    // We define a method that accepts the value and all range boundaries as parameters.
    // It returns a double containing the converted value.
    public static double ConvertRange(double value, double inStart, double inEnd, double outStart, double outEnd)
    {
        // Calculate the size of the ranges
        double inRangeSize = inEnd - inStart;
        double outRangeSize = outEnd - outStart;

        // Calculate the percentage of the input range the value covers
        double percentage = (value - inStart) / inRangeSize;

        // Apply that percentage to the output range
        double finalValue = outStart + (percentage * outRangeSize);

        return finalValue;
    }

    public static void Main()
    {
        // Scenario: Converting a temperature from Celsius (0-100) to Farenheit (32-212)
        double celsiusValue = 50.0;
        
        double fahrenheit = ConvertRange(celsiusValue, 0, 100, 32, 212);
        
        Console.WriteLine($"{celsiusValue}°C is equal to {fahrenheit}°F");
    }
}
