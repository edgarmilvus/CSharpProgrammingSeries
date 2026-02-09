
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

class Program
{
    static void Main()
    {
        // --- Variable Declaration and Initialization ---
        
        // Declaring an integer variable to store age.
        // An 'int' is a whole number (no decimals).
        int age = 30;

        // Declaring a double variable to store a precise height.
        // A 'double' is a floating-point number (includes decimals).
        double height = 1.75;

        // Declaring a string variable to store a name.
        // A 'string' is a sequence of characters (text).
        string name = "Alice";

        // Declaring a boolean variable to store a student status.
        // A 'bool' can only be true or false.
        bool isStudent = false;

        // --- Displaying the Values ---
        
        // Using Console.WriteLine to output the variables to the console.
        // This method automatically adds a new line at the end.
        Console.WriteLine("--- User Profile ---");
        Console.WriteLine("Name: " + name);
        Console.WriteLine("Age: " + age);
        Console.WriteLine("Height: " + height + " meters");
        
        // Using Console.Write to output without a new line.
        // This keeps the cursor on the same line.
        Console.Write("Is a student? ");
        Console.WriteLine(isStudent);
    }
}
