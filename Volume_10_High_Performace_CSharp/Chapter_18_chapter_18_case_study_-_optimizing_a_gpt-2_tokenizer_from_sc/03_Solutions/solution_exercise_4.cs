
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Gpt2Tokenizer
{
    // Mock vocabulary for demonstration
    private static readonly Dictionary<string, int> _vocab = new Dictionary<string, int>
    {
        { "hello", 1 }, { " world", 2 }, { "!", 3 }
    };

    /// <summary>
    /// Refactored Encode method using Spans and ArrayPools.
    /// </summary>
    public int[] Encode(string text)
    {
        // 1. Pre-allocate a buffer for tokens to avoid List<int> resizing.
        // We rent this from the pool. We estimate an upper bound (e.g., 1 token per char).
        int estimatedTokenCount = text.Length;
        int[] rentedTokenBuffer = ArrayPool<int>.Shared.Rent(estimatedTokenCount);
        
        int tokenCount = 0;

        try
        {
            // 2. Convert string to ReadOnlySpan<char>.
            // This avoids allocating a new char[] or string.
            ReadOnlySpan<char> textSpan = text.AsSpan();

            // 3. Iterative BPE simulation (simplified for this exercise).
            // In a real GPT-2 tokenizer, this involves a greedy merge loop.
            // Here, we simulate splitting by whitespace and looking up in vocab.
            int start = 0;
            for (int i = 0; i <= textSpan.Length; i++)
            {
                // Split on whitespace or end of string
                if (i == textSpan.Length || char.IsWhiteSpace(textSpan[i]))
                {
                    if (i > start)
                    {
                        ReadOnlySpan<char> tokenStr = textSpan.Slice(start, i - start);
                        
                        // We need to look up this string in the dictionary.
                        // Dictionaries require string keys. To avoid allocation here,
                        // in a real scenario, we would use a Trie or a perfect hash.
                        // For this exercise, we allocate ONLY the key for the lookup.
                        string key = tokenStr.ToString(); 
                        if (_vocab.TryGetValue(key, out int tokenId))
                        {
                            rentedTokenBuffer[tokenCount++] = tokenId;
                        }
                    }
                    start = i + 1;
                }
            }

            // 4. Create the final result array.
            // We know exactly how many tokens we have.
            int[] result = new int[tokenCount];
            Span<int> resultSpan = result.AsSpan();
            Span<int> sourceSpan = rentedTokenBuffer.AsSpan(0, tokenCount);
            sourceSpan.CopyTo(resultSpan);

            return result;
        }
        finally
        {
            // 5. Return the rented buffer to the pool.
            ArrayPool<int>.Shared.Return(rentedTokenBuffer);
        }
    }
}
