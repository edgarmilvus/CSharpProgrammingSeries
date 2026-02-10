
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

public class CsvTokenizer
{
    // Returns a Memory<char> slice and the actual count of tokens found
    public static (Memory<char> buffer, int tokenCount) TokenizeCsvLine(
        string line, char separator, ArrayPool<char> pool)
    {
        // 1. Rent a buffer from the shared pool.
        // We rent slightly more than needed to be safe.
        char[] rentedArray = pool.Rent(line.Length);

        try
        {
            // 2. Copy string data to the rented array using Span.
            // This avoids creating a char[] copy of the string manually.
            line.AsSpan().CopyTo(rentedArray);

            // 3. Parse manually to count tokens (simulating tokenization logic).
            // In a real scenario, we might track start/end indices of tokens.
            int tokenCount = 0;
            bool inToken = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (rentedArray[i] == separator)
                {
                    inToken = false;
                }
                else if (!inToken)
                {
                    tokenCount++;
                    inToken = true;
                }
            }

            // Return the Memory slice (a view into the rented array)
            // and the count of tokens identified.
            return (new Memory<char>(rentedArray, 0, line.Length), tokenCount);
        }
        catch
        {
            // If an error occurs during processing, return the array immediately.
            pool.Return(rentedArray);
            throw;
        }
    }

    public static void Main()
    {
        string csvLine = "id_123,John Doe,Engineer,42";
        var pool = ArrayPool<char>.Shared;

        // Rent and Parse
        var (memorySlice, count) = TokenizeCsvLine(csvLine, ',', pool);

        try
        {
            // Access the data via Span (read-only view)
            ReadOnlySpan<char> span = memorySlice.Span;
            Console.WriteLine($"Parsed {count} tokens. Buffer content: {span.ToString()}");

            // In a real AI pipeline, 'memorySlice' might be passed to a 
            // UTF-8 encoder or tokenizer without ever allocating a string.
        }
        finally
        {
            // CRITICAL: Return the array to the pool.
            // We must retrieve the underlying array from the Memory object.
            // Note: memorySlice.ToArray() is used here for demonstration safety,
            // but in strict high-perf code, we would keep a reference to 'rentedArray'
            // in the calling scope to avoid the ToArray() copy.
            pool.Return(memorySlice.ToArray());
        }
    }
}
