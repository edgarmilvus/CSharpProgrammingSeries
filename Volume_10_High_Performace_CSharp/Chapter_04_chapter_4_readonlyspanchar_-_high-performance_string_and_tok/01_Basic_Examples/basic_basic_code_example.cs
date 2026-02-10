
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;

public class SpanTokenizer
{
    public static void Main()
    {
        // 1. The Input: A raw string representing user input to an AI model.
        //    In a real scenario, this could be megabytes of text.
        string userInput = "The quick brown fox, jumps over the lazy dog!";

        Console.WriteLine($"Original Input: \"{userInput}\"");
        Console.WriteLine($"Input Length: {userInput.Length} characters");
        Console.WriteLine(new string('-', 40));

        // 2. Create a ReadOnlySpan<char> view of the entire string.
        //    This is a "zero-allocation" slice. It doesn't copy the string data.
        //    It simply points to the existing memory location of the original string.
        ReadOnlySpan<char> remainingText = userInput.AsSpan();

        // 3. Prepare a list to hold our results.
        //    Note: We are NOT storing strings here yet. We will store spans first.
        var tokensAsSpans = new List<ReadOnlySpan<char>>();

        // 4. The Tokenization Loop
        //    We will process the text chunk by chunk, identifying words separated by punctuation or spaces.
        while (!remainingText.IsEmpty)
        {
            // 4a. Trim leading whitespace and punctuation.
            //     Span<T>.TrimStart is highly optimized and allocates no memory.
            remainingText = remainingText.TrimStart(" ,.!?;:");

            if (remainingText.IsEmpty)
            {
                break; // No more content to process.
            }

            // 4b. Find the end of the current word.
            //     We search for the next delimiter (space or punctuation).
            //     IndexOfAny is optimized using SIMD under the hood in modern .NET runtimes.
            int delimiterIndex = remainingText.IndexOfAny(" ,.!?;:");

            ReadOnlySpan<char> token;

            if (delimiterIndex == -1)
            {
                // 4c. If no delimiter is found, the rest of the span is the last word.
                token = remainingText;
                remainingText = ReadOnlySpan<char>.Empty; // Mark as finished.
            }
            else
            {
                // 4d. Slice the span from the start to the delimiter.
                //     This creates a NEW span (a lightweight struct), but NO heap allocation.
                token = remainingText.Slice(0, delimiterIndex);

                // 4e. Advance the view of the remaining text.
                //     We slice from the delimiter + 1 to skip over the delimiter itself.
                remainingText = remainingText.Slice(delimiterIndex + 1);
            }

            // 5. Store the token.
            //    We are adding a struct (ReadOnlySpan<char>) to the list.
            //    The list itself allocates memory for the struct wrappers, but the actual
            //    character data remains in the original string's memory.
            tokensAsSpans.Add(token);
        }

        // 6. Output the results.
        //    We convert the spans back to strings ONLY for display purposes.
        //    In a real processing pipeline, you might pass the spans directly to the next stage.
        Console.WriteLine("Extracted Tokens (via ReadOnlySpan<char>):");
        foreach (var tokenSpan in tokensAsSpans)
        {
            // .ToString() allocates a new string on the heap.
            // This is necessary for printing, but in the processing logic above, we avoided it.
            Console.WriteLine($" - '{tokenSpan.ToString()}' (Length: {tokenSpan.Length})");
        }
    }
}
