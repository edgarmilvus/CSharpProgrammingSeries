
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
using System.Buffers;
using System.Collections.Generic;

public class NGramGenerator
{
    public static IEnumerable<string> GenerateBiGrams(Span<Range> tokens, ReadOnlySpan<char> sourceText)
    {
        if (tokens.Length < 2) yield break;

        // Rent a buffer from the shared pool (Thread-safe, fast)
        // We estimate a reasonable size; ArrayPool handles resizing if needed.
        char[] buffer = ArrayPool<char>.Shared.Rent(256);

        try
        {
            for (int i = 0; i < tokens.Length - 1; i++)
            {
                Range r1 = tokens[i];
                Range r2 = tokens[i + 1];

                ReadOnlySpan<char> t1 = sourceText[r1];
                ReadOnlySpan<char> t2 = sourceText[r2];

                int totalLength = t1.Length + 1 + t2.Length; // Token1 + Space + Token2

                // Ensure buffer capacity
                if (totalLength > buffer.Length)
                {
                    // Return old buffer and rent a larger one
                    ArrayPool<char>.Shared.Return(buffer);
                    buffer = ArrayPool<char>.Shared.Rent(totalLength);
                }

                // Copy data into the rented buffer
                t1.CopyTo(buffer);
                buffer[t1.Length] = ' ';
                t2.CopyTo(buffer.AsSpan(t1.Length + 1));

                // Create the string. 
                // IMPORTANT: The string constructor copies the data from the buffer 
                // to the heap, so we can safely return the buffer to the pool immediately.
                string biGram = new string(buffer, 0, totalLength);
                yield return biGram;
            }
        }
        finally
        {
            // Crucial: Return the buffer to the pool to be reused.
            // If we skip this, the memory is leaked from the pool perspective.
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
