
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic; // Required for List<T>

class Program
{
    static void Main()
    {
        // 1. Initialize a new List of strings to hold grocery items.
        // Unlike arrays (string[]), we don't need to specify a size here.
        List<string> groceries = new List<string>();

        // 2. Add items to the list using the Add() method.
        groceries.Add("Apples");
        groceries.Add("Milk");
        groceries.Add("Bread");

        // 3. Display the current list of items.
        Console.WriteLine("Current Grocery List:");
        
        // Use a foreach loop (Chapter 12) to iterate through the list.
        foreach (string item in groceries)
        {
            Console.WriteLine("- " + item);
        }

        // 4. Remove an item from the list.
        // We remove "Milk" because we found some at home.
        groceries.Remove("Milk");

        Console.WriteLine("\nUpdated Grocery List (after removing Milk):");
        
        // Iterate again to show the updated list.
        foreach (string item in groceries)
        {
            Console.WriteLine("- " + item);
        }

        // 5. Access a specific item by index (similar to arrays).
        // Note: The index is now 0 because "Milk" was removed.
        string firstItem = groceries[0];
        Console.WriteLine("\nThe first item on the list is: " + firstItem);
    }
}
