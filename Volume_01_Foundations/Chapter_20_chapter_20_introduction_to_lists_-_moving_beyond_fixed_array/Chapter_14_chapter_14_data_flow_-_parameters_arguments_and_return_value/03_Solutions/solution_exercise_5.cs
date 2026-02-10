
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;

public class Program
{
    public static void Main()
    {
        int[] scores = { 45, 88, 92, 76, 60 };
        int foundScore; // Variable to hold the out value (does not need initialization)

        // Call method. 'out' parameter does not need to be initialized before passing.
        bool success = FindHighScore(scores, out foundScore);

        if (success)
        {
            Console.WriteLine($"The highest score is: {foundScore}");
        }
    }

    // Method definition: returns bool, has one 'out' parameter
    public static bool FindHighScore(int[] scores, out int highest)
    {
        // The 'out' parameter must be assigned a value inside the method
        highest = scores[0];

        // Loop starting from index 1 (since index 0 is already set as highest)
        for (int i = 1; i < scores.Length; i++)
        {
            if (scores[i] > highest)
            {
                highest = scores[i];
            }
        }

        return true;
    }
}
