
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

public class HighPerformanceImputation
{
    // Memory<T> is stored here to hold ownership of the rented array
    private Memory<float> _dataBuffer;

    public void ProcessData(int size)
    {
        // 1. Allocation Strategy: Rent from ArrayPool to avoid Heap Gen 0 GC
        float[] rentedArray = ArrayPool<float>.Shared.Rent(size);
        _dataBuffer = rentedArray.AsMemory(0, size);

        try
        {
            // Simulate loading data with missing values (NaN)
            InitializeDataWithMissingValues(_dataBuffer.Span);

            // 2. Calculate Mean (using Span for zero-copy access)
            float mean = CalculateMean(_dataBuffer.Span);

            // 3. Impute Missing Values (SIMD Accelerated)
            ImputeMissingValues(_dataBuffer.Span, mean);

            // 4. Use the data for AI Embedding (e.g., passing to a tensor)
            // Since we used Span, we didn't allocate new arrays during processing.
            ConsumeForInference(_dataBuffer.Span);
        }
        finally
        {
            // 5. Return the array to the pool. Crucial for long-running apps.
            ArrayPool<float>.Shared.Return(rentedArray);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float CalculateMean(Span<float> data)
    {
        // We cannot use LINQ on Span. We use a simple loop.
        // For very large spans, we could use Vector<T> to sum chunks.
        double sum = 0;
        int count = 0;

        int i = 0;
        int length = data.Length;
        
        // Process in chunks for better cache locality
        const int blockSize = 64; 
        for (; i <= length - blockSize; i += blockSize)
        {
            var block = data.Slice(i, blockSize);
            for (int j = 0; j < blockSize; j++)
            {
                float val = block[j];
                if (!float.IsNaN(val))
                {
                    sum += val;
                    count++;
                }
            }
        }

        // Handle remaining elements
        for (; i < length; i++)
        {
            if (!float.IsNaN(data[i]))
            {
                sum += data[i];
                count++;
            }
        }

        return count == 0 ? 0 : (float)(sum / count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ImputeMissingValues(Span<float> data, float replacementValue)
    {
        int i = 0;
        int length = data.Length;
        
        // SIMD Vectorization Setup
        // Vector<T>.Count depends on hardware (e.g., 8 for float on AVX2)
        int vectorSize = Vector<float>.Count;
        Vector<float> replacementVector = new Vector<float>(replacementValue);

        // Vectorized loop: Processes multiple floats in a single CPU instruction
        for (; i <= length - vectorSize; i += vectorSize)
        {
            var slice = data.Slice(i, vectorSize);
            Vector<float> vector = new Vector<float>(slice);
            
            // Check for NaN using SIMD is complex because NaN != NaN.
            // A common trick is to compare the value to itself. 
            // However, standard Vector<T> lacks direct NaN checks.
            // For high performance, we often do a scalar check or use Avx intrinsics directly.
            // Here, we stick to scalar check inside the vector block for safety, 
            // or we can assume the data is dense and just replace if we are doing bulk operations.
            
            // For this example, we will do a scalar check per element in the vector block
            // to ensure correctness, as Vector<T> doesn't have IsNaN.
            for (int j = 0; j < vectorSize; j++)
            {
                if (float.IsNaN(slice[j]))
                {
                    slice[j] = replacementValue;
                }
            }
        }

        // Handle remaining elements
        for (; i < length; i++)
        {
            if (float.IsNaN(data[i]))
            {
                data[i] = replacementValue;
            }
        }
    }

    private void InitializeDataWithMissingValues(Span<float> data)
    {
        // Fill with random data and some NaNs
        Random rnd = new Random(42);
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (rnd.NextDouble() > 0.1) ? (float)rnd.NextDouble() * 100 : float.NaN;
        }
    }

    private void ConsumeForInference(Span<float> data)
    {
        // In an AI context, this Span would be passed to a Tensor constructor
        // or a native binding (like ONNX Runtime) without copying.
        // Example: Tensor.Create(data, dimensions);
        Console.WriteLine($"Processed {data.Length} elements with zero heap allocations.");
    }
}
