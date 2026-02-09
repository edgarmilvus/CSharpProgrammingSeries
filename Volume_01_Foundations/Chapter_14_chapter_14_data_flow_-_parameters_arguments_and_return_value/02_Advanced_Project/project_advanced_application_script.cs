
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;

namespace ExpenseTracker
{
    class Program
    {
        // The Main method is the entry point of our application.
        static void Main(string[] args)
        {
            Console.WriteLine("--- Daily Expense Tracker ---");
            Console.WriteLine();

            // 1. GET THE BUDGET
            // We call a method to get the user's budget. 
            // We pass NO data into this method, but it RETURNS a double value.
            double dailyBudget = GetDailyBudget();
            Console.WriteLine($"Budget set to: ${dailyBudget}");
            Console.WriteLine();

            // 2. PROCESS EXPENSES
            // We call a method to handle the logic of buying items.
            // We PASS the 'dailyBudget' variable into the method.
            // The method will RETURN the final remaining balance.
            double remainingBalance = ProcessExpenses(dailyBudget);

            // 3. FINAL SUMMARY
            // We call a method to display the final result.
            // We PASS the 'remainingBalance' into this method.
            // This method returns nothing (void), so it just prints to the console.
            ShowFinalSummary(remainingBalance);
        }

        // ---------------------------------------------------------
        // METHOD 1: Getting the Budget
        // ---------------------------------------------------------
        // This method has NO parameters (empty parentheses).
        // It RETURNS a double (the budget amount).
        static double GetDailyBudget()
        {
            Console.Write("Enter your daily budget amount: ");
            string input = Console.ReadLine();
            
            // We use double.Parse to convert the string input to a number.
            // We return this value immediately to the caller (Main).
            return double.Parse(input);
        }

        // ---------------------------------------------------------
        // METHOD 2: Processing Expenses
        // ---------------------------------------------------------
        // This method has ONE parameter: 'double initialBudget'.
        // This creates a local variable inside this method that holds the value passed in.
        // It RETURNS a double (the remaining money).
        static double ProcessExpenses(double initialBudget)
        {
            // 'currentBalance' starts with the value passed in via the parameter.
            double currentBalance = initialBudget;

            // We use a 'do-while' loop to ensure we ask for at least one item.
            do
            {
                Console.Write("Enter item name (or type 'done' to finish): ");
                string itemName = Console.ReadLine();

                // Check if the user wants to stop entering items.
                // We use 'break' to exit the loop immediately.
                if (itemName.ToLower() == "done")
                {
                    break;
                }

                Console.Write($"Enter cost for {itemName}: $");
                string costInput = Console.ReadLine();
                double itemCost = double.Parse(costInput);

                // LOGIC: Check if the purchase is affordable.
                // We use the 'currentBalance' variable to make a decision.
                if (itemCost <= currentBalance)
                {
                    // Subtract the cost from the balance.
                    currentBalance = currentBalance - itemCost; // or currentBalance -= itemCost;
                    Console.WriteLine($"Purchased {itemName}. Remaining: ${currentBalance}");
                }
                else
                {
                    // Logic for insufficient funds.
                    // We call a helper method here to show how methods can call other methods.
                    // We pass the cost and current balance to get a specific message.
                    ShowInsufficientFundsMessage(itemCost, currentBalance);
                }

                Console.WriteLine(); // Empty line for readability.

            } while (true); // Infinite loop until 'break' is called.

            // RETURN the final calculated balance back to Main.
            return currentBalance;
        }

        // ---------------------------------------------------------
        // METHOD 3: Insufficient Funds Message
        // ---------------------------------------------------------
        // This method has TWO parameters: 'double cost' and 'double balance'.
        // It returns nothing (void), acting purely as an output mechanism.
        static void ShowInsufficientFundsMessage(double cost, double balance)
        {
            Console.ForegroundColor = ConsoleColor.Red; // Visual cue
            Console.WriteLine($"ERROR: You cannot afford this item.");
            Console.WriteLine($"Item cost: ${cost} | Current Balance: ${balance}");
            Console.WriteLine($"You are short by ${cost - balance}.");
            Console.ResetColor();
        }

        // ---------------------------------------------------------
        // METHOD 4: Final Summary
        // ---------------------------------------------------------
        // This method has ONE parameter: 'double finalBalance'.
        // It returns void.
        static void ShowFinalSummary(double finalBalance)
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("END OF DAY REPORT");
            Console.WriteLine($"Final Balance: ${finalBalance}");

            // We use a simple 'if' statement to give advice based on the return value.
            if (finalBalance > 0)
            {
                Console.WriteLine("Status: Under Budget. Great job!");
            }
            else if (finalBalance == 0)
            {
                Console.WriteLine("Status: Exactly on Budget.");
            }
            else
            {
                Console.WriteLine("Status: Over Budget. Watch your spending tomorrow!");
            }
            Console.WriteLine("------------------------------------------------");
        }
    }
}
