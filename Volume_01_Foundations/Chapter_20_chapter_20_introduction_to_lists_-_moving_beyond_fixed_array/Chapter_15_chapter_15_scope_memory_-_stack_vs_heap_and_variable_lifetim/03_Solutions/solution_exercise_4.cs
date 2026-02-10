
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
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
*/

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;

class Program
{
    static void Main()
    {
        int[] original = { 5, 10, 15 };

        // Capture the new array returned by the method
        int[] doubled = DoubleArray(original);

        // Print original
        Console.Write("Original: ");
        for (int i = 0; i < original.Length; i++)
        {
            Console.Write(original[i] + " ");
        }
        Console.WriteLine();

        // Print doubled
        Console.Write("Doubled:  ");
        for (int i = 0; i < doubled.Length; i++)
        {
            Console.Write(doubled[i] + " ");
        }
        Console.WriteLine();
    }

    // Rewrite DoubleArray here
    static int[] DoubleArray(int[] inputArray)
    {
        // 1. Create a new array on the Heap
        int[] result = new int[inputArray.Length];

        // 2. Loop through the input
        for (int i = 0; i < inputArray.Length; i++)
        {
            // 3. Calculate doubled value and assign to the NEW array
            result[i] = inputArray[i] * 2;
        }

        // 4. Return the reference to the new array
        return result;
    }
}
