
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;

public class Car
{
    // Mandatory fields (Set via Constructor)
    public string Make { get; }
    public string Model { get; }
    public int Year { get; }

    // Optional fields (Set via Properties/Object Initializer)
    public string Color { get; set; }
    public double Mileage { get; set; }

    // Constructor
    public Car(string make, string model, int year)
    {
        Make = make;
        Model = model;
        Year = year;
        
        // Set defaults for optional properties
        Color = "White";
        Mileage = 0.0;
    }

    public void DisplayDetails()
    {
        Console.WriteLine($"{Year} {Make} {Model} ({Color}) - {Mileage} miles");
    }
}

public class Program
{
    public static void Main()
    {
        // 1. Create using constructor only (Defaults for color/mileage)
        // The constructor sets Color to "White" and Mileage to 0.0
        Car car1 = new Car("Toyota", "Corolla", 2022);
        car1.DisplayDetails();

        // 2. Create using constructor + object initializer
        // The constructor runs first (setting Make/Model/Year and defaults).
        // Then the object initializer { ... } runs to override the defaults.
        Car car2 = new Car("Ford", "Mustang", 2024)
        {
            Color = "Red",
            Mileage = 1500.5
        };
        car2.DisplayDetails();
    }
}
