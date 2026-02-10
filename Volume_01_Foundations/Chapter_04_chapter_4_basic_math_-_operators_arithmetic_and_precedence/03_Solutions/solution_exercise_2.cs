
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

class FuelEngineer
{
    static void Main()
    {
        // 1. Variables
        int distance = 150;
        double fuelEfficiency = 12.5; 

        // 2. Calculate Total Fuel (Floating point division)
        // We must ensure at least one operand is a double to avoid integer truncation.
        double totalFuelNeeded = distance / fuelEfficiency;

        // 3. Fuel Price
        double fuelPrice = 2.35;

        // 4. Total Cost
        double totalCost = totalFuelNeeded * fuelPrice;

        Console.WriteLine($"Fuel Required: {totalFuelNeeded}");
        Console.WriteLine($"Total Cost: {totalCost}");

        // 5. Precedence Challenge (Parentheses first, then multiplication)
        double complexCalculation = (distance + 50) * 2.5;
        Console.WriteLine($"Complex Calculation: {complexCalculation}");
    }
}
