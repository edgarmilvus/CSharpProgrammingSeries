
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;

namespace Chapter5_Exercise4
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Prompt
            Console.Write("Enter a single character: ");
            
            // 2. Read input
            string input = Console.ReadLine();
            
            // 3. Convert to char
            // Convert.ToChar takes the first character of the string
            char character = Convert.ToChar(input);
            
            // 4. Perform arithmetic
            // 'char' is essentially a number (ASCII/Unicode).
            // We assign it to an int to see the numeric value clearly.
            int asciiValue = character; 
            
            // Add 1 to get the next character in the ASCII table
            int nextValue = asciiValue + 1;
            
            // 5. Output
            Console.WriteLine($"The character '{character}' has value {asciiValue}.");
            Console.WriteLine($"If we add 1, we get {nextValue}.");
        }
    }
}
