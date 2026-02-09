
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

namespace Chapter5_Exercise1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Prompt the user for their age
            Console.Write("How old are you? ");
            
            // 2. Read the input (this is a string)
            string input = Console.ReadLine();
            
            // 3. Convert the string to an integer
            int age = int.Parse(input);
            
            // 4. Perform the arithmetic
            // We multiply step-by-step to keep it clear.
            // 1 Year = 365 Days
            // 1 Day = 24 Hours
            // 1 Hour = 60 Minutes
            // 1 Minute = 60 Seconds
            int days = age * 365;
            int hours = days * 24;
            int minutes = hours * 60;
            int seconds = minutes * 60;
            
            // 5. Output the result using String Interpolation
            Console.WriteLine($"You are roughly {seconds} seconds old!");
        }
    }
}
