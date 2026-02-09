
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

public class Program
{
    public static void Main()
    {
        // 1. Define a data source (a list of numbers)
        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        Console.WriteLine("--- Using Func<> to pass a calculation logic ---");
        
        // 2. Use Func<> to pass a predicate (a function that returns a bool)
        // We are passing the logic "IsEven" directly into the filter method.
        FilterAndPrint(numbers, IsEven);

        Console.WriteLine("\n--- Using Lambda Expressions for concise syntax ---");
        
        // 3. Use a Lambda Expression to define the logic inline
        // This avoids defining a separate named method for simple logic.
        FilterAndPrint(numbers, n => n > 5);

        Console.WriteLine("\n--- Using Action<> to pass an output logic ---");
        
        // 4. Use Action<> to pass a void method (a side effect)
        // We are passing the logic "PrintSquare" to execute on each number.
        ProcessAndAct(numbers, PrintSquare);
    }

    // ---------------------------------------------------------
    // METHOD: FilterAndPrint
    // Uses Func<> to accept a function that returns a boolean.
    // ---------------------------------------------------------
    public static void FilterAndPrint(int[] data, Func<int, bool> condition)
    {
        foreach (int num in data)
        {
            // The logic passed in via 'condition' is executed here.
            if (condition(num))
            {
                Console.Write(num + " ");
            }
        }
        Console.WriteLine(); // New line for formatting
    }

    // ---------------------------------------------------------
    // METHOD: IsEven
    // A specific function matching the Func<int, bool> signature.
    // ---------------------------------------------------------
    public static bool IsEven(int number)
    {
        return number % 2 == 0;
    }

    // ---------------------------------------------------------
    // METHOD: ProcessAndAct
    // Uses Action<> to accept a void method (side effect).
    // ---------------------------------------------------------
    public static void ProcessAndAct(int[] data, Action<int> action)
    {
        foreach (int num in data)
        {
            // The logic passed in via 'action' is executed here.
            action(num);
        }
    }

    // ---------------------------------------------------------
    // METHOD: PrintSquare
    // A specific action matching the Action<int> signature.
    // ---------------------------------------------------------
    public static void PrintSquare(int number)
    {
        Console.WriteLine($"{number} squared is {number * number}");
    }
}
