
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Text.RegularExpressions;

public class AdvancedRegexTokenizer
{
    // Regex option to compile for speed if used frequently
    private static readonly Regex WordRegex = new Regex(@"\b\w+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void TokenizeZeroAlloc(ReadOnlySpan<char> text, Action<ReadOnlySpan<char>> processToken)
    {
        // EnumerateMatches is available in .NET 7+ and is highly optimized.
        // It returns ValueMatch structs, avoiding heap allocation of Match objects.
        foreach (ValueMatch match in WordRegex.EnumerateMatches(text))
        {
            // We have the index and length. 
            // We can slice the original text directly.
            ReadOnlySpan<char> token = text.Slice(match.Index, match.Length);

            // Process the token. 
            // Since we can't return it via yield (due to stack lifetime of stackalloc),
            // we pass it to a delegate for immediate processing.
            processToken(token);
        }
    }

    // Alternative: Using stackalloc for temporary manipulation
    public static void NormalizeAndProcess(ReadOnlySpan<char> text)
    {
        foreach (ValueMatch match in WordRegex.EnumerateMatches(text))
        {
            ReadOnlySpan<char> token = text.Slice(match.Index, match.Length);

            // Example: Copy to stack buffer to modify (e.g., to lowercase)
            // We use stackalloc to avoid heap allocation for the temporary buffer.
            // This memory lives only as long as the current iteration of the loop.
            Span<char> buffer = stackalloc char[token.Length];
            token.CopyTo(buffer);

            // Perform modification (e.g., ToLower simulation)
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] >= 'A' && buffer[i] <= 'Z')
                {
                    buffer[i] = (char)(buffer[i] + 32);
                }
            }

            // Now 'buffer' contains the normalized token.
            // We can hash it or compare it without ever creating a string.
            Console.WriteLine($"Processed (Stack): {buffer.ToString()}");
        }
    }

    public static void Run()
    {
        string text = "AI is reshaping, 2024 is the year!";
        
        Console.WriteLine("Method 1: Zero-Allocation Slicing via Callback");
        TokenizeZeroAlloc(text, token => 
        {
            Console.WriteLine($"Token: {token.ToString()}");
        });

        Console.WriteLine("\nMethod 2: Stackalloc Normalization");
        NormalizeAndProcess(text);
    }
}
