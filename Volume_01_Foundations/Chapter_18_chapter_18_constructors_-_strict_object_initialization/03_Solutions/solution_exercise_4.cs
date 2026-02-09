
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;

public class Rectangle
{
    // Properties with private setters to control modification
    public double Width { get; private set; }
    public double Height { get; private set; }

    // Constructor with Validation
    public Rectangle(double width, double height)
    {
        // Logic for Width
        if (width < 0)
        {
            throw new Exception("Width cannot be negative.");
        }
        else if (width == 0)
        {
            Width = 1; // Auto-correct to minimum size
        }
        else
        {
            Width = width;
        }

        // Logic for Height
        if (height < 0)
        {
            throw new Exception("Height cannot be negative.");
        }
        else if (height == 0)
        {
            Height = 1; // Auto-correct to minimum size
        }
        else
        {
            Height = height;
        }
    }

    public double GetArea()
    {
        return Width * Height;
    }

    public double GetPerimeter()
    {
        return 2 * (Width + Height);
    }
}

public class Program
{
    public static void Main()
    {
        // Test 1: Valid dimensions
        try
        {
            Rectangle rect1 = new Rectangle(10, 5);
            Console.WriteLine($"Area: {rect1.GetArea()}, Perimeter: {rect1.GetPerimeter()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Test 2: Zero dimensions (should auto-correct to 1)
        try
        {
            Rectangle rect2 = new Rectangle(0, 10);
            Console.WriteLine($"Zero width corrected. Area: {rect2.GetArea()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Test 3: Negative dimensions (should throw exception)
        try
        {
            Rectangle rect3 = new Rectangle(-5, 5);
            Console.WriteLine($"Area: {rect3.GetArea()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating rect3: {ex.Message}");
        }
    }
}
