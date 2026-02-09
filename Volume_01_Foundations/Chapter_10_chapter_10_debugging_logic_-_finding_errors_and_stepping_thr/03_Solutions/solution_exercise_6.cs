
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

# Source File: solution_exercise_6.cs
# Description: Solution for Exercise 6
# ==========================================

using System;

namespace Exercise5
{
    class Program
    {
        static void Main(string[] args)
        {
            // Simulating a list of numbers using individual variables
            int num1 = 1;
            int num2 = 2;
            int num3 = 3;
            int num4 = 4;
            int num5 = 5;

            Console.WriteLine("Even numbers found:");

            for (int i = 1; i <= 5; i++)
            {
                int currentNumber = 0;

                // Logic to select the correct variable based on the loop index
                if (i == 1) currentNumber = num1;
                else if (i == 2) currentNumber = num2;
                else if (i == 3) currentNumber = num3;
                else if (i == 4) currentNumber = num4;
                else if (i == 5) currentNumber = num5;

                // Check if the number is odd
                if (currentNumber % 2 != 0)
                {
                    // If odd, skip the rest of the loop and start the next iteration
                    continue;
                }

                // If the code reaches here, the number is even
                Console.WriteLine(currentNumber);
            }
        }
    }
}
