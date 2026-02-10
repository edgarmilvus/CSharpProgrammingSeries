
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic; // Required for List<T>

public class InventorySystem
{
    public static void Main()
    {
        // Initialize the dynamic list
        List<string> products = new List<string>();
        
        // Start an infinite loop to allow continuous input
        while (true)
        {
            Console.Write("Enter a product name: ");
            string input = Console.ReadLine();
            
            // Add the item to the list using the .Add() method
            products.Add(input);
            
            Console.Write("Add another product? (yes/no): ");
            string response = Console.ReadLine();
            
            // Check if the user wants to stop
            // If they type anything other than "yes", we break the loop
            if (response != "yes")
            {
                break; 
            }
        }
        
        Console.WriteLine("\n--- Current Inventory ---");
        
        // Iterate and print using foreach (Chapter 12)
        // This loops through every string in the 'products' list
        foreach (string product in products)
        {
            Console.WriteLine($"- {product}");
        }
    }
}
