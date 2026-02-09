
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

class Program
{
    static void Main()
    {
        // 1. Create a string array with size 5
        string[] temperatures = new string[5];

        // 2. Use a for loop to ask for inputs and store them
        Console.WriteLine("Enter 5 daily high temperatures:");
        for (int i = 0; i < temperatures.Length; i++)
        {
            Console.Write($"Day {i + 1}: ");
            temperatures[i] = Console.ReadLine();
        }

        // 3. Use a foreach loop to iterate over the array
        int sum = 0;
        
        // 4. Inside the loop, convert string to int and add to sum
        foreach (string tempStr in temperatures)
        {
            int tempInt = int.Parse(tempStr);
            sum += tempInt;
        }

        // 5. Calculate average (divide by 5.0 to keep decimal precision)
        double average = sum / 5.0;

        // 6. Print the result
        Console.WriteLine($"The average temperature is: {average}");
    }
}
