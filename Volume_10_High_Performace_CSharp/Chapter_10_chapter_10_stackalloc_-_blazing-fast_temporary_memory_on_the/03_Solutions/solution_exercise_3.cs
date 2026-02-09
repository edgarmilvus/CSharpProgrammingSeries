
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
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class BatchPipeline
{
    // Threshold to switch from stackalloc to ArrayPool
    private const int StackThreshold = 1024;

    public static async Task<int[]> ProcessBatchAsync(string[] inputs)
    {
        var results = new int[inputs.Length];

        // Use Task.WhenAll for concurrency
        var tasks = new Task[inputs.Length];
        
        for (int i = 0; i < inputs.Length; i++)
        {
            int index = i; // Capture index for closure
            tasks[i] = Task.Run(() => 
            {
                results[index] = ProcessSingleInput(inputs[index]);
            });
        }

        await Task.WhenAll(tasks);
        return results;
    }

    private static int ProcessSingleInput(string input)
    {
        // Convert string to UTF8 bytes. 
        // Note: Encoding.UTF8.GetBytes allocates a new byte[].
        // To be strictly zero-allocation, we would need to use an encoder that writes to a Span.
        // However, for the scope of this exercise, we assume the input conversion is necessary 
        // and focus on the processing logic which handles the buffer allocation.
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(input);
        ReadOnlySpan<byte> utf8Span = utf8Bytes.AsSpan();

        // Hybrid Allocation Strategy
        bool usePool = utf8Span.Length > StackThreshold;
        byte[] rentedArray = null;
        Span<byte> buffer = usePool 
            ? (rentedArray = ArrayPool<byte>.Shared.Rent(utf8Span.Length)) 
            : stackalloc byte[StackThreshold];

        try
        {
            // Ensure we only use the valid length of the buffer
            Span<byte> validBuffer = buffer.Slice(0, utf8Span.Length);
            utf8Span.CopyTo(validBuffer);

            // SIMBOL: Vectorized Hashing (Sum of bytes)
            // We process the buffer in chunks using Vector<byte>
            int hash = 0;
            int i = 0;
            int vectorSize = Vector<byte>.Count;

            // Vectorized summation
            Vector<int> sumVector = Vector<int>.Zero;
            
            // Note: We can't easily sum bytes into a Vector<int> directly without casting.
            // A common trick is to treat the bytes as sbyte or use Widen.
            // For simplicity in this specific exercise, we will sum bytes using a scalar loop 
            // but the requirement asks for Vector usage. Let's use Vector.Sum(Vector<byte>)
            // on chunks to demonstrate the SIMD acceleration.
            
            for (; i <= validBuffer.Length - vectorSize; i += vectorSize)
            {
                Vector<byte> chunk = Vector.LoadUnsafe(ref validBuffer[i]);
                // Summing bytes in a vector and adding to total
                // Note: Vector.Sum returns the sum of all elements in the vector.
                // This is a valid use of SIMD to accelerate the summation part of hashing.
                hash += Vector.Sum(chunk);
            }

            // Scalar tail
            for (; i < validBuffer.Length; i++)
            {
                hash += validBuffer[i];
            }

            return hash;
        }
        finally
        {
            // Critical: Return the array to the pool if we rented it
            if (rentedArray != null)
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
    }
}
