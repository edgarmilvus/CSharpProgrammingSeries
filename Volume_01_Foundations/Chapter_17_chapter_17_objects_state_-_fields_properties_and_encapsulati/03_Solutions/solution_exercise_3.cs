
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;

public class Car
{
    // Mutable properties (can be changed after creation)
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }

    // Immutable property from outside perspective
    // 'private set' allows the class itself to set it, but not external code
    public string Vin { get; private set; }

    // Constructor: Used to initialize the object
    public Car(string make, string model, int year, string vin)
    {
        Make = make;
        Model = model;
        Year = year;
        
        // We can set Vin here because we are inside the class definition
        Vin = vin;
    }
}

public class Program
{
    public static void Main()
    {
        // Create a new Car instance using the constructor
        Car myCar = new Car("Toyota", "Camry", 2023, "1HGBH41JXMN109186");

        // Print initial details
        Console.WriteLine($"Initial Car: {myCar.Year} {myCar.Make} {myCar.Model}, VIN: {myCar.Vin}");

        // This is allowed because Year has a public setter
        myCar.Year = 2024;
        Console.WriteLine($"Updated Year: {myCar.Year}");

        // This line would cause a compile-time error:
        // Error: The property 'Car.Vin' cannot be used in this context because the set accessor is inaccessible.
        // myCar.Vin = "NEWVIN123"; 
    }
}
