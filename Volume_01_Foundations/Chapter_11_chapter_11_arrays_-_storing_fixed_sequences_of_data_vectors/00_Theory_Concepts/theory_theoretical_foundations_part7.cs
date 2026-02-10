
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

// Source File: theory_theoretical_foundations_part7.cs
// Description: Theoretical Foundations
// ==========================================

using System;

class Program
{
    static void Main()
    {
        // 1. Initialize the array with fixed ratings
        double[] ratings = { 4.5, 5.0, 3.5, 2.0, 4.0 };

        // 2. Use a variable to hold the sum (Chapter 2)
        double sum = 0.0;

        // 3. Iterate using a for loop (Chapter 9)
        // We use ratings.Length to get the exact size
        for (int i = 0; i < ratings.Length; i++)
        {
            // Access the element at the current index
            sum = sum + ratings[i];
            
            // Visualizing the process
            Console.WriteLine($"Processing index {i}, rating: {ratings[i]}");
        }

        // 4. Calculate average (Arithmetic from Chapter 4)
        double average = sum / ratings.Length;

        Console.WriteLine("Average Rating: " + average);
    }
}
