
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

# Source File: theory_theoretical_foundations_part6.cs
# Description: Theoretical Foundations
# ==========================================

using System;

// Assume a Tensor class (simplified for illustration)
public class Tensor
{
    public float[] Data { get; set; }
    public int[] Shape { get; set; }

    public Tensor(float[] data, int[] shape)
    {
        Data = data;
        Shape = shape;
    }
}

public static class TensorExtensions
{
    // Extension method for reshaping
    public static Tensor Reshape(this Tensor tensor, int[] newShape)
    {
        // Validation: Ensure total elements match
        int totalElements = 1;
        foreach (var dim in newShape) totalElements *= dim;
        if (tensor.Data.Length != totalElements)
            throw new ArgumentException("Shape mismatch");

        return new Tensor(tensor.Data, newShape);
    }

    // Extension method for scalar multiplication
    public static Tensor Multiply(this Tensor tensor, float scalar)
    {
        var newData = new float[tensor.Data.Length];
        for (int i = 0; i < tensor.Data.Length; i++)
        {
            newData[i] = tensor.Data[i] * scalar;
        }
        return new Tensor(newData, tensor.Shape);
    }

    // Extension method for summation
    public static float Sum(this Tensor tensor)
    {
        float sum = 0;
        foreach (var val in tensor.Data)
        {
            sum += val;
        }
        return sum;
    }
}

// Usage
class Program
{
    static void Main()
    {
        var tensor = new Tensor(new[] { 1f, 2f, 3f, 4f }, new[] { 2, 2 });

        // Fluent chain
        var result = tensor
            .Reshape(new[] { 4 }) // Flatten to 1D
            .Multiply(2f)         // Multiply each element by 2
            .Sum();               // Sum all elements

        Console.WriteLine(result); // Output: 20 (1+2+3+4=10, *2=20)
    }
}
