
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

namespace BakeryInventorySystem
{
    // CLASS DEFINITION: Item
    // Represents a single product in the bakery.
    // We use a class to group data (fields) and behavior (properties).
    public class Item
    {
        // FIELDS: The raw data storage.
        // These are marked 'private' to hide them from outside code.
        // This enforces encapsulation.
        private string _name;
        private double _price;
        private int _stockQuantity;

        // CONSTRUCTOR: Initializes the object with specific values.
        // This runs when we type 'new Item(...)'
        public Item(string name, double price, int quantity)
        {
            _name = name;
            _price = price;
            _stockQuantity = quantity;
        }

        // PROPERTIES: Controlled access to the fields.
        // 1. NAME PROPERTY (Read-Only for external users)
        // We only provide a 'get' accessor. There is no 'set'.
        // This prevents the name from being changed after creation.
        public string Name
        {
            get { return _name; }
        }

        // 2. PRICE PROPERTY (Read-Only for external users)
        // We calculate the price with tax included.
        // This is a computed property.
        public double Price
        {
            get { return _price * 1.08; } // 8% Sales Tax
        }

        // 3. STOCK PROPERTY (Read/Write with Validation)
        // This property allows reading and setting the stock,
        // but it includes logic to prevent invalid values.
        public int StockQuantity
        {
            get { return _stockQuantity; }
            set
            {
                // 'value' is the keyword representing the number being assigned.
                if (value < 0)
                {
                    Console.WriteLine($"Error: Cannot set stock for {_name} to a negative number.");
                }
                else
                {
                    _stockQuantity = value;
                }
            }
        }

        // METHOD: Displays the item details.
        // Uses String Interpolation ($"{...}") to format output.
        public void DisplayInfo()
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"Item: {Name}");
            Console.WriteLine($"Price (inc. tax): {Price:C}"); // :C formats as currency
            Console.WriteLine($"In Stock: {StockQuantity}");
            Console.WriteLine("---------------------------------");
        }
    }

    class Program
    {
        // MAIN METHOD: The entry point of the application.
        static void Main(string[] args)
        {
            Console.WriteLine("=== Bakery Inventory Management System ===");
            Console.WriteLine("Initializing system...");
            Console.WriteLine();

            // SCENARIO 1: Creating Items and Using Properties
            // We create instances of the Item class using 'new'.
            // We pass initial values to the constructor.
            Item croissant = new Item("Butter Croissant", 2.50, 15);
            Item baguette = new Item("French Baguette", 3.00, 8);
            Item muffin = new Item("Blueberry Muffin", 2.00, 20);

            // Display initial state
            Console.WriteLine("INITIAL INVENTORY STATE:");
            croissant.DisplayInfo();
            baguette.DisplayInfo();
            muffin.DisplayInfo();

            // SCENARIO 2: Attempting to modify Read-Only properties
            // If we try to set croissant.Name = "Plain Croissant", it will fail.
            // The compiler prevents it because there is no 'set' accessor.
            // Uncommenting the line below would cause an error:
            // croissant.Name = "Plain Croissant"; // ERROR: Property or indexer cannot be assigned to -- it is read only

            // SCENARIO 3: Using the 'set' accessor with Validation
            // We try to update the stock.
            Console.WriteLine("\nUPDATING INVENTORY (Valid Data):");
            
            // Valid update
            croissant.StockQuantity = 12; 
            croissant.DisplayInfo(); // Shows stock changed from 15 to 12

            // Invalid update (Negative number)
            Console.WriteLine("\nATTEMPTING INVALID UPDATE:");
            baguette.StockQuantity = -5; // This triggers the error message inside the property
            baguette.DisplayInfo(); // Stock remains 8

            // SCENARIO 4: Calculating Total Value
            // We use the Price property (which includes tax) to calculate total inventory value.
            Console.WriteLine("\nCALCULATING TOTAL INVENTORY VALUE:");
            
            double totalValue = 0.0;

            // We calculate value for each item individually.
            // Note: In a full application, we might use arrays/loops, but for this chapter
            // we demonstrate the concept clearly line-by-line.
            
            // Croissant Value
            double croissantValue = croissant.Price * croissant.StockQuantity;
            Console.WriteLine($"{croissant.Name} Value: {croissantValue:C}");
            totalValue += croissantValue;

            // Baguette Value
            double baguetteValue = baguette.Price * baguette.StockQuantity;
            Console.WriteLine($"{baguette.Name} Value: {baguetteValue:C}");
            totalValue += baguetteValue;

            // Muffin Value
            double muffinValue = muffin.Price * muffin.StockQuantity;
            Console.WriteLine($"{muffin.Name} Value: {muffinValue:C}");
            totalValue += muffinValue;

            Console.WriteLine($"\nTotal Inventory Value: {totalValue:C}");

            // SCENARIO 5: Restocking Logic
            // We simulate a restocking event.
            Console.WriteLine("\nRESTOCKING EVENT:");
            
            // We use a method to handle restocking logic.
            RestockItem(croissant, 50);
            RestockItem(baguettes: baguette, quantityToAdd: 20);

            Console.WriteLine("\nFINAL INVENTORY STATE:");
            croissant.DisplayInfo();
            baguette.DisplayInfo();
        }

        // HELPER METHOD: Demonstrates passing objects and modifying their state.
        // Parameters: 'targetItem' is the object to modify, 'amount' is the quantity to add.
        static void RestockItem(Item targetItem, int amount)
        {
            Console.WriteLine($"Restocking {targetItem.Name} by {amount} units...");
            
            // We access the StockQuantity property.
            // The 'get' reads the current value, we add 'amount', 
            // and the 'set' updates the private field.
            int newStock = targetItem.StockQuantity + amount;
            targetItem.StockQuantity = newStock;
        }
    }
}
