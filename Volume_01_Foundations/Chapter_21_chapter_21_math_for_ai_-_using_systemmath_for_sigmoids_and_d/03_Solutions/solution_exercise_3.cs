
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

public class Program
{
    public static void Main()
    {
        // Test valid vector (3, 4)
        string normalized1 = NormalizeVector(3.0, 4.0);
        Console.WriteLine($"Normalized (3, 4): {normalized1}");

        // Test zero vector (0, 0)
        string normalized2 = NormalizeVector(0.0, 0.0);
        Console.WriteLine($"Normalized (0, 0): {normalized2}");
    }

    public static string NormalizeVector(double x, double y)
    {
        // Calculate magnitude (Euclidean distance from origin)
        double magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

        // Check for zero magnitude to prevent division by zero
        if (magnitude == 0)
        {
            return "(0, 0)";
        }

        // Divide components by magnitude
        double newX = x / magnitude;
        double newY = y / magnitude;

        // Return formatted string with 2 decimal places
        return $"({newX:F2}, {newY:F2})";
    }
}
