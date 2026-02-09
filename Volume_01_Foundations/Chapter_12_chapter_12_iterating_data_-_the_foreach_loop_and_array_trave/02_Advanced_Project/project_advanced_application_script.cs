
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

// ---------------------------------------------------------
// Advanced Application Script: The "Inventory Manager"
// ---------------------------------------------------------
// Problem: A small store owner needs to track the stock levels
// of 5 different products and calculate the total value of
// their inventory. We will use arrays to store data and
// foreach loops to process it safely.
// ---------------------------------------------------------

class Program
{
    static void Main()
    {
        // -----------------------------------------------------
        // STEP 1: Initialize Data Arrays
        // -----------------------------------------------------
        // We use arrays because we know exactly how many items
        // we have (5). Arrays are fixed-size containers.
        
        // Array to store product names (Strings)
        string[] productNames = new string[5];
        
        // Array to store quantity of each product (Integers)
        int[] quantities = new int[5];
        
        // Array to store price per unit (Doubles)
        double[] prices = new double[5];

        // -----------------------------------------------------
        // STEP 2: Populate Arrays with User Input
        // -----------------------------------------------------
        // We use a 'for' loop here because we need the index 
        // to assign data to specific positions in the arrays.
        // We ask the user for details for each of the 5 products.

        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"\n--- Entering Product #{i + 1} ---");

            // Get Product Name
            Console.Write("Enter Product Name: ");
            productNames[i] = Console.ReadLine();

            // Get Quantity (must convert string input to integer)
            Console.Write("Enter Quantity: ");
            string quantityInput = Console.ReadLine();
            quantities[i] = int.Parse(quantityInput);

            // Get Price (must convert string input to double)
            Console.Write("Enter Price: ");
            string priceInput = Console.ReadLine();
            prices[i] = Convert.ToDouble(priceInput);
        }

        // -----------------------------------------------------
        // STEP 3: Display Inventory using 'foreach'
        // -----------------------------------------------------
        // Here we introduce the 'foreach' loop. Unlike the 'for'
        // loop, we do not manage an index variable. We simply
        // read every item in the collection from start to finish.
        // This is safer because we cannot accidentally access an
        // invalid index (like index 5 in an array of size 5).

        Console.WriteLine("\n\n=================================================");
        Console.WriteLine("CURRENT INVENTORY REPORT");
        Console.WriteLine("=================================================");

        // We want to display Name, Quantity, and Price together.
        // Since 'foreach' gives us one item at a time from a single
        // array, we need a way to track our position to get data
        // from the other arrays. We will use a manual counter variable.
        
        int counter = 0;

        // Iterating over the 'productNames' array
        foreach (string name in productNames)
        {
            // 'name' now holds the value of the current product.
            // Because we are inside the loop, we know exactly which
            // iteration we are on via 'counter'.
            
            int currentQty = quantities[counter];
            double currentPrice = prices[counter];

            // Calculate value of this specific line item
            double lineValue = currentQty * currentPrice;

            // Display the details using String Interpolation
            Console.WriteLine($"Product: {name}");
            Console.WriteLine($"  Stock: {currentQty} units");
            Console.WriteLine($"  Price: ${currentPrice:F2}");
            Console.WriteLine($"  Value: ${lineValue:F2}");
            Console.WriteLine("-------------------------------------------------");

            // IMPORTANT: Increment the counter so the next iteration
            // looks at the next index in the quantities and prices arrays.
            counter++;
        }

        // -----------------------------------------------------
        // STEP 4: Calculate Total Inventory Value
        // -----------------------------------------------------
        // We use a 'for' loop here to perform arithmetic accumulation.
        // We could use 'foreach', but 'for' is slightly easier when
        // we need to access multiple arrays simultaneously by index.

        double totalValue = 0.0;

        for (int i = 0; i < 5; i++)
        {
            // Multiply quantity by price and add to the running total
            totalValue += quantities[i] * prices[i];
        }

        Console.WriteLine("\n=================================================");
        Console.WriteLine($"TOTAL INVENTORY VALUE: ${totalValue:F2}");
        Console.WriteLine("=================================================");

        // -----------------------------------------------------
        // STEP 5: Safety Check - Modifying Data
        // -----------------------------------------------------
        // Scenario: The store owner wants to apply a 10% discount
        // to all items that have a quantity less than 10 (low stock).
        // We will use a 'foreach' loop to check conditions.
        
        Console.WriteLine("\n--- Applying Low Stock Adjustment ---");

        // CRITICAL RULE: You cannot modify the array itself 
        // (e.g., adding or removing items) inside a foreach loop.
        // However, you CAN modify the *contents* of the elements
        // if the array type is mutable (like double or int).
        
        // We will iterate using a 'for' loop here so we can 
        // modify the 'prices' array directly by index.
        
        for (int i = 0; i < 5; i++)
        {
            if (quantities[i] < 10)
            {
                // Apply 10% discount (multiply by 0.9)
                prices[i] = prices[i] * 0.90;
                Console.WriteLine($"Low Stock Alert: {productNames[i]} discounted to ${prices[i]:F2}");
            }
        }

        // -----------------------------------------------------
        // STEP 6: Final Verification with Foreach
        // -----------------------------------------------------
        // Let's display the updated prices using foreach one last time
        // to prove the data was changed.

        Console.WriteLine("\n--- Updated Price List (Read-Only Check) ---");
        
        // Reset counter for the new loop
        counter = 0;
        
        foreach (string name in productNames)
        {
            Console.WriteLine($"{name}: ${prices[counter]:F2}");
            counter++;
        }
    }
}
