
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;

public class Car
{
    // Private fields (cannot be accessed directly from Main)
    private string _make;
    private string _model;
    private int _year;

    // The Constructor: Runs automatically when 'new' is called
    public Car(string make, string model, int year)
    {
        // 'this' distinguishes the class field from the parameter
        this._make = make;
        this._model = model;
        this._year = year;
    }

    // A public method to allow Main to see the data
    public void DisplayInfo()
    {
        Console.WriteLine($"Car: {_year} {_make} {_model}");
    }
}

public class Program
{
    public static void Main()
    {
        // Create the car instance, passing arguments to the constructor
        Car myCar = new Car("Toyota", "Corolla", 2024);

        // Call the method to display the initialized data
        myCar.DisplayInfo();
    }
}
