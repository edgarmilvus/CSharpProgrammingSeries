
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;

class InventoryChecker
{
    static void Main()
    {
        // 1. Initialize the array
        string[] inventory = { "Apple", "Banana", "Cherry", "Date" };

        // 2. Get user input
        Console.Write("Enter product name to check: ");
        string userInput = Console.ReadLine();

        // 3. Flag to track if we found the item
        bool itemFound = false;

        // 4. Iterate through the array
        for (int i = 0; i < inventory.Length; i++)
        {
            // 5. Check for match
            if (inventory[i] == userInput)
            {
                itemFound = true;
                // 6. Optimization: Break immediately
                break; 
            }
        }

        // 7. Check the flag and print result
        if (itemFound)
        {
            Console.WriteLine("Item found!");
        }
        else
        {
            Console.WriteLine("Item not in stock.");
        }
    }
}
