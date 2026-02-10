
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Text;

public static class SpanTokenizer
{
    // Cache the SearchValues instance to avoid rebuilding the lookup table on every call.
    // This covers ASCII punctuation, symbols, and control characters.
    private static readonly SearchValues<char> Separators = 
        SearchValues.Create("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~\t\n\r ");

    public static IEnumerable<ReadOnlySpan<char>> GetTokens(ReadOnlySpan<char> text)
    {
        int start = 0;
        int length = text.Length;

        while (start < length)
        {
            // 1. Find the next separator
            int separatorIndex = text.Slice(start).IndexOfAny(Separators);

            if (separatorIndex == -1)
            {
                // No more separators, yield the rest of the text if not empty
                ReadOnlySpan<char> remaining = text.Slice(start);
                if (!remaining.IsEmpty)
                {
                    yield return remaining;
                }
                yield break;
            }

            // 2. If separator is found immediately at the start, skip it
            if (separatorIndex == 0)
            {
                start++;
                continue;
            }

            // 3. Yield the token (the part before the separator)
            yield return text.Slice(start, separatorIndex);

            // 4. Move start past the separator
            start += separatorIndex + 1;
        }
    }
}
