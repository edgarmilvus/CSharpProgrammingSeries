
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class DistanceCalculator
{
    public static double Manhattan(IEnumerable<double> vecA, IEnumerable<double> vecB)
    {
        // Zip pairs elements, Select calculates the absolute difference, 
        // and Sum aggregates the results.
        return vecA.Zip(vecB, (a, b) => Math.Abs(a - b))
                   .Sum();
    }

    public static void CompareDistances()
    {
        var v1 = new double[] { 1.0, 2.0 };
        var v2 = new double[] { 4.0, 6.0 };

        double euclidean = Math.Sqrt(v1.Zip(v2, (a, b) => Math.Pow(a - b, 2)).Sum());
        double manhattan = v1.Zip(v2, (a, b) => Math.Abs(a - b)).Sum();

        Console.WriteLine($"Euclidean: {euclidean}"); // 5.0
        Console.WriteLine($"Manhattan: {manhattan}"); // 7.0
    }
}
