
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;

// 1. Interface definition
public interface ITensorOperable
{
    void Multiply(double scalar);
    void Add(double scalar);
}

// 2. Implementation 1: Valid
public class StandardTensor : ITensorOperable
{
    public double Value { get; private set; }

    public StandardTensor(double initialValue)
    {
        Value = initialValue;
    }

    public void Multiply(double scalar)
    {
        Value *= scalar;
        Console.WriteLine($"StandardTensor multiplied by {scalar}. New Value: {Value}");
    }

    public void Add(double scalar)
    {
        Value += scalar;
        Console.WriteLine($"StandardTensor added {scalar}. New Value: {Value}");
    }
}

// 2. Implementation 2: Invalid (Missing Interface)
public class ImageTensor
{
    public double PixelData { get; set; }

    public void ProcessImage()
    {
        Console.WriteLine("Processing image specific logic...");
    }
}

// 3. Extension Class with Constraints
public static class LayerExtensions
{
    // The constraint 'where T : ITensorOperable' ensures this method 
    // is only available on types implementing the interface.
    public static void ApplyRelu<T>(this T tensor) where T : ITensorOperable
    {
        Console.WriteLine($"Applying ReLU to {typeof(T).Name}...");
        
        // Simulating ReLU logic based on the interface contract
        for (int i = 0; i < 10; i++)
        {
            if (i % 2 == 0)
            {
                // Simulate negative input -> set to 0
                tensor.Multiply(0); 
            }
            else
            {
                // Simulate positive input -> leave as is (add 0)
                tensor.Add(0);
            }
        }
    }
}

// 4. Usage
public class Program
{
    public static void Main()
    {
        // Case A: Valid usage
        var standard = new StandardTensor(5.0);
        standard.ApplyRelu(); // Compiles: StandardTensor implements ITensorOperable
        Console.WriteLine($"Final StandardTensor Value: {standard.Value}");

        // Case B: Invalid usage
        var image = new ImageTensor();
        // image.ApplyRelu(); // UNCOMMENTING THIS LINE CAUSES A COMPILE ERROR
        // Error CS1061: 'ImageTensor' does not contain a definition for 'ApplyRelu'
        
        Console.WriteLine("\nCompile-time check successful: ImageTensor cannot call ApplyRelu.");
    }
}
