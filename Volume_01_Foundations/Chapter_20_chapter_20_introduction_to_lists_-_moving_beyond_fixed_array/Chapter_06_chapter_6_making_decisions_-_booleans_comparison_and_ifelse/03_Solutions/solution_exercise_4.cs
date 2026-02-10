
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

class TemperatureConverter
{
    static void Main()
    {
        // Ask for input
        Console.Write("Enter temperature in Celsius: ");
        
        // Convert input to integer
        int celsius = int.Parse(Console.ReadLine());

        // Calculate Fahrenheit using integer arithmetic.
        // We multiply by 9 first to avoid losing precision from integer division.
        int fahrenheit = (celsius * 9) / 5 + 32;

        // Output the calculated value
        Console.WriteLine($"Temperature in Fahrenheit: {fahrenheit}°F");

        // Determine the weather description
        if (fahrenheit > 100)
        {
            Console.WriteLine("It's hot outside!");
        }
        else if (fahrenheit >= 32)
        {
            // If we reach this line, we know fahrenheit is NOT > 100.
            // Therefore, this block handles values between 32 and 100 inclusive.
            Console.WriteLine("The weather is mild.");
        }
        else
        {
            // If we reach this line, fahrenheit is NOT > 100 and NOT >= 32.
            // Therefore, it must be < 32.
            Console.WriteLine("It's freezing!");
        }
    }
}
