
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;

namespace Chapter2.Exercise1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Declare and initialize the celsius variable
            double celsius = 25.5;

            // 2. Declare the fahrenheit variable
            double fahrenheit;

            // 3. Perform the calculation and assign the result to fahrenheit
            // Note: We use 9.0 and 5.0 (doubles) instead of 9 and 5 (ints) 
            // to ensure the division results in a double, not an integer.
            fahrenheit = (celsius * 9.0 / 5.0) + 32;

            // 4. Output the result
            // We convert the double values to strings implicitly via concatenation.
            Console.WriteLine(celsius + "C is " + fahrenheit + "F");
        }
    }
}
