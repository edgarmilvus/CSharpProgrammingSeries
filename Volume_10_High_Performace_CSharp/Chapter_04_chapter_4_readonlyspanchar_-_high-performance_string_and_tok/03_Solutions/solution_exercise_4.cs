
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class OptimizedAttentionMaskGenerator
{
    // Struct to encapsulate the scanner logic
    public ref struct StopTokenScanner
    {
        private readonly ReadOnlySpan<char> _text;
        private readonly List<ReadOnlySpan<char>> _stopTokens;
        private readonly SearchValues<char> _firstChars;

        public StopTokenScanner(ReadOnlySpan<char> text, List<ReadOnlySpan<char>> stopTokens)
        {
            _text = text;
            _stopTokens = stopTokens;

            // Optimization: Extract the first character of every stop token
            // and create a SearchValues set for fast vectorized lookup.
            char[] uniqueFirstChars = new char[stopTokens.Count];
            for (int i = 0; i < stopTokens.Count; i++)
            {
                if (!stopTokens[i].IsEmpty)
                {
                    uniqueFirstChars[i] = stopTokens[i][0];
                }
            }
            _firstChars = SearchValues.Create(uniqueFirstChars);
        }

        public bool TryFindNextStopToken(int startIndex, out int foundIndex, out int tokenLength)
        {
            // Search for any occurrence of the *start characters* of stop tokens
            int relativeIndex = _text.Slice(startIndex).IndexOfAny(_firstChars);

            if (relativeIndex == -1)
            {
                foundIndex = -1;
                tokenLength = 0;
                return false;
            }

            int absoluteIndex = startIndex + relativeIndex;

            // Verify full tokens
            foreach (var token in _stopTokens)
            {
                // Boundary check
                if (absoluteIndex + token.Length > _text.Length) continue;

                // Compare the potential match with the stop token
                if (_text.Slice(absoluteIndex, token.Length).SequenceEqual(token))
                {
                    foundIndex = absoluteIndex;
                    tokenLength = token.Length;
                    return true;
                }
            }

            // If we found a start char but it didn't match any full token, 
            // we must continue searching from the next character.
            // Recursive call to continue search (or use a loop in the caller)
            return TryFindNextStopToken(absoluteIndex + 1, out foundIndex, out tokenLength);
        }
    }

    public static void GenerateMask(ReadOnlySpan<char> prompt, List<ReadOnlySpan<char>> stopTokens)
    {
        // Example usage of the optimized scanner
        var scanner = new StopTokenScanner(prompt, stopTokens);
        int currentPos = 0;

        Console.WriteLine($"Scanning prompt of length {prompt.Length}...");

        // In a real scenario, you would build a mask array here.
        // We simulate the loop to demonstrate the optimization.
        while (scanner.TryFindNextStopToken(currentPos, out int foundIndex, out int length))
        {
            // Found a stop token at 'foundIndex'
            Console.WriteLine($"Stop token found at index: {foundIndex}");
            
            // Advance position to after the found token
            currentPos = foundIndex + length;
        }
    }
}
