
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System;

class Program
{
    // Passing a value type (int)
    static void ModifyNumber(int number)
    {
        // This changes the COPY inside the method.
        number = 100;
        Console.WriteLine($"Inside method: {number}");
    }

    // Passing a reference type (array)
    static void ModifyArray(int[] numbers)
    {
        // This changes the ACTUAL array in memory because we have the reference.
        numbers[0] = 999;
        Console.WriteLine($"Inside method: {numbers[0]}");
    }

    static void Main()
    {
        int myNumber = 5;
        ModifyNumber(myNumber);
        // The original 'myNumber' is still 5 because int is a value type.
        Console.WriteLine($"Outside method: {myNumber}");

        int[] myArray = { 1, 2, 3 };
        ModifyArray(myArray);
        // The original 'myArray' is changed because arrays are reference types.
        Console.WriteLine($"Outside method: {myArray[0]}");
    }
}
