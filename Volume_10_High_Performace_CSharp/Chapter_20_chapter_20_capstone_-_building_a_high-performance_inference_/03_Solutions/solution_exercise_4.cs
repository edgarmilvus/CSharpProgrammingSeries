
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
using System.Buffers;

/// <summary>
/// A struct that manages a rented array from ArrayPool.
/// Structs are value types, but we must be careful with Dispose patterns.
/// </summary>
public struct PooledTensorBuffer : IDisposable
{
    private float[] _rentedArray;
    private readonly int _length;

    public PooledTensorBuffer(int length)
    {
        _length = length;
        // Rent the array from the shared pool
        _rentedArray = ArrayPool<float>.Shared.Rent(length);
    }

    /// <summary>
    /// Returns a Span view into the rented buffer.
    /// </summary>
    public Span<float> AsSpan() => _rentedArray.AsSpan(0, _length);

    /// <summary>
    /// Returns a slice of the buffer without allocating.
    /// </summary>
    public Span<float> GetSlice(int start, int length)
    {
        if (start < 0 || length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException();
        return _rentedArray.AsSpan(start, length);
    }

    /// <summary>
    /// Crucial: Returns the array to the pool.
    /// </summary>
    public void Dispose()
    {
        if (_rentedArray != null)
        {
            ArrayPool<float>.Shared.Return(_rentedArray);
            _rentedArray = null; // Prevent double return
        }
    }
}

public static class ForwardPassSimulator
{
    public static void RunSimulation()
    {
        Console.WriteLine("Starting Forward Pass Simulation...");

        // 1. Rent a buffer for intermediate calculations
        using (var buffer = new PooledTensorBuffer(1024))
        {
            // 2. Get a span for the whole buffer
            Span<float> tensor = buffer.AsSpan();
            
            // Simulate filling data
            tensor.Fill(1.0f);

            // 3. Perform calculation on a slice
            Span<float> subTensor = buffer.GetSlice(0, 64);
            for (int i = 0; i < subTensor.Length; i++)
            {
                subTensor[i] *= 2.0f; // Example op
            }
            
            Console.WriteLine($"Calculated value: {subTensor[0]}");
        } // Buffer is automatically disposed (returned to pool) here

        // 4. Demonstrate stackalloc for small, fixed buffers
        // e.g., Positional Embeddings (small fixed size)
        Span<float> posEmbeddings = stackalloc float[16];
        InitializePosEmbeddings(posEmbeddings);
        Console.WriteLine($"Stackalloc value: {posEmbeddings[0]}");
    }

    private static void InitializePosEmbeddings(Span<float> embeddings)
    {
        for (int i = 0; i < embeddings.Length; i++)
        {
            embeddings[i] = i * 0.1f;
        }
    }
}
