
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

// This script calculates the total cost of an order for a small business.
// It handles quantity, unit price, tax calculation, and profit margin analysis.
// We use only basic math operators and variables to solve this real-world problem.

class OrderCalculator
{
    static void Main()
    {
        // --- PART 1: DEFINING THE VARIABLES ---
        // We define the raw data for our specific order.
        // In a real app, these might come from user input, but for now, we hardcode them.
        
        // The number of items the customer is buying.
        int quantity = 15;
        
        // The cost to make one item (our cost).
        double costPerItem = 4.50;
        
        // The price we sell one item for to the customer.
        double sellingPricePerItem = 12.99;
        
        // The sales tax rate (7.5% expressed as a decimal).
        double taxRate = 0.075;
        
        // The threshold for a bulk discount (e.g., buying more than 10 items).
        int bulkThreshold = 10;
        
        // The discount percentage for bulk orders (10% off).
        double discountPercentage = 0.10;

        // --- PART 2: CALCULATING SUBTOTALS ---
        // We calculate the base totals before tax or discounts.
        
        // Calculate the total cost of goods sold (how much we spent to make these items).
        double totalCost = quantity * costPerItem;
        
        // Calculate the gross revenue (price * quantity) before any adjustments.
        double grossRevenue = quantity * sellingPricePerItem;

        // --- PART 3: APPLYING LOGIC WITH MATH (SIMULATED) ---
        // Since we cannot use 'if' statements yet, we will calculate both scenarios
        // and use math to determine the correct final amount.
        
        // Calculate the discount amount if the order qualifies (Quantity * Price * Discount %).
        // Note: We use the increment operator (++) to show we are processing the order.
        quantity++;
        quantity--; // We increment and decrement immediately to keep the original value for display.
        
        double potentialDiscountAmount = quantity * sellingPricePerItem * discountPercentage;
        
        // Calculate the discount amount if the order does NOT qualify (it will be 0).
        // We calculate the difference between the threshold and the quantity.
        // If quantity is 15 and threshold is 10, difference is 5 (positive).
        // If quantity is 5 and threshold is 10, difference is -5 (negative).
        double thresholdDifference = quantity - bulkThreshold;
        
        // We use a trick: (thresholdDifference + |thresholdDifference|) / 2.
        // If difference is positive (5 + 5)/2 = 5. If negative (-5 + 5)/2 = 0.
        // This isolates the positive difference only.
        double effectiveDifference = (thresholdDifference + Math.Abs(thresholdDifference)) / 2;
        
        // If effectiveDifference > 0, we qualify for the discount.
        // We calculate the actual discount by scaling the potential discount based on the excess.
        // This is a complex math simulation of an 'if' statement.
        double actualDiscount = potentialDiscountAmount * (effectiveDifference / (double)quantity);

        // --- PART 4: FINAL INVOICE CALCULATIONS ---
        
        // Apply the discount to the gross revenue to get the subtotal.
        double subTotal = grossRevenue - actualDiscount;
        
        // Calculate the tax amount based on the subtotal (after discount).
        double taxAmount = subTotal * taxRate;
        
        // Calculate the final total the customer pays.
        double finalTotal = subTotal + taxAmount;
        
        // Calculate the profit (Revenue - Cost - Tax we pay - Discount given).
        // Note: The business pays the tax collected to the government, so it's an expense.
        double profit = grossRevenue - totalCost - actualDiscount - taxAmount;

        // --- PART 5: OUTPUTTING THE INVOICE ---
        // We use String Interpolation ($) to format the numbers nicely.
        
        Console.WriteLine("========================================");
        Console.WriteLine("      HANDMADE CRAFTS INVOICE");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        Console.WriteLine($"Items Sold:      {quantity}");
        Console.WriteLine($"Unit Price:      {sellingPricePerItem:C}"); // :C formats as Currency
        Console.WriteLine($"Cost per Item:   {costPerItem:C}");
        Console.WriteLine();
        
        Console.WriteLine("--- CALCULATIONS ---");
        Console.WriteLine($"Gross Revenue:   {grossRevenue:C}");
        Console.WriteLine($"Bulk Discount:   -{actualDiscount:C}");
        Console.WriteLine($"Subtotal:        {subTotal:C}");
        Console.WriteLine($"Tax (7.5%):      +{taxAmount:C}");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"TOTAL DUE:       {finalTotal:C}");
        Console.WriteLine();
        
        Console.WriteLine("--- BUSINESS ANALYSIS ---");
        Console.WriteLine($"Total Production Cost: {totalCost:C}");
        Console.WriteLine($"Net Profit:            {profit:C}");
        
        // Check if the profit is positive using a boolean variable (allowed in Ch 2).
        bool isProfitable = profit > 0;
        Console.WriteLine($"Is this order profitable? {isProfitable}");
        
        Console.WriteLine("========================================");
    }
}
