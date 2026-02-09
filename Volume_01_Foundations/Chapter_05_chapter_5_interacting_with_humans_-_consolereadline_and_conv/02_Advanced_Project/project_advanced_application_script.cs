
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

namespace Chapter5_AdvancedApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- SETUP PHASE ---
            // We define variables to hold the data for our three items.
            // We initialize them to 0. We use 'double' because inventory might be in decimals (e.g., 1.5 kg).
            
            double beansStock = 0.0;
            double milkStock = 0.0;
            double sugarStock = 0.0;
            
            // Variables to hold the total calculation
            double totalInventory = 0.0;
            
            // Strings to hold the raw input from the user
            string inputBeans = "";
            string inputMilk = "";
            string inputSugar = "";

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("      COFFEE SHOP INVENTORY MANAGEMENT SYSTEM     ");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Please enter the current stock levels for the day.");
            Console.WriteLine("");

            // --- INPUT PHASE: ITEM 1 (COFFEE BEANS) ---
            Console.Write("Enter stock for [Coffee Beans] (kg): ");
            inputBeans = Console.ReadLine();

            // Convert the string input to a number. 
            // We use double.Parse because we expect decimal numbers.
            beansStock = double.Parse(inputBeans);

            Console.WriteLine($"Recorded: {beansStock} kg of Coffee Beans.");
            Console.WriteLine(""); // Empty line for readability

            // --- INPUT PHASE: ITEM 2 (MILK) ---
            Console.Write("Enter stock for [Milk] (liters): ");
            inputMilk = Console.ReadLine();

            // Convert the string input to a number.
            milkStock = double.Parse(inputMilk);

            Console.WriteLine($"Recorded: {milkStock} liters of Milk.");
            Console.WriteLine("");

            // --- INPUT PHASE: ITEM 3 (SUGAR) ---
            Console.Write("Enter stock for [Sugar] (kg): ");
            inputSugar = Console.ReadLine();

            // Convert the string input to a number.
            sugarStock = double.Parse(inputSugar);

            Console.WriteLine($"Recorded: {sugarStock} kg of Sugar.");
            Console.WriteLine("");
            Console.WriteLine("Data entry complete. Calculating totals...");
            Console.WriteLine("");

            // --- CALCULATION PHASE ---
            // We perform arithmetic to find the total weight of all items combined.
            // Note: We are mixing units (kg and liters) for the sake of the example.
            totalInventory = beansStock + milkStock + sugarStock;

            // --- OUTPUT PHASE ---
            Console.WriteLine("==================================================");
            Console.WriteLine("             DAILY INVENTORY REPORT              ");
            Console.WriteLine("==================================================");
            
            // Displaying the individual items using String Interpolation
            Console.WriteLine($"Coffee Beans: {beansStock} kg");
            Console.WriteLine($"Milk:         {milkStock} liters");
            Console.WriteLine($"Sugar:        {sugarStock} kg");
            
            Console.WriteLine("--------------------------------------------------");
            
            // Displaying the calculated total
            Console.WriteLine($"TOTAL STOCK:  {totalInventory} units");
            
            Console.WriteLine("==================================================");
            Console.WriteLine("Thank you for using the system!");
        }
    }
}
