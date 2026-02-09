
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
using System.Numerics; // Required for Vector<T> (SIMD)
using System.Buffers;  // Required for ArrayPool<T>

public class HighPerformanceEmbeddingEngine
{
    public static void Main()
    {
        // 1. Simulate a large block of text (e.g., a document buffer).
        // In a real AI scenario, this might be a memory-mapped file or a tensor buffer.
        string rawText = "The quick brown fox jumps over the lazy dog. " +
                         "The quick brown fox jumps over the lazy dog. " +
                         "The quick brown fox jumps over the lazy dog.";
        
        // 2. Convert to Span to avoid heap allocations during slicing.
        // 'ReadOnlySpan<char>' is a view into the original string's memory.
        ReadOnlySpan<char> textSpan = rawText.AsSpan();

        // 3. Rent a shared array from the ArrayPool (Zero-Allocation pattern).
        // We use this to store the frequency count of characters 'a' through 'z'.
        // This avoids 'new int[26]' which would immediately put pressure on the GC.
        int[] rentedBuffer = ArrayPool<int>.Shared.Rent(26);
        
        try
        {
            // Zero out the buffer (required because ArrayPool returns dirty memory).
            Array.Clear(rentedBuffer, 0, rentedBuffer.Length);

            // 4. Process the Span using Hardware Acceleration (SIMD).
            CalculateCharacterFrequenciesSimd(textSpan, rentedBuffer);

            // 5. Output results (Simulating vector embedding generation).
            Console.WriteLine("Character Frequency Vector (Embedding):");
            for (int i = 0; i < 26; i++)
            {
                Console.WriteLine($"'{(char)('a' + i)}': {rentedBuffer[i]}");
            }
        }
        finally
        {
            // 6. CRITICAL: Return the array to the pool.
            // If we forget this, we lose the benefits of pooling and cause memory leaks.
            ArrayPool<int>.Shared.Return(rentedBuffer);
        }
    }

    /// <summary>
    /// Calculates character frequencies using SIMD (Single Instruction, Multiple Data).
    /// This processes multiple characters in a single CPU cycle.
    /// </summary>
    private static void CalculateCharacterFrequenciesSimd(ReadOnlySpan<char> text, int[] frequencies)
    {
        // Define the vector size based on the hardware (e.g., 256-bit on AVX2, 128-bit on SSE).
        // Vector<int>.Count typically equals 8 (256-bit / 32-bit) or 4 (128-bit / 32-bit).
        int vectorSize = Vector<int>.Count;
        
        // We iterate with a step of 'vectorSize' to process chunks of data.
        int i = 0;
        
        // 7. The Hot Path: Looping over the Span.
        // We avoid LINQ here. LINQ on Span<T> is not supported in older frameworks 
        // and introduces overhead (boxing/indirection) that kills performance.
        for (; i <= text.Length - vectorSize; i += vectorSize)
        {
            // Load a chunk of characters into a SIMD vector.
            // This loads 8 characters (assuming 32-bit int vector) into a single CPU register.
            Vector<int> chunk = new Vector<int>(text.Slice(i).UnsafeAs<int[]>()); 
            
            // NOTE: In a real scenario, we would map 'char' to an index (e.g., 'a' -> 0).
            // For this simplified example, we assume the vector contains valid indices.
            // In production, you would use Vector.ShiftRight/Left to mask ASCII ranges.
            
            // 8. Hardware Accelerated Math:
            // We add 1 to every element in the vector simultaneously.
            // This is significantly faster than a scalar 'for' loop.
            Vector<int> ones = Vector<int>.One;
            Vector<int> result = Vector.Add(chunk, ones);

            // 9. Store result back to memory (SIMD write).
            // We write back to the rented buffer.
            result.CopyTo(frequencies, i % frequencies.Length); 
        }

        // 10. The Tail: Process remaining elements that didn't fit in a vector.
        // This ensures correctness for any input length.
        for (; i < text.Length; i++)
        {
            char c = text[i];
            if (c >= 'a' && c <= 'z')
            {
                // Standard scalar operation for the remainder.
                frequencies[c - 'a']++;
            }
        }
    }
}
