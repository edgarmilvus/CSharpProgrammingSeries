
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Buffers; // Required for ArrayPool

public void CountCharactersInSegment(string segment)
{
    // Rent an array from the shared pool.
    // This avoids a heap allocation.
    char[] buffer = ArrayPool<char>.Shared.Rent(segment.Length);

    try
    {
        // Copy data into the rented buffer.
        segment.AsSpan().CopyTo(buffer);

        // Process the buffer (e.g., count vowels).
        int vowelCount = 0;
        for (int i = 0; i < segment.Length; i++)
        {
            char c = buffer[i];
            if ("aeiou".Contains(c, StringComparison.OrdinalIgnoreCase))
                vowelCount++;
        }
        
        Console.WriteLine($"Vowels: {vowelCount}");
    }
    finally
    {
        // CRITICAL: Return the array to the pool so it can be reused.
        // Failure to do so causes a memory leak (the pool thinks the array is still in use).
        ArrayPool<char>.Shared.Return(buffer);
    }
}
