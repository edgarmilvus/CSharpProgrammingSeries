
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;

public class ZeroAllocTokenizer
{
    public static IEnumerable<ReadOnlySpan<char>> Tokenize(ReadOnlySpan<char> text)
    {
        int start = 0;
        bool inToken = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // Check if the character is part of a word
            if (char.IsLetterOrDigit(c))
            {
                if (!inToken)
                {
                    start = i;
                    inToken = true;
                }
            }
            else
            {
                if (inToken)
                {
                    // Yield the slice. This creates a view into the original memory,
                    // not a new string on the heap.
                    yield return text[start..i];
                    inToken = false;
                }
            }
        }

        // Handle token at end of text
        if (inToken)
        {
            yield return text[start..];
        }
    }

    public static void Run()
    {
        string rawText = "Hello, World! AI is evolving rapidly. 2024.";
        
        Console.WriteLine("Original Text: " + rawText);
        Console.WriteLine("Tokens (Span Slices):");

        foreach (var token in Tokenize(rawText))
        {
            // Note: printing a Span requires conversion to string for display,
            // but processing would happen directly on the Span.
            Console.WriteLine($"  '{token.ToString()}' (Length: {token.Length})");
        }
    }
}
