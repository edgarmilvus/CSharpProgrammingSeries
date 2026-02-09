
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
using System.Text;

public class SpanTokenProcessor
{
    public static void Main()
    {
        // 1. Real-world context: Processing a large log stream or text buffer.
        // We want to extract tokens (words) without allocating new string objects on the heap.
        string logEntry = "ERROR:2023-10-27:System.OutOfMemoryException: Allocation failed.";
        
        Console.WriteLine($"Original String: {logEntry}");
        Console.WriteLine("--- Processing with Span<T> ---");

        // 2. Convert the immutable string to a mutable character buffer.
        // In a real high-performance scenario, this might come from a network stream or file I/O.
        // We use 'stackalloc' to allocate memory on the stack (zero GC pressure).
        // Note: The size 256 is arbitrary; in production, you might use ArrayPool.
        Span<char> buffer = stackalloc char[256];
        logEntry.AsSpan().CopyTo(buffer);

        // 3. Define the delimiter for tokenization.
        char delimiter = ':';

        // 4. Iterate over the buffer using Span<T> to find and process tokens.
        // 'Split' is a modern C# method that works on Span<T> and returns a SpanRange.
        // This avoids creating an array of strings.
        foreach (Range tokenRange in buffer.Split(delimiter))
        {
            // 5. Slice the buffer to get the specific token.
            // This operation is O(1) and allocates zero memory on the heap.
            Span<char> token = buffer[tokenRange];

            // 6. Trim whitespace (common in log processing).
            // Span<T> allows us to manipulate the view without copying data.
            token = Trim(token);

            // 7. Convert the Span<char> to a string ONLY if necessary for output.
            // This is the only allocation in this loop.
            // In a pure calculation pipeline, we might avoid this entirely.
            string tokenStr = new string(token);
            Console.WriteLine($"Token: {tokenStr}");
        }
    }

    // Helper method to trim whitespace from a Span<char>.
    // This is a zero-allocation implementation of string.Trim().
    public static Span<char> Trim(Span<char> span)
    {
        int start = 0;
        int end = span.Length - 1;

        // Find first non-whitespace character
        while (start <= end && char.IsWhiteSpace(span[start]))
        {
            start++;
        }

        // Find last non-whitespace character
        while (end >= start && char.IsWhiteSpace(span[end]))
        {
            end--;
        }

        // Return the sliced view
        return span.Slice(start, end - start + 1);
    }
}
