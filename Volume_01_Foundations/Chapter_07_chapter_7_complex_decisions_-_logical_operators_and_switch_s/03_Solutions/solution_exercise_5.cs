
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

namespace Exercise5
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter Number 1: ");
            int num1 = int.Parse(Console.ReadLine());

            Console.Write("Enter Number 2: ");
            int num2 = int.Parse(Console.ReadLine());

            Console.Write("Enter Operator (+, -, *, /): ");
            string op = Console.ReadLine();

            Console.Write("Enter Mode (Standard, Scientific): ");
            string mode = Console.ReadLine();

            double result = 0;

            // We use a switch expression to select the operation type
            result = op switch
            {
                "+" => num1 + num2,
                "-" => num1 - num2,
                
                // Complex case for multiplication
                "*" => (mode == "Scientific") ? (num1 * num2) * 1.5 : num1 * num2,
                
                // Complex case for division with safety check
                "/" => num2 != 0 ? (double)num1 / num2 : 0, 
                
                _ => 0 // Default
            };

            // Check if the operator was valid (not the default case)
            if (op == "+" || op == "-" || op == "*" || op == "/")
            {
                if (op == "/" && num2 == 0)
                {
                    Console.WriteLine("Error: Division by zero is not allowed.");
                }
                else
                {
                    Console.WriteLine($"Result: {result}");
                }
            }
            else
            {
                Console.WriteLine("Invalid Operator.");
            }
        }
    }
}
