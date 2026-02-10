
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using System;

public class Program
{
    public static void Main()
    {
        // Setup data
        double[] transactions = { 100.5, 250.0, 300.0, 450.0 };
        double bonusLevel = 5.0;

        Console.WriteLine($"Initial Bonus Level: {bonusLevel}");

        // Call the method with mixed parameter types
        ProcessTransactionBatch(transactions, out double total, out double avg, ref bonusLevel);

        Console.WriteLine($"Total Volume: {total}");
        Console.WriteLine($"Average Transaction: {avg}");
        Console.WriteLine($"Updated Bonus Level: {bonusLevel}");
    }

    // Method definition with mixed parameter modifiers
    public static void ProcessTransactionBatch(double[] txs, out double totalVolume, out double averageTransaction, ref double bonusThreshold)
    {
        // Initialize 'out' parameter
        totalVolume = 0.0;

        // Calculate Sum using foreach
        foreach (double amount in txs)
        {
            totalVolume += amount;
        }

        // Calculate Average
        averageTransaction = totalVolume / txs.Length;

        // Modify the 'ref' parameter based on logic
        if (totalVolume > 1000.0)
        {
            bonusThreshold += 10.0;
        }
    }
}
