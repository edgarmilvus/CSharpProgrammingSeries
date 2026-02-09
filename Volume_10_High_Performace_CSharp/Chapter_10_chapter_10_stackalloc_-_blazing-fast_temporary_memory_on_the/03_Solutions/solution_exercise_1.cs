
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
using System.Buffers;
using System.Runtime.CompilerServices;

public static class StackAllocTokenizer
{
    // Define the maximum token length constant as requested.
    private const int MaxTokenLength = 64;

    public static void TokenizeStackAlloc(ReadOnlySpan<byte> utf8Input, Action<ReadOnlySpan<byte>> tokenHandler)
    {
        // We allocate the buffer once on the stack. 
        // Note: This is safe because MaxTokenLength (64) is small.
        // If this were larger, it could risk a StackOverflowException.
        Span<byte> tokenBuffer = stackalloc byte[MaxTokenLength];
        int tokenIndex = 0;

        // Iterate over the input span
        foreach (byte b in utf8Input)
        {
            // Check for whitespace (Space, Tab, Newline, Carriage Return)
            if (b == (byte)' ' || b == (byte)'\t' || b == (byte)'\n' || b == (byte)'\r')
            {
                // If we have accumulated a token, process it
                if (tokenIndex > 0)
                {
                    // Yield the valid portion of the buffer
                    tokenHandler(tokenBuffer.Slice(0, tokenIndex));
                    tokenIndex = 0; // Reset for the next token
                }
                continue; // Skip the whitespace
            }

            // Accumulate non-whitespace bytes
            if (tokenIndex < MaxTokenLength)
            {
                tokenBuffer[tokenIndex++] = b;
            }
            else
            {
                // Handle buffer overflow: Truncate logic as requested.
                // In a real scenario, we might want to flush the current buffer 
                // and reset, or throw an error. Here we effectively ignore 
                // characters exceeding the MaxTokenLength for the current token.
            }
        }

        // Handle the final token if the input didn't end with whitespace
        if (tokenIndex > 0)
        {
            tokenHandler(tokenBuffer.Slice(0, tokenIndex));
        }
    }
}
