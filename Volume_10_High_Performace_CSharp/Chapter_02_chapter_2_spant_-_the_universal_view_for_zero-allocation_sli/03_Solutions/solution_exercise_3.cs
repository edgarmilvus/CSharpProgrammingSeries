
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
using System.Collections.Generic;
using System.Linq;

public static class PromptFilter
{
    public static string MaskTokens(string input, params string[] forbiddenTokens)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // 1. Rent Array
        char[] buffer = ArrayPool<char>.Shared.Rent(input.Length);
        
        try
        {
            // 2. Copy to Span
            input.AsSpan().CopyTo(buffer);

            // We need to operate on the valid part of the buffer
            Span<char> workSpan = buffer.AsSpan(0, input.Length);

            // 3. Search and Mask
            foreach (var token in forbiddenTokens)
            {
                if (string.IsNullOrEmpty(token)) continue;
                
                // Recursive mask for the same token (in case of overlapping or multiple occurrences)
                // Note: Span.IndexOf finds the first occurrence.
                int index;
                while ((index = workSpan.IndexOf(token.AsSpan())) != -1)
                {
                    // Mask the characters
                    for (int i = 0; i < token.Length; i++)
                    {
                        workSpan[index + i] = '*';
                    }
                }
            }

            // 4. Return new string (this is the only allocation allowed)
            return new string(workSpan);
        }
        finally
        {
            // 5. Return to pool
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    // Interactive Challenge: Multiple tokens efficiently
    public static string MaskTokensAdvanced(string input, params string[] forbiddenTokens)
    {
        if (string.IsNullOrEmpty(input)) return input;

        char[] buffer = ArrayPool<char>.Shared.Rent(input.Length);
        
        try
        {
            input.AsSpan().CopyTo(buffer);
            Span<char> workSpan = buffer.AsSpan(0, input.Length);

            // Optimization: Instead of scanning for each token sequentially (O(N*M)),
            // we can scan once and check if the current substring matches any token.
            // However, checking against a list of strings at every index is expensive.
            // A better approach for "mask any character belonging to any token" is creating a lookup set.
            
            var forbiddenSet = new HashSet<char>();
            foreach (var token in forbiddenTokens)
            {
                foreach (char c in token) forbiddenSet.Add(c);
            }

            // Now we scan the buffer once.
            // If we find a character in the set, we need to check if it starts a forbidden token.
            // To do this efficiently without regex, we iterate the buffer and check all tokens at that position.
            
            for (int i = 0; i < workSpan.Length; i++)
            {
                foreach (var token in forbiddenTokens)
                {
                    if (i + token.Length > workSpan.Length) continue;

                    // Compare the slice at i with the token
                    if (workSpan.Slice(i, token.Length).SequenceEqual(token))
                    {
                        // Mask it
                        for (int k = 0; k < token.Length; k++)
                        {
                            workSpan[i + k] = '*';
                        }
                        // Advance i to skip masked characters to avoid re-checking parts of a masked token
                        // (though masking with '*' usually prevents matches, it's safer to skip)
                        i += token.Length - 1; 
                        break; 
                    }
                }
            }

            return new string(workSpan);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
