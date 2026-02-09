
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

class TemperatureLogger
{
    static void Main()
    {
        // 1. Declare the array with a fixed size of 7
        int[] temperatures = new int[7];

        // 2. Use a for loop to get input for each day
        // We use i < 7 because arrays are 0-indexed (indices 0 to 6)
        for (int i = 0; i < 7; i++)
        {
            Console.Write($"Enter temperature for Day {i + 1}: ");
            string input = Console.ReadLine();
            temperatures[i] = int.Parse(input);
        }

        // 3. Calculate the sum
        int sum = 0;
        for (int i = 0; i < 7; i++)
        {
            sum = sum + temperatures[i]; // Accumulate the values
        }

        // 4. Calculate the average
        // We divide by 7.0 to force floating-point division
        double average = sum / 7.0;

        // 5. Output the result
        Console.WriteLine($"The average temperature for the week is: {average:F1}°C");
    }
}
