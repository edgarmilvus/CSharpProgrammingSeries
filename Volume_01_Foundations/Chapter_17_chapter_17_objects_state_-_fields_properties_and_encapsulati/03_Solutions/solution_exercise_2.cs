
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;

public class Product
{
    // Private backing fields
    private string _name;
    private double _price;
    private int _stock;

    // Property for Name (Auto-implemented for simplicity)
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    // Property for Price with custom validation logic
    public double Price
    {
        get { return _price; }
        set
        {
            // Check if the value being assigned is valid
            if (value >= 0)
            {
                _price = value;
            }
            else
            {
                Console.WriteLine($"Error: Cannot set price to {value:C}. Price remains unchanged.");
            }
        }
    }

    // Property for Stock with custom validation logic
    public int Stock
    {
        get { return _stock; }
        set
        {
            // Check if the value being assigned is valid
            if (value >= 0)
            {
                _stock = value;
            }
            else
            {
                Console.WriteLine($"Error: Cannot set stock to {value}. Stock remains unchanged.");
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        Product coffeeMug = new Product();

        // Setting properties
        coffeeMug.Name = "Coffee Mug";
        coffeeMug.Price = 12.99;
        coffeeMug.Stock = 50;

        // Printing details
        Console.WriteLine($"Product: {coffeeMug.Name}, Price: {coffeeMug.Price:C}, Stock: {coffeeMug.Stock}");

        // Attempting invalid assignments
        Console.WriteLine("\nAttempting invalid updates...");
        coffeeMug.Price = -5.00; // This will trigger the error message in the setter
        coffeeMug.Stock = -10;   // This will trigger the error message in the setter

        // Verify values haven't changed
        Console.WriteLine($"After invalid updates -> Product: {coffeeMug.Name}, Price: {coffeeMug.Price:C}, Stock: {coffeeMug.Stock}");
    }
}
