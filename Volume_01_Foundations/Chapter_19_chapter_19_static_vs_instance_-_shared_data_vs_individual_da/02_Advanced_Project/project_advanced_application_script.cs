
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

namespace CoffeeShopInventory
{
    // ---------------------------------------------------------
    // CLASS 1: The Menu Item (Shared Data)
    // ---------------------------------------------------------
    // This class represents a drink available on the menu.
    // We use STATIC fields here because the menu is shared by 
    // every customer. If one customer looks at the menu, 
    // it doesn't change for the next customer.
    public class MenuItem
    {
        // Static fields: Shared across all instances of MenuItem
        public static string ShopName = "The C# Café";
        public static int TotalMenuItems = 0;

        // Instance fields: Unique to this specific menu item
        public string Name;
        public double BasePrice;

        // Constructor: Runs when we create a new menu item
        public MenuItem(string name, double price)
        {
            Name = name;
            BasePrice = price;
            
            // We modify the STATIC variable here.
            // Every time a new MenuItem is created, the count goes up.
            TotalMenuItems++; 
        }

        // Static Method: Used to display the shop header
        public static void DisplayShopHeader()
        {
            // We can only access static fields inside a static method
            Console.WriteLine("========================================");
            Console.WriteLine($"Welcome to {ShopName}");
            Console.WriteLine("========================================");
        }
    }

    // ---------------------------------------------------------
    // CLASS 2: The Customer Order (Instance Data)
    // ---------------------------------------------------------
    // This class represents a specific order being built by a user.
    // Every instance of this class is unique to a specific transaction.
    public class CustomerOrder
    {
        // Instance Fields: Unique to this specific order object
        public string DrinkName;
        public double FinalPrice;
        public string Size;
        public bool HasExtraShot;
        public bool HasWhippedCream;

        // Constructor: Sets up a new empty order
        public CustomerOrder()
        {
            // Default values for a new order
            DrinkName = "None";
            FinalPrice = 0.0;
            Size = "Small";
            HasExtraShot = false;
            HasWhippedCream = false;
        }

        // Instance Method: Adds a drink based on the static menu price
        public void AddDrink(MenuItem menuItem)
        {
            DrinkName = menuItem.Name;
            FinalPrice = menuItem.BasePrice;
            Console.WriteLine($"Added {DrinkName} to your order. Base price: ${FinalPrice}");
        }

        // Instance Method: Modifies the instance data (size)
        public void SetSize(string sizeChoice)
        {
            Size = sizeChoice;
            
            // Logic to modify the instance price based on size
            if (sizeChoice == "Medium")
            {
                FinalPrice += 1.00;
            }
            else if (sizeChoice == "Large")
            {
                FinalPrice += 2.00;
            }
            
            Console.WriteLine($"Size set to {Size}. New price: ${FinalPrice}");
        }

        // Instance Method: Modifies the instance data (extras)
        public void AddExtra(string extraType)
        {
            if (extraType == "Shot")
            {
                HasExtraShot = true;
                FinalPrice += 1.50;
                Console.WriteLine("Added extra shot. +$1.50");
            }
            else if (extraType == "Whipped")
            {
                HasWhippedCream = true;
                FinalPrice += 0.50;
                Console.WriteLine("Added whipped cream. +$0.50");
            }
        }

        // Instance Method: Finalizes this specific order
        public void Checkout()
        {
            Console.WriteLine("\n--- FINAL RECEIPT ---");
            Console.WriteLine($"Item: {DrinkName}");
            Console.WriteLine($"Size: {Size}");
            Console.WriteLine($"Extras: {(HasExtraShot ? "Extra Shot" : "None")} {(HasWhippedCream ? "Whipped Cream" : "")}");
            Console.WriteLine($"Total: ${FinalPrice}");
            Console.WriteLine("---------------------\n");
        }
    }

    // ---------------------------------------------------------
    // MAIN PROGRAM: The Application Logic
    // ---------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            // 1. SETUP SHARED DATA (Static Context)
            // -------------------------------------------------
            // We create the menu items. Notice we don't use 'new' 
            // for static fields, but we use 'new' to create instances 
            // of the MenuItem class.
            MenuItem coffee = new MenuItem("Coffee", 2.50);
            MenuItem latte = new MenuItem("Latte", 3.50);
            MenuItem tea = new MenuItem("Tea", 2.00);

            // Display the shared shop header (Static Method)
            MenuItem.DisplayShopHeader();
            
            // Display the shared menu stats (Static Fields)
            Console.WriteLine($"We currently have {MenuItem.TotalMenuItems} items on the menu.\n");

            // 2. START A NEW ORDER (Instance Context)
            // -------------------------------------------------
            // Create a specific order for a customer.
            // This 'currentOrder' variable holds unique data (Instance).
            CustomerOrder currentOrder = new CustomerOrder();
            
            bool isOrdering = true;

            // 3. INTERACTIVE LOOP
            // -------------------------------------------------
            while (isOrdering)
            {
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Coffee ($2.50)");
                Console.WriteLine("2. Latte ($3.50)");
                Console.WriteLine("3. Tea ($2.00)");
                Console.Write("Select a drink number: ");
                string choice = Console.ReadLine();

                // Selecting a drink based on input
                if (choice == "1")
                {
                    currentOrder.AddDrink(coffee);
                }
                else if (choice == "2")
                {
                    currentOrder.AddDrink(latte);
                }
                else if (choice == "3")
                {
                    currentOrder.AddDrink(tea);
                }
                else
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                    continue; // Skip the rest of the loop
                }

                // 4. CUSTOMIZE THE INSTANCE
                // -------------------------------------------------
                // Ask for size (modifies instance data)
                Console.Write("Choose size (S/M/L): ");
                string sizeInput = Console.ReadLine().ToUpper();
                
                if (sizeInput == "M") currentOrder.SetSize("Medium");
                else if (sizeInput == "L") currentOrder.SetSize("Large");
                else currentOrder.SetSize("Small");

                // Ask for extras (modifies instance data)
                Console.Write("Add extra shot? (y/n): ");
                if (Console.ReadLine().ToLower() == "y")
                {
                    currentOrder.AddExtra("Shot");
                }

                Console.Write("Add whipped cream? (y/n): ");
                if (Console.ReadLine().ToLower() == "y")
                {
                    currentOrder.AddExtra("Whipped");
                }

                // 5. FINALIZE
                // -------------------------------------------------
                currentOrder.Checkout();

                // 6. RESET OR EXIT?
                // -------------------------------------------------
                // We can ask if they want another drink.
                // Note: If they say yes, we could either:
                // A) Create a brand new CustomerOrder (New Instance)
                // B) Continue using the current one (but we'd need to reset fields)
                
                Console.Write("Start a new order? (y/n): ");
                string next = Console.ReadLine().ToLower();
                
                if (next == "y")
                {
                    // We create a NEW instance. 
                    // The old order is "done" and this new one starts fresh.
                    currentOrder = new CustomerOrder();
                    Console.WriteLine("\n--- NEW ORDER STARTED ---\n");
                }
                else
                {
                    isOrdering = false;
                }
            }

            Console.WriteLine("Thank you for visiting the C# Café!");
        }
    }
}
