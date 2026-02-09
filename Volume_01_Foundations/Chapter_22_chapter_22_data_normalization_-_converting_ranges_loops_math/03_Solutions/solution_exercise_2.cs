
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

class ScoreNormalizer
{
    static void Main()
    {
        // 1. Define the source data (Array of integers)
        int[] rawScores = { 150, 180, 95, 200, 45, 172 };

        // 2. Define the ranges
        double inputMin = 0;
        double inputMax = 200;
        double outputMin = 0;
        double outputMax = 100;

        Console.WriteLine("Raw Score -> Percentage");
        Console.WriteLine("-----------------------");

        // 3. Iterate through the array using foreach
        foreach (int score in rawScores)
        {
            // 4. Apply the normalization formula
            // Note: We rely on implicit casting here. 
            // (score - inputMin) is an int, but dividing by (inputMax - inputMin) 
            // promotes the result to a double automatically.
            double percentage = ((score - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin;

            // 5. Print the result
            Console.WriteLine($"{score} -> {percentage}%");
        }
    }
}
