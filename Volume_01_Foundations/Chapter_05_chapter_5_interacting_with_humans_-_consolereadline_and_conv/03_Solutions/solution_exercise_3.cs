
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

namespace Chapter5_Exercise3
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Get numerator
            Console.Write("Enter the numerator: ");
            // We can read and parse directly in one line
            int num = int.Parse(Console.ReadLine());

            // 2. Get denominator
            Console.Write("Enter the denominator: ");
            int denom = int.Parse(Console.ReadLine());

            // 3. Perform division
            // DANGER: If denom is 0, the program will crash here.
            // We do not have 'if' statements yet to check if (denom != 0).
            int result = num / denom;

            // 4. Output
            Console.WriteLine($"Result: {result}");
            
            /*
             * INSTRUCTOR NOTE:
             * If you run this and enter 0 as the denominator, the program will crash.
             * This is expected! You have not learned 'if/else' or 'TryParse' yet.
             * This exercise demonstrates the *need* for the concepts coming in the next sections.
             */
        }
    }
}
