
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;

// A class represents a real-world entity.
// Here, we define a 'Car' that needs to track its speed.
public class Car
{
    // 1. FIELD DECLARATION
    // A field is a variable declared directly inside a class.
    // We use 'private' to restrict access to only code inside this class.
    // This protects the data from being changed incorrectly.
    private int _currentSpeed;

    // 2. PROPERTY DECLARATION
    // A property provides controlled access to a field.
    // It uses 'get' and 'set' accessors.
    public int Speed
    {
        // The 'get' accessor allows reading the value.
        get { return _currentSpeed; }

        // The 'set' accessor allows writing a value.
        // 'value' is a keyword representing the data being assigned.
        set { _currentSpeed = value; }
    }

    // A method to demonstrate behavior
    public void DisplayStatus()
    {
        Console.WriteLine($"The car is traveling at {Speed} mph.");
    }
}

public class Program
{
    public static void Main()
    {
        // 3. CREATING AN INSTANCE
        // We create an object of the Car class using 'new'.
        Car myCar = new Car();

        // 4. USING THE PROPERTY
        // We can assign a value to the 'Speed' property.
        // This actually calls the 'set' accessor in the Car class.
        myCar.Speed = 60;

        // 5. READING THE PROPERTY
        // We read the value using the 'Speed' property.
        // This calls the 'get' accessor.
        Console.WriteLine($"Current speed: {myCar.Speed} mph");

        // 6. CALLING A METHOD
        // We use the object's method to display the status.
        myCar.DisplayStatus();

        // 7. MODIFYING THE STATE
        // Let's change the speed.
        myCar.Speed = 75;
        Console.WriteLine("Accelerating...");
        myCar.DisplayStatus();
    }
}
