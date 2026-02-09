
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

# Source File: theory_theoretical_foundations_part9.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Numerics; // For Vector<T> if available

// Assume an ITensor interface from a previous chapter
public interface ITensor<T> where T : struct
{
    T[] Data { get; }
    int[] Shape { get; }
}

public class Tensor<T> : ITensor<T> where T : struct
{
    public T[] Data { get; set; }
    public int[] Shape { get; set; }

    public Tensor(T[] data, int[] shape)
    {
        Data = data;
        Shape = shape;
    }
}

// Extension methods constrained to ITensor<T>
public static class TensorExtensions
{
    // Extension method for element-wise addition
    public static ITensor<T> Add<T>(
        this ITensor<T> tensor1, 
        ITensor<T> tensor2) 
        where T : struct, IAdditionOperators<T, T, T> // C# 11+ for generic math
    {
        if (tensor1.Shape.Length != tensor2.Shape.Length)
            throw new ArgumentException("Shape mismatch");

        for (int i = 0; i < tensor1.Shape.Length; i++)
        {
            if (tensor1.Shape[i] != tensor2.Shape[i])
                throw new ArgumentException($"Dimension {i} mismatch");
        }

        var resultData = new T[tensor1.Data.Length];
        for (int i = 0; i < tensor1.Data.Length; i++)
        {
            resultData[i] = tensor1.Data[i] + tensor2.Data[i];
        }

        return new Tensor<T>(resultData, tensor1.Shape);
    }
}

// Usage
class Program
{
    static void Main()
    {
        var tensor1 = new Tensor<int>(new[] { 1, 2, 3 }, new[] { 3 });
        var tensor2 = new Tensor<int>(new[] { 4, 5, 6 }, new[] { 3 });

        var result = tensor1.Add(tensor2); // Fluent chain starter
        // Could chain further: result.Multiply(2).Sum()
    }
}
