
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
using System.Linq;

// Simple Tensor class representing a 2D array of doubles
// This is a placeholder for a real tensor library
public class Tensor
{
    public double[,] Data { get; private set; }
    
    public Tensor(int rows, int cols)
    {
        Data = new double[rows, cols];
    }
    
    public Tensor(double[,] data)
    {
        Data = data;
    }
    
    // Helper method to display tensor contents
    public void Display(string name = "Tensor")
    {
        Console.WriteLine($"{name} (Shape: {Data.GetLength(0)}x{Data.GetLength(1)}):");
        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Console.Write($"{Data[i, j]:F2}\t");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

// Extension methods for fluent tensor operations
public static class TensorExtensions
{
    // Scale operation using lambda expression for transformation
    public static Tensor Scale(this Tensor tensor, double scalar)
    {
        // Create new tensor with same dimensions
        var result = new Tensor(tensor.Data.GetLength(0), tensor.Data.GetLength(1));
        
        // Apply scaling using lambda expression for element-wise operation
        ForEachElement(tensor, result, (x, y) => tensor.Data[x, y] * scalar);
        
        return result;
    }
    
    // Add operation for element-wise addition
    public static Tensor Add(this Tensor tensor, Tensor other)
    {
        if (tensor.Data.GetLength(0) != other.Data.GetLength(0) || 
            tensor.Data.GetLength(1) != other.Data.GetLength(1))
            throw new ArgumentException("Tensors must have the same dimensions");
            
        var result = new Tensor(tensor.Data.GetLength(0), tensor.Data.GetLength(1));
        
        // Use lambda for element-wise addition
        ForEachElement(tensor, result, (x, y) => tensor.Data[x, y] + other.Data[x, y]);
        
        return result;
    }
    
    // Map operation using delegate - applies a function to each element
    public static Tensor Map(this Tensor tensor, Func<double, double> transformation)
    {
        var result = new Tensor(tensor.Data.GetLength(0), tensor.Data.GetLength(1));
        
        // Apply custom transformation using delegate
        ForEachElement(tensor, result, (x, y) => transformation(tensor.Data[x, y]));
        
        return result;
    }
    
    // Helper method for element-wise operations
    private static void ForEachElement(Tensor source, Tensor destination, 
                                      Func<int, int, double> operation)
    {
        for (int i = 0; i < source.Data.GetLength(0); i++)
        {
            for (int j = 0; j < source.Data.GetLength(1); j++)
            {
                destination.Data[i, j] = operation(i, j);
            }
        }
    }
}

// Example usage in an AI chain scenario
class Program
{
    static void Main()
    {
        // Create initial input tensor (simulating neural network input)
        var inputTensor = new Tensor(new double[,]
        {
            { 1.0, 2.0 },
            { 3.0, 4.0 }
        });
        
        inputTensor.Display("Input");
        
        // Build fluent AI chain using extension methods
        // This demonstrates lazy evaluation - operations are chained but not executed until needed
        var processedTensor = inputTensor
            .Scale(0.5)                    // Scale input by factor 0.5
            .Add(new Tensor(new double[,]   // Add bias tensor
            {
                { 0.1, 0.2 },
                { 0.3, 0.4 }
            }))
            .Map(x => Math.Max(0, x));      // Apply ReLU activation using lambda
        
        processedTensor.Display("Processed (after chain)");
        
        // Demonstrate delegate usage with custom transformation
        var customTransform = inputTensor
            .Map(x => x * x + 2)            // Custom quadratic transformation
            .Scale(0.1);
            
        customTransform.Display("Custom Transform");
        
        // Additional example: Chaining with multiple lambda expressions
        var complexChain = inputTensor
            .Scale(2.0)
            .Map(x => x > 2.5 ? 1.0 : 0.0)  // Thresholding with lambda
            .Add(new Tensor(new double[,] { { 0.5, 0.5 }, { 0.5, 0.5 } }));
            
        complexChain.Display("Complex Chain");
    }
}
