
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
using System.Collections.Generic; // Required to use List<T>

namespace InventoryManager
{
    class Program
    {
        // Global list to store inventory items.
        // We use a List<string> because we don't know how many items we will have.
        // Unlike an array (string[]), this list can grow dynamically.
        static List<string> inventory = new List<string>();

        static void Main(string[] args)
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n--- INVENTORY MANAGEMENT SYSTEM ---");
                Console.WriteLine("1. Add Item");
                Console.WriteLine("2. Remove Item");
                Console.WriteLine("3. View Inventory");
                Console.WriteLine("4. Check Stock Level");
                Console.WriteLine("5. Exit");
                Console.Write("Select an option (1-5): ");

                string input = Console.ReadLine();

                // Input validation using if/else
                if (input == "1")
                {
                    AddItem();
                }
                else if (input == "2")
                {
                    RemoveItem();
                }
                else if (input == "3")
                {
                    ViewInventory();
                }
                else if (input == "4")
                {
                    CheckStockLevel();
                }
                else if (input == "5")
                {
                    running = false;
                    Console.WriteLine("Exiting system...");
                }
                else
                {
                    Console.WriteLine("Invalid option. Please try again.");
                }
            }
        }

        // Method to add an item to the list
        static void AddItem()
        {
            Console.Write("Enter item name to add: ");
            string item = Console.ReadLine();

            // Add the item to the end of the list
            inventory.Add(item);
            
            Console.WriteLine($"'{item}' added to inventory.");
            Console.WriteLine($"Current capacity: {inventory.Capacity} (Can hold {inventory.Capacity - inventory.Count} more before resizing)");
        }

        // Method to remove an item
        static void RemoveItem()
        {
            Console.Write("Enter item name to remove: ");
            string item = Console.ReadLine();

            // Check if item exists before trying to remove
            bool found = false;
            
            // We use a for loop here to iterate through the list
            // We cannot use foreach if we want to modify the list while iterating (though Remove is safe, searching requires logic)
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == item)
                {
                    // RemoveAt removes the item at the specific index
                    inventory.RemoveAt(i);
                    Console.WriteLine($"'{item}' removed from inventory.");
                    found = true;
                    break; // Exit loop once found
                }
            }

            if (!found)
            {
                Console.WriteLine($"'{item}' not found in inventory.");
            }
        }

        // Method to view all items
        static void ViewInventory()
        {
            if (inventory.Count == 0)
            {
                Console.WriteLine("Inventory is empty.");
                return;
            }

            Console.WriteLine("\n--- CURRENT INVENTORY ---");
            // We use a foreach loop to iterate over the list
            // This is allowed (Chapter 12 concept applied to List<T>)
            int index = 1;
            foreach (string item in inventory)
            {
                Console.WriteLine($"{index}. {item}");
                index++;
            }
            Console.WriteLine($"Total Items: {inventory.Count}");
        }

        // Method to check specific stock logic
        static void CheckStockLevel()
        {
            if (inventory.Count == 0)
            {
                Console.WriteLine("Inventory is empty. Cannot check levels.");
                return;
            }

            Console.Write("Enter item name to check: ");
            string item = Console.ReadLine();

            int count = 0;
            
            // Count occurrences of the item
            foreach (string storedItem in inventory)
            {
                if (storedItem == item)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                Console.WriteLine($"Stock for '{item}': {count}");
                
                // Simple logic based on count
                if (count < 3)
                {
                    Console.WriteLine("Status: LOW STOCK - Consider reordering.");
                }
                else
                {
                    Console.WriteLine("Status: OK");
                }
            }
            else
            {
                Console.WriteLine($"'{item}' is out of stock.");
            }
        }
    }
}
