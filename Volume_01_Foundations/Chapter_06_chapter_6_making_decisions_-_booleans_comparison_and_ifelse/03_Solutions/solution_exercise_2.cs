
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;

class GradingSystem
{
    static void Main()
    {
        // Ask the user for the student's score
        Console.Write("Enter the student's score (0-100): ");
        
        // Read input and convert to an integer immediately
        int score = int.Parse(Console.ReadLine());

        // Check for Grade A (90 and above)
        if (score >= 90)
        {
            Console.WriteLine("Grade: A");
        }
        // Check for Grade B (80 to 89)
        else if (score >= 80)
        {
            Console.WriteLine("Grade: B");
        }
        // Check for Grade C (70 to 79)
        else if (score >= 70)
        {
            Console.WriteLine("Grade: C");
        }
        // Check for Grade D (60 to 69)
        else if (score >= 60)
        {
            Console.WriteLine("Grade: D");
        }
        // If none of the above, it must be below 60
        else
        {
            Console.WriteLine("Grade: F");
        }
    }
}
