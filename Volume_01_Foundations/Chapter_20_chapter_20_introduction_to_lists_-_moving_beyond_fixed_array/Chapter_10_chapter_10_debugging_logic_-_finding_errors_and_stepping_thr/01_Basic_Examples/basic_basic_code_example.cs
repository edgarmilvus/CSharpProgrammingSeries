
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

namespace InventoryTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialize the variable to track items sold.
            int itemsSold = 0;

            // 2. Simulate 5 customers entering the store.
            // We use a 'for' loop (Chapter 9) to count iterations.
            for (int customerNumber = 1; customerNumber <= 5; customerNumber++)
            {
                // 3. Simulate a random purchase amount for this customer.
                // In a real app, this might come from user input or a calculation.
                // Here, we hardcode it to 2 items per customer for simplicity.
                int purchaseAmount = 2;

                // 4. Add the purchase to the total.
                itemsSold = itemsSold + purchaseAmount;

                // 5. Display the current status using String Interpolation (Chapter 3).
                Console.WriteLine($"Customer {customerNumber} bought items. Total sold: {itemsSold}");

                // 6. Check if we have reached the daily limit (Logic from Chapter 6).
                // If we sell more than 8 items, the store closes early.
                if (itemsSold > 8)
                {
                    Console.WriteLine("Daily limit reached! Closing store early.");
                    
                    // 7. The 'break' statement (Chapter 10).
                    // This immediately exits the loop, skipping any remaining customers.
                    break;
                }
            }

            // 8. Final message indicating the program has finished.
            Console.WriteLine("Inventory update complete.");
        }
    }
}
