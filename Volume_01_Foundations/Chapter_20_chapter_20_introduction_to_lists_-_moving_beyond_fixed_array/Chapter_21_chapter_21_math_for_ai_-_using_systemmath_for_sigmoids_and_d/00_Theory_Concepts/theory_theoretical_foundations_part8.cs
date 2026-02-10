
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

// Source File: theory_theoretical_foundations_part8.cs
// Description: Theoretical Foundations
// ==========================================

using System;

public class LoanDecision
{
    public static void MakeDecision(double rawScore, double volatility)
    {
        // 1. Normalize the raw score using Sigmoid
        // We use the Sigmoid method defined earlier (conceptually)
        // For this example, let's inline the math:
        double probability = 1.0 / (1.0 + Math.Pow(Math.E, -rawScore));

        // 2. Check volatility using Absolute Value
        // If volatility is high (e.g., 10.0), we penalize the score
        double volatilityPenalty = Math.Abs(volatility) * 0.1;

        // 3. Adjust probability
        double finalScore = probability - volatilityPenalty;

        // 4. Make decision using logical operators (Chapter 7)
        if (finalScore > 0.5)
        {
            Console.WriteLine($"Approved (Score: {finalScore:F2})");
        }
        else
        {
            Console.WriteLine($"Denied (Score: {finalScore:F2})");
        }
    }
}
