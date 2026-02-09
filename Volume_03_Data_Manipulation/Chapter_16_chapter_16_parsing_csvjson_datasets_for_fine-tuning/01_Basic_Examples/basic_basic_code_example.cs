
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
using System.Buffers;
using System.Numerics; // Required for Vector<T> (SIMD)
using System.Runtime.InteropServices; // For MemoryMarshal

public static class HighPerformanceTokenizer
{
    // Simulates a fixed-size buffer from a stream reader (e.g., 4KB chunk).
    // In a real scenario, this is likely a byte array rented from ArrayPool<byte>.
    private static readonly byte[] _rawDataBuffer = System.Text.Encoding.UTF8.GetBytes(
        "101,Hello world from the tensor buffer");

    public static void Process()
    {
        // 1. Zero-Allocation Slicing using Span<T>
        // We treat the raw byte array as a contiguous block of memory.
        // 'AsSpan()' creates a lightweight view (reference + length) without copying data.
        Span<byte> bufferSpan = _rawDataBuffer.AsSpan();

        // 2. Find the delimiter (comma) to separate ID from Text.
        // We use IndexOf for high-performance searching.
        int commaIndex = bufferSpan.IndexOf((byte)',');

        if (commaIndex == -1) return; // Invalid format

        // 3. Parse the ID (first segment) from the Span.
        // We slice the Span from the start to the comma.
        // This is a zero-allocation operation (just adjusting pointer/length).
        Span<byte> idSpan = bufferSpan.Slice(0, commaIndex);

        // 4. Parse the Text (second segment).
        // Slice from after the comma to the end.
        Span<byte> textSpan = bufferSpan.Slice(commaIndex + 1);

        // 5. Convert ID Span to a numerical value.
        // We use System.Text.Encoding to parse the bytes to an integer.
        // Note: In ultra-hot paths, we might implement a custom Atoi (ASCII to Integer) loop.
        int idValue = int.Parse(System.Text.Encoding.UTF8.GetString(idSpan));

        // 6. Vectorized Processing (SIMD) on the Text.
        // We convert the text bytes to numerical tokens (simulated by casting byte to float).
        // We use Vector<T> to process multiple data points in a single CPU instruction.
        // This is significantly faster than a standard foreach loop.
        
        // Allocate a small buffer on the STACK for the tokens (Zero Heap Allocation).
        // 'stackalloc' creates memory that lives only within the current method scope.
        // We calculate the vector count. Vector<float>.Count is typically 4 (AVX) or 8 (AVX512).
        int vectorSize = Vector<float>.Count;
        Span<float> tokenBuffer = stackalloc float[textSpan.Length]; 

        // Process the textSpan in chunks using SIMD
        int i = 0;
        for (; i <= textSpan.Length - vectorSize; i += vectorSize)
        {
            // Load a chunk of bytes into a Vector.
            // This assumes the underlying data is aligned; if not, Vector.LoadUnsafe is used.
            Vector<byte> byteVector = Vector.LoadUnsafe(ref textSpan[i]);
            
            // Convert bytes to floats (Widening). 
            // In a real tokenizer, this is where we map bytes to vocabulary indices.
            // Here, we simply cast to float to demonstrate the math.
            Vector<float> floatVector = Vector.ConvertToSingle(byteVector);

            // Store the result back into our stack-allocated buffer.
            floatVector.StoreUnsafe(ref tokenBuffer[i]);
        }

        // 7. Handle the "Tail" (Remaining elements not fitting in a Vector).
        // Standard loops finish the job for the remainder.
        for (; i < textSpan.Length; i++)
        {
            tokenBuffer[i] = (float)textSpan[i];
        }

        // 8. Output verification (Simulated)
        Console.WriteLine($"Parsed ID: {idValue}");
        Console.WriteLine($"First Token: {tokenBuffer[0]}");
        
        // 9. Memory Management
        // 'stackalloc' memory is automatically reclaimed when the method returns.
        // If we had used ArrayPool<float>.Shared.Rent(), we would explicitly return it here.
    }
}
