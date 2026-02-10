
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
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
*/

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Buffers; // For ArrayPool
using System.Collections.Generic;
using System.Linq;

public class TokenizerComparison
{
    // OLD WAY: Heap Allocations
    // Returns a List<string>, meaning every token is a new object on the Heap.
    public static List<string> TokenizeNaive(string text)
    {
        // Splits create new string objects immediately.
        return text.Split(new[] { ' ', '.', ',', '!' }, StringSplitOptions.RemoveEmptyEntries)
                   .ToList();
    }

    // NEW WAY: Zero-Allocation
    // Returns a List<ReadOnlyMemory<char>>. 
    // The text data remains in the original buffer; we just store pointers and lengths.
    public static List<ReadOnlyMemory<char>> TokenizeWithSpan(ReadOnlySpan<char> text)
    {
        var tokens = new List<ReadOnlyMemory<char>>();
        
        // We iterate manually to avoid LINQ (which causes allocations on Spans in older frameworks 
        // or requires specific overloads) and to control memory precisely.
        int start = -1;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            // Define delimiters (simplified for this example)
            if (c == ' ' || c == '.' || c == ',' || c == '!')
            {
                if (start != -1)
                {
                    // SLICE: This is a zero-copy operation.
                    // It creates a new Span pointing to a segment of the original text.
                    ReadOnlySpan<char> tokenSpan = text.Slice(start, i - start);
                    
                    // IMPORTANT: We cannot store Span<T> in a List (it's a ref struct).
                    // We must convert to ReadOnlyMemory<T> (which is heap-allocated but lightweight)
                    // or process it immediately.
                    tokens.Add(tokenSpan.ToArray().AsMemory()); 
                    start = -1;
                }
            }
            else
            {
                if (start == -1) start = i;
            }
        }

        // Handle the last token
        if (start != -1)
        {
            tokens.Add(text.Slice(start).ToArray().AsMemory());
        }

        return tokens;
    }
}
