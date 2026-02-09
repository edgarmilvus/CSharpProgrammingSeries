
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;

public class Program
{
    public static void Main()
    {
        double[] scores = { 10.0, 20.0, 30.0, 40.0, 50.0 };
        
        double[] normalized = NormalizeScores(scores);

        Console.Write("Original: ");
        foreach (double s in scores) Console.Write(s + " ");
        Console.WriteLine();

        Console.Write("Normalized: ");
        foreach (double n in normalized) Console.Write(n + " ");
        Console.WriteLine();
    }

    public static double[] NormalizeScores(double[] data)
    {
        if (data.Length == 0) return new double[0];

        // Find Min and Max
        double min = data[0];
        double max = data[0];

        foreach (double val in data)
        {
            if (val < min) min = val;
            if (val > max) max = val;
        }

        // Create result array
        double[] result = new double[data.Length];

        // Handle edge case: all values are the same
        if (max == min)
        {
            return result; // Returns array of 0.0s
        }

        // Apply Min-Max formula
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (data[i] - min) / (max - min);
        }

        return result;
    }
}
