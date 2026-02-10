
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;

namespace BudgetValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- SETUP PHASE ---
            // We define the total monthly income as a fixed integer for this simulation.
            int monthlyIncome = 2000;
            
            // We use individual variables to store the sum of expenses.
            // Since we cannot use arrays, we will track 3 specific expense categories manually.
            int rentExpense = 0;
            int groceryExpense = 0;
            int entertainmentExpense = 0;
            
            // A variable to hold the running total of all expenses.
            int totalExpenses = 0;

            Console.WriteLine("Welcome to the Budget Validator.");
            Console.WriteLine($"Your monthly income is: ${monthlyIncome}");
            Console.WriteLine("--------------------------------------");

            // --- RENT VALIDATION (do-while loop) ---
            // We use 'do-while' because we must ask for the rent amount at least once.
            // The loop will repeat if the input is invalid (negative or exceeds 50% of income).
            bool rentValid = false;
            
            do
            {
                Console.Write("Enter your monthly Rent amount ($): ");
                string inputRent = Console.ReadLine();
                
                // Convert string input to integer (Chapter 5)
                // Note: We assume the user enters a number. In a real app, we'd need error handling, 
                // but for Book 1, we rely on Parse.
                rentExpense = int.Parse(inputRent);

                // Validation Logic (Chapter 6 & 7)
                if (rentExpense < 0)
                {
                    Console.WriteLine("Error: Rent cannot be negative. Try again.");
                }
                else if (rentExpense > (monthlyIncome / 2))
                {
                    // Checking if rent exceeds 50% of income
                    Console.WriteLine("Warning: Rent is very high relative to income. Please confirm.");
                    // For this script, we will allow it but warn the user.
                    rentValid = true; 
                }
                else
                {
                    rentValid = true;
                }
            } while (!rentValid); // Loop repeats while rent is NOT valid

            // Update running total
            totalExpenses = totalExpenses + rentExpense;
            Console.WriteLine($"Rent recorded: ${rentExpense}. Running Total: ${totalExpenses}");
            Console.WriteLine();

            // --- GROCERY VALIDATION (while loop) ---
            // We use 'while' here to demonstrate re-entry if the user makes a mistake immediately.
            // This simulates a "check-first, then act" pattern.
            bool groceryValid = false;

            while (!groceryValid)
            {
                Console.Write("Enter your monthly Groceries amount ($): ");
                string inputGrocery = Console.ReadLine();
                groceryExpense = int.Parse(inputGrocery);

                if (groceryExpense < 0)
                {
                    Console.WriteLine("Error: Amount cannot be negative.");
                }
                else if (groceryExpense > 1000)
                {
                    // Arbitrary limit for groceries in this simulation
                    Console.WriteLine("Error: That seems too high for groceries. Max limit $1000.");
                }
                else
                {
                    groceryValid = true;
                }
            }

            // Update running total
            totalExpenses = totalExpenses + groceryExpense;
            Console.WriteLine($"Groceries recorded: ${groceryExpense}. Running Total: ${totalExpenses}");
            
            // Check budget status dynamically
            if (totalExpenses > monthlyIncome)
            {
                Console.WriteLine("ALERT: You have already exceeded your income!");
            }
            Console.WriteLine();

            // --- ENTERTAINMENT VALIDATION (Nested Logic) ---
            // We will ask for entertainment, but if the budget is already tight, we restrict the entry.
            bool entertainmentValid = false;

            do
            {
                Console.Write("Enter your monthly Entertainment amount ($): ");
                string inputEnt = Console.ReadLine();
                entertainmentExpense = int.Parse(inputEnt);

                // Complex logic: Check negative, then check if it fits in remaining budget
                if (entertainmentExpense < 0)
                {
                    Console.WriteLine("Error: Negative amount not allowed.");
                }
                else
                {
                    int remainingBudget = monthlyIncome - totalExpenses;
                    
                    if (entertainmentExpense > remainingBudget)
                    {
                        Console.WriteLine($"Error: You only have ${remainingBudget} left in your budget.");
                        Console.WriteLine("You cannot spend more than what you have.");
                        // We do NOT set entertainmentValid to true, so the loop repeats.
                    }
                    else
                    {
                        entertainmentValid = true;
                    }
                }
            } while (!entertainmentValid);

            // Update running total
            totalExpenses = totalExpenses + entertainmentExpense;
            Console.WriteLine($"Entertainment recorded: ${entertainmentExpense}. Running Total: ${totalExpenses}");
            Console.WriteLine();

            // --- FINAL SUMMARY ---
            // Calculate the difference using arithmetic operators
            int remaining = monthlyIncome - totalExpenses;

            Console.WriteLine("======================================");
            Console.WriteLine("BUDGET REPORT");
            Console.WriteLine("======================================");
            Console.WriteLine($"Income:      ${monthlyIncome}");
            Console.WriteLine($"Total Spent: ${totalExpenses}");
            Console.WriteLine($"Remaining:   ${remaining}");
            
            // Final Logic Check
            if (remaining < 0)
            {
                Console.WriteLine("STATUS: DEFICIT (You are over budget!)");
            }
            else if (remaining == 0)
            {
                Console.WriteLine("STATUS: ZERO BALANCE");
            }
            else
            {
                Console.WriteLine("STATUS: SURPLUS (Good job!)");
            }

            // Keep console open
            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }
    }
}
