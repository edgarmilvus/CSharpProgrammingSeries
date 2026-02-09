
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

// PROBLEM CONTEXT:
// Imagine you are building a user interface for a music player.
// A volume slider on the screen is 200 pixels wide.
// The internal volume level for the audio system, however, works on a scale of 0.0 to 1.0.
// When a user drags the slider to the middle (100 pixels), we need to calculate
// the corresponding volume level (0.5).
// This is a classic "Range Conversion" problem. We need to convert a value from
// one range (0 to 200) to another (0.0 to 1.0).

public class RangeConverter
{
    public static void Main()
    {
        // --- 1. DEFINING THE RANGES ---
        // We define the input range (the slider's position) and the output range (the volume).
        // Input Range: 0 to 200 pixels
        // Output Range: 0.0 to 1.0 volume
        
        double inputStart = 0;     // Minimum slider position
        double inputEnd = 200;     // Maximum slider position
        
        double outputStart = 0.0;  // Minimum volume
        double outputEnd = 1.0;    // Maximum volume

        // --- 2. THE INPUT VALUE ---
        // Let's say the user has dragged the slider to position 150.
        double currentPosition = 150;

        // --- 3. THE MATH LOGIC (Linear Interpolation) ---
        // To convert ranges, we need to perform three specific calculations:
        
        // Step A: Calculate the size of the input and output ranges.
        double inputRange = inputEnd - inputStart;   // 200 - 0 = 200
        double outputRange = outputEnd - outputStart; // 1.0 - 0.0 = 1.0

        // Step B: Calculate how far the current position is through the input range (as a percentage/decimal).
        // We subtract the start from the current position, then divide by the total range size.
        // (150 - 0) / 200 = 0.75
        double howFarThrough = (currentPosition - inputStart) / inputRange;

        // Step C: Apply that percentage to the output range.
        double result = outputStart + (howFarThrough * outputRange);
        // 0.0 + (0.75 * 1.0) = 0.75

        // --- 4. DISPLAYING THE RESULT ---
        Console.WriteLine($"The slider is at {currentPosition}.");
        Console.WriteLine($"The calculated volume level is: {result}");
        
        // Let's try another value to be sure.
        currentPosition = 50;
        
        // We can reuse the logic, or create a method (see below).
        // Recalculating manually for demonstration:
        howFarThrough = (currentPosition - inputStart) / inputRange;
        result = outputStart + (howFarThrough * outputRange);
        
        Console.WriteLine($"\nIf the slider is at {currentPosition}...");
        Console.WriteLine($"The calculated volume level is: {result}");
    }
}
