
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

class PhysicsMath
{
    static void Main()
    {
        // 1. Circle Calculations
        double radius = 5.0;
        // Math.Pow(base, exponent)
        double area = Math.PI * Math.Pow(radius, 2);

        // 2. Triangle Calculations
        double sideA = 3.0;
        double sideB = 4.0;
        
        // Pythagorean Theorem: a^2 + b^2 = c^2
        // c = sqrt(a^2 + b^2)
        double sumOfSquares = Math.Pow(sideA, 2) + Math.Pow(sideB, 2);
        double hypotenuse = Math.Sqrt(sumOfSquares);

        // 3. Output with formatting
        Console.WriteLine($"Circle Area: {area:F2}");
        Console.WriteLine($"Triangle Hypotenuse: {hypotenuse:F2}");
    }
}
