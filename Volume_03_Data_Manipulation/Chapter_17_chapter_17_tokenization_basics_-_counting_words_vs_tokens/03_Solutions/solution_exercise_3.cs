
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

public class PooledFrequencyCounter
{
    public static void CountFrequencies(ReadOnlySpan<char> text)
    {
        // Rent array from the shared pool. 
        // This retrieves a reusable array from a global cache.
        // We rent slightly more than needed to cover all char values (0-65535).
        int[] rentedArray = ArrayPool<int>.Shared.Rent(65536); 

        try
        {
            // IMPORTANT: Rented arrays are dirty. We must clear them.
            // We use Span for this to avoid unsafe code and ensure performance.
            rentedArray.AsSpan().Clear();

            // Increment frequencies
            foreach (char c in text)
            {
                // Safe cast to ushort (0-65535) to use as index
                ushort index = (ushort)c;
                rentedArray[index]++;
            }

            // Demonstrate reading data
            // Let's print counts for 'A' (65) and 'a' (97)
            Console.WriteLine($"Frequency of 'A': {rentedArray['A']}");
            Console.WriteLine($"Frequency of 'a': {rentedArray['a']}");
            
            // In a real scenario, you would convert this to a Dictionary or process it here.
        }
        finally
        {
            // RETURN TO POOL. If you forget this, the array stays in the process memory
            // but becomes unavailable for other requests, causing a "memory leak" in the pool.
            ArrayPool<int>.Shared.Return(rentedArray);
        }
    }

    public static void Run()
    {
        string text = "Hello High-Performance C#";
        CountFrequencies(text);
    }
}
