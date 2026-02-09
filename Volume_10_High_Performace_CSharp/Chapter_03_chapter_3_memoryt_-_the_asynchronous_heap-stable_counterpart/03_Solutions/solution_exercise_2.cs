
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Tokenizer
{
    public static IEnumerable<ReadOnlyMemory<char>> TokenizeSentence(ReadOnlyMemory<char> sentence)
    {
        ReadOnlySpan<char> span = sentence.Span;
        int start = 0;

        for (int i = 0; i < span.Length; i++)
        {
            if (char.IsWhiteSpace(span[i]))
            {
                if (i > start)
                {
                    // Yield a slice of the original memory
                    yield return sentence.Slice(start, i - start);
                }
                start = i + 1;
            }
        }

        // Yield the last word if the sentence doesn't end with whitespace
        if (start < span.Length)
        {
            yield return sentence.Slice(start, span.Length - start);
        }
    }

    // Debug helper to verify memory addresses
    public static unsafe void PrintMemoryDetails(ReadOnlyMemory<char> memory)
    {
        // Using MemoryMarshal to get the underlying array for demonstration
        // In a real scenario, this might be a native pointer.
        if (MemoryMarshal.TryGetArray(memory, out ArraySegment<char> segment))
        {
            fixed (char* ptr = segment.Array)
            {
                Console.WriteLine($"  Address: {(IntPtr)ptr:X} (Offset: {segment.Offset})");
            }
        }
    }
}

public class Consumer
{
    public void Process(string input)
    {
        Console.WriteLine($"Input String: '{input}'");
        ReadOnlyMemory<char> memory = input.AsMemory();

        foreach (var token in Tokenizer.TokenizeSentence(memory))
        {
            // Print the token string (for readability) and memory details (for verification)
            Console.Write($"Token: '{token}' ");
            Tokenizer.PrintMemoryDetails(token);
        }
    }
}
