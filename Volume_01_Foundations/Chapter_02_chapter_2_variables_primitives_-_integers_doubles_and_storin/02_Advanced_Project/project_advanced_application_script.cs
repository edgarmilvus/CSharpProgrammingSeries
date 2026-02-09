
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

namespace GroceryCheckout
{
    class Program
    {
        static void Main(string[] args)
        {
            // ==========================================
            // PART 1: DEFINING VARIABLES FOR INVENTORY
            // ==========================================
            // We need to store the names and prices of 5 distinct items.
            // Since we cannot use arrays or lists yet, we must declare
            // individual variables for each piece of data.

            // Item 1: Gala Apples
            // We use 'string' for text data (the name).
            string item1Name = "Gala Apples";
            // We use 'double' for decimal numbers (the price per pound).
            double item1Price = 1.49;
            // We use 'int' for whole numbers (the quantity the user buys).
            int item1Quantity = 0;

            // Item 2: Whole Milk
            string item2Name = "Whole Milk";
            double item2Price = 3.99;
            int item2Quantity = 0;

            // Item 3: Sourdough Bread
            string item3Name = "Sourdough Bread";
            double item3Price = 4.50;
            int item3Quantity = 0;

            // Item 4: Free-Range Eggs
            string item4Name = "Free-Range Eggs";
            double item4Price = 5.25;
            int item4Quantity = 0;

            // Item 5: Organic Butter
            string item5Name = "Organic Butter";
            double item5Price = 6.80;
            int item5Quantity = 0;

            // ==========================================
            // PART 2: SIMULATING USER INPUT
            // ==========================================
            // In a real app, we would use Console.ReadLine() to get input.
            // However, 'ReadLine' is not introduced in Chapter 2.
            // Therefore, we simulate a user selecting items by assigning
            // values directly to our quantity variables.

            // The user decides to buy 3 pounds of Apples.
            item1Quantity = 3;

            // The user decides to buy 2 cartons of Milk.
            item2Quantity = 2;

            // The user decides to buy 1 loaf of Bread.
            item3Quantity = 1;

            // The user decides to buy 0 boxes of Eggs (they changed their mind).
            item4Quantity = 0;

            // The user decides to buy 1 package of Butter.
            item5Quantity = 1;

            // ==========================================
            // PART 3: CALCULATING SUBTOTALS
            // ==========================================
            // We calculate the total cost for each item type by multiplying
            // the price by the quantity. We store these in new variables.

            double subTotal1 = item1Price * item1Quantity;
            double subTotal2 = item2Price * item2Quantity;
            double subTotal3 = item3Price * item3Quantity;
            double subTotal4 = item4Price * item4Quantity;
            double subTotal5 = item5Price * item5Quantity;

            // ==========================================
            // PART 4: CALCULATING TAX AND GRAND TOTAL
            // ==========================================
            // We define a tax rate (8.5%) as a double.
            // We calculate the tax amount for the entire cart.
            // We calculate the final total.

            double taxRate = 0.085;
            double sumOfSubtotals = subTotal1 + subTotal2 + subTotal3 + subTotal4 + subTotal5;
            double taxAmount = sumOfSubtotals * taxRate;
            double grandTotal = sumOfSubtotals + taxAmount;

            // ==========================================
            // PART 5: DISPLAYING THE RECEIPT
            // ==========================================
            // We use Console.WriteLine to print the header.
            // We use Console.Write to print items on the same line if needed.
            // We format the numbers to look like currency.

            Console.WriteLine("=========================================");
            Console.WriteLine("   GROCERY STORE CHECKOUT RECEIPT");
            Console.WriteLine("=========================================");
            Console.WriteLine(); // Empty line for spacing

            // Display Item 1
            Console.Write(item1Name);
            Console.Write(" (x");
            Console.Write(item1Quantity);
            Console.Write("): $");
            Console.WriteLine(subTotal1.ToString("F2")); // Formats to 2 decimal places

            // Display Item 2
            Console.Write(item2Name);
            Console.Write(" (x");
            Console.Write(item2Quantity);
            Console.Write("): $");
            Console.WriteLine(subTotal2.ToString("F2"));

            // Display Item 3
            Console.Write(item3Name);
            Console.Write(" (x");
            Console.Write(item3Quantity);
            Console.Write("): $");
            Console.WriteLine(subTotal3.ToString("F2"));

            // Display Item 4
            Console.Write(item4Name);
            Console.Write(" (x");
            Console.Write(item4Quantity);
            Console.Write("): $");
            Console.WriteLine(subTotal4.ToString("F2"));

            // Display Item 5
            Console.Write(item5Name);
            Console.Write(" (x");
            Console.Write(item5Quantity);
            Console.Write("): $");
            Console.WriteLine(subTotal5.ToString("F2"));

            Console.WriteLine("-----------------------------------------");
            
            // Display Subtotal
            Console.Write("Subtotal:               $");
            Console.WriteLine(sumOfSubtotals.ToString("F2"));

            // Display Tax
            Console.Write("Tax (8.5%):             $");
            Console.WriteLine(taxAmount.ToString("F2"));

            Console.WriteLine("-----------------------------------------");

            // Display Grand Total
            Console.Write("GRAND TOTAL:            $");
            Console.WriteLine(grandTotal.ToString("F2"));

            Console.WriteLine();
            Console.WriteLine("Thank you for shopping with us!");
            Console.WriteLine("=========================================");
        }
    }
}
