
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

namespace BookstoreInventory
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- PART 1: SETUP VARIABLES ---
            // We need variables to store the stock counts for three books.
            // We initialize them to -1 to ensure the loop logic works correctly 
            // (since stock counts cannot be negative).
            int book1Stock = -1;
            int book2Stock = -1;
            int book3Stock = -1;
            
            // Variable to track if we found a critical error (stock = 0).
            bool criticalAlertTriggered = false;

            Console.WriteLine("--- Bookstore Daily Inventory Check ---");
            Console.WriteLine("Please enter the stock count for each book.");
            Console.WriteLine("----------------------------------------");

            // --- PART 2: DATA ENTRY LOOP ---
            // We use a for loop to iterate 3 times (for 3 books).
            // We use 'i' as the counter (1, 2, 3).
            for (int i = 1; i <= 3; i++)
            {
                // We use a string variable to construct the prompt message dynamically.
                string promptMessage = "Enter stock count for Book " + i + ": ";
                
                Console.Write(promptMessage);
                
                // Read the user's input from the console.
                string input = Console.ReadLine();
                
                // Convert the string input to an integer.
                // We use Convert.ToInt32 because it handles empty strings better than int.Parse for beginners.
                int stockCount = Convert.ToInt32(input);

                // --- PART 3: LOGIC CHECKS ---
                // Check if the stock is exactly 0.
                if (stockCount == 0)
                {
                    Console.WriteLine("CRITICAL ALERT: Book " + i + " is out of stock!");
                    criticalAlertTriggered = true;
                    
                    // We found a critical issue. We stop the data entry immediately.
                    // 'break' exits the nearest loop (the for loop).
                    break;
                }
                // Check if the stock is negative (invalid input).
                else if (stockCount < 0)
                {
                    Console.WriteLine("Error: Stock count cannot be negative. Please restart the check.");
                    
                    // We exit the loop because the data is invalid.
                    break;
                }
                // If stock is valid (greater than 0), save it to the correct variable.
                else
                {
                    // We use a switch expression (allowed in Ch 7) to assign the value 
                    // to the correct variable based on the loop counter 'i'.
                    switch (i)
                    {
                        case 1:
                            book1Stock = stockCount;
                            break; // Exit the switch case
                        case 2:
                            book2Stock = stockCount;
                            break;
                        case 3:
                            book3Stock = stockCount;
                            break;
                    }
                }
            }

            Console.WriteLine("----------------------------------------");

            // --- PART 4: FINAL SUMMARY ---
            // We check if the critical alert was triggered during the loop.
            if (criticalAlertTriggered)
            {
                Console.WriteLine("Inventory check aborted due to critical stock level.");
            }
            else
            {
                // Calculate the total stock using the variables we stored.
                int totalStock = book1Stock + book2Stock + book3Stock;
                
                // Display the summary using string interpolation.
                Console.WriteLine($"Inventory Complete.");
                Console.WriteLine($"Book 1 Stock: {book1Stock}");
                Console.WriteLine($"Book 2 Stock: {book2Stock}");
                Console.WriteLine($"Book 3 Stock: {book3Stock}");
                Console.WriteLine($"Total Stock on Hand: {totalStock}");
            }

            // Keep the console window open until a key is pressed.
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
