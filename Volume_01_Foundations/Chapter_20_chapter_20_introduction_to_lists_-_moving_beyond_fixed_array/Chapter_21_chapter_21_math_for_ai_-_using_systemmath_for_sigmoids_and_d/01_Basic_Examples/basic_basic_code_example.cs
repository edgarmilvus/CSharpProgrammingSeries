
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;

class GameLogic
{
    static void Main()
    {
        // --- 1. Define the Enemy's Stats ---
        // We use 'double' because we need precise decimal numbers for probability.
        double currentHealth = 25.5;
        double aggressionLevel = 80.0;
        double maxHealth = 100.0;

        // --- 2. Calculate the Input Value (x) ---
        // We want a value that increases as health gets LOW and aggression gets HIGH.
        // We use subtraction (maxHealth - currentHealth) so that low health results in a higher number.
        // We use Division to normalize the values relative to the max health.
        double rawValue = (maxHealth - currentHealth) / maxHealth + (aggressionLevel / 100.0);

        // --- 3. Apply the Sigmoid Function ---
        // Formula: 1 / (1 + e^-x)
        // e is Euler's number (approx 2.71828). We get it using Math.E.
        // We use Math.Pow(base, exponent) to calculate e raised to the power of -rawValue.
        double exponent = -1.0 * rawValue;
        double sigmoidOutput = 1.0 / (1.0 + Math.Pow(Math.E, exponent));

        // --- 4. Make a Decision ---
        // If the sigmoid output is greater than 0.5, the enemy attacks.
        // We format the output to 2 decimal places using F2 in the string interpolation.
        if (sigmoidOutput > 0.5)
        {
            Console.WriteLine($"Enemy Attacks! (Probability: {sigmoidOutput:F2})");
        }
        else
        {
            Console.WriteLine($"Enemy Retreats! (Probability: {sigmoidOutput:F2})");
        }
    }
}
