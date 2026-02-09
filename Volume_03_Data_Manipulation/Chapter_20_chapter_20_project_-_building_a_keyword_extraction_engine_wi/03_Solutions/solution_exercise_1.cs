
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;

public class ZeroAllocationTokenizer
{
    /// <summary>
    /// Tokenizes text into spans without allocating memory on the heap.
    /// </summary>
    /// <param name="source">The input text as a ReadOnlySpan.</param>
    /// <param name="output">A pre-allocated Span to store the resulting token ranges.</param>
    /// <returns>The number of tokens found.</returns>
    public static int TokenizeToSpans(ReadOnlySpan<char> source, Span<Range> output)
    {
        int tokenCount = 0;
        int i = 0;
        int length = source.Length;

        while (i < length)
        {
            // Skip whitespace characters
            while (i < length && char.IsWhiteSpace(source[i]))
            {
                i++;
            }

            if (i >= length) break;

            // Mark the start of the token
            int start = i;

            // Find the end of the token (until whitespace or end)
            while (i < length && !char.IsWhiteSpace(source[i]))
            {
                i++;
            }

            // Store the range in the output span
            if (tokenCount < output.Length)
            {
                output[tokenCount++] = new Range(start, i);
            }
            else
            {
                // Output buffer is full; stop processing to prevent overflow
                break;
            }
        }

        return tokenCount;
    }

    // Helper method to demonstrate usage
    public static void ProcessText(string input)
    {
        // Convert string to ReadOnlySpan (Zero allocation)
        ReadOnlySpan<char> textSpan = input.AsSpan();

        // Allocate memory on the stack (Fast, auto-cleaned, zero GC pressure)
        // We limit the size to prevent stack overflow (e.g., max 1024 tokens)
        Span<Range> tokenRanges = stackalloc Range[1024];

        int count = TokenizeToSpans(textSpan, tokenRanges);

        // Process the tokens
        for (int i = 0; i < count; i++)
        {
            // Slicing a Span creates another Span (Zero allocation)
            ReadOnlySpan<char> token = textSpan[tokenRanges[i]];
            
            // Note: Console.WriteLine requires a string, which forces an allocation here
            // for display purposes, but the processing logic remains allocation-free.
            Console.WriteLine(token.ToString());
        }
    }
}
