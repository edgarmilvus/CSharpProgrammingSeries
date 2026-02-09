
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

// This script simulates a coffee shop order processor.
// It calculates the final price and determines order status.
class CoffeeShopOrder
{
    static void Main()
    {
        // --- PART 1: GATHERING ORDER DATA ---
        // We use individual variables to store the order details.
        // In a real app, this would come from user input, but we'll simulate it here for clarity.

        // Customer details
        string customerName = "Alex";
        bool isLoyaltyMember = true; // Is the customer part of the loyalty program?
        int loyaltyPoints = 150;     // Current points balance

        // Drink details
        string drinkType = "Latte";  // Options: "Espresso", "Latte", "Cappuccino"
        string size = "Large";       // Options: "Small", "Medium", "Large"
        bool wantsMilk = true;       // Does the customer want milk?
        bool wantsSugar = false;     // Does the customer want sugar?
        bool wantsWhippedCream = true; // Does the customer want whipped cream?

        // --- PART 2: CALCULATING BASE PRICE USING SWITCH ---
        // We use a switch expression to determine the base price based on the drink type.
        // This is cleaner than multiple 'if-else if' statements for a single variable.
        // Note: We are using the modern switch expression syntax allowed in C# 12.
        double basePrice = drinkType switch
        {
            "Espresso" => 2.50,
            "Latte" => 3.50,
            "Cappuccino" => 3.25,
            _ => 4.00 // Default price for any unknown drink type
        };

        // --- PART 3: APPLYING SIZE MULTIPLIER ---
        // We use logical operators to check the size and adjust the price.
        // The '&&' operator ensures both conditions must be true.
        // The '||' operator checks if the size is either Medium or Large.
        double sizeMultiplier = 1.0;

        if (size == "Medium" || size == "Large")
        {
            // If the size is Medium or Large, we increase the multiplier.
            if (size == "Medium")
            {
                sizeMultiplier = 1.25; // 25% increase
            }
            else if (size == "Large")
            {
                sizeMultiplier = 1.50; // 50% increase
            }
        }
        // If size is "Small", the multiplier stays 1.0 (no change).

        // Apply the multiplier to the base price
        double priceAfterSize = basePrice * sizeMultiplier;

        // --- PART 4: CALCULATING EXTRAS COST ---
        // We use logical operators to check multiple conditions for extras.
        // The '&&' operator is used to ensure the customer actually wants the extra
        // AND the specific drink type allows it (e.g., whipped cream is only for Lattes).
        double extrasCost = 0.0;

        // Check for Milk (0.50 charge)
        if (wantsMilk == true)
        {
            extrasCost += 0.50;
        }

        // Check for Sugar (free, but we track it)
        if (wantsSugar == true)
        {
            // No cost, but we log it in the console later.
        }

        // Check for Whipped Cream (0.75 charge) - ONLY if it's a Latte
        // This uses a compound logical condition (&&).
        if (wantsWhippedCream == true && drinkType == "Latte")
        {
            extrasCost += 0.75;
        }
        else if (wantsWhippedCream == true && drinkType != "Latte")
        {
            // If they want whipped cream on a non-Latte, we don't charge, but we warn them.
            Console.WriteLine($"Note: Whipped cream is not standard for {drinkType}, but we added it.");
        }

        // --- PART 5: CALCULATING FINAL PRICE & LOYALTY DISCOUNT ---
        // We combine the price and apply a discount if the customer is a loyalty member.
        double subtotal = priceAfterSize + extrasCost;
        double finalPrice = subtotal;
        double discountAmount = 0.0;

        // Logical Operator: '&&'
        // Condition: Customer must be a member AND have enough points (e.g., > 100).
        if (isLoyaltyMember == true && loyaltyPoints > 100)
        {
            // Apply a 10% discount
            discountAmount = subtotal * 0.10;
            finalPrice = subtotal - discountAmount;
        }
        // Logical Operator: '||'
        // Condition: If they are NOT a member OR have low points, no discount applies.
        // (This is implicit in the flow, but we can check it for demonstration).
        else if (isLoyaltyMember == false || loyaltyPoints <= 100)
        {
            discountAmount = 0.0;
        }

        // --- PART 6: DETERMINING ORDER STATUS (COMPLEX LOGIC) ---
        // We use a standard if-else if-else chain with logical operators to determine
        // how quickly the order needs to be prepared.
        string orderStatus;
        int estimatedTimeMinutes;

        // Complex Condition: Uses '&&' and '||'
        // If the drink is an Espresso (fast to make) AND doesn't have many extras...
        // OR if the customer is a VIP (loyalty member with high points)...
        // ...the order is marked as "Priority".
        if ((drinkType == "Espresso" && wantsWhippedCream == false) || (isLoyaltyMember == true && loyaltyPoints > 200))
        {
            orderStatus = "Priority";
            estimatedTimeMinutes = 2;
        }
        // Else if the size is Large AND they want extras (milk or sugar), it takes longer.
        else if (size == "Large" && (wantsMilk == true || wantsSugar == true))
        {
            orderStatus = "Standard (Complex)";
            estimatedTimeMinutes = 8;
        }
        // Else if the drink is a Cappuccino (requires steaming milk), it takes moderate time.
        else if (drinkType == "Cappuccino")
        {
            orderStatus = "Standard";
            estimatedTimeMinutes = 5;
        }
        // Default case for simple orders
        else
        {
            orderStatus = "Standard";
            estimatedTimeMinutes = 4;
        }

        // --- PART 7: DISPLAYING THE RECEIPT ---
        // We use String Interpolation ($) to format the output nicely.
        Console.WriteLine("========================================");
        Console.WriteLine($"COFFEE SHOP ORDER RECEIPT");
        Console.WriteLine($"Customer: {customerName}");
        Console.WriteLine("========================================");
        Console.WriteLine($"Drink: {drinkType} ({size})");
        Console.WriteLine($"Base Price: ${basePrice:F2}");
        Console.WriteLine($"Size Multiplier: {sizeMultiplier}x");
        Console.WriteLine($"Price after Size: ${priceAfterSize:F2}");
        Console.WriteLine($"Extras Cost: ${extrasCost:F2}");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Subtotal: ${subtotal:F2}");
        Console.WriteLine($"Discount (Loyalty): -${discountAmount:F2}");
        Console.WriteLine($"FINAL TOTAL: ${finalPrice:F2}");
        Console.WriteLine("========================================");
        Console.WriteLine($"ORDER STATUS: {orderStatus}");
        Console.WriteLine($"ESTIMATED TIME: {estimatedTimeMinutes} minutes");
        Console.WriteLine("========================================");

        // Edge Case Handling: Display specific notes based on the order details.
        // We use the NOT operator '!' to check if a condition is false.
        if (!wantsSugar)
        {
            Console.WriteLine(">> No sugar added as requested.");
        }
        else
        {
            Console.WriteLine(">> Sugar added.");
        }

        // We use the '==' operator to check for exact matches.
        if (drinkType == "Espresso")
        {
            Console.WriteLine(">> Enjoy your strong espresso!");
        }
        else if (drinkType == "Latte")
        {
            Console.WriteLine(">> Enjoy your creamy latte!");
        }
    }
}
