
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;

public static class CsvParser
{
    public static ReadOnlySpan<char>[] ParseCsvLine(ReadOnlySpan<char> line, char delimiter = ',')
    {
        // 1. Handle empty input immediately
        if (line.IsEmpty)
        {
            return Array.Empty<ReadOnlySpan<char>>();
        }

        // 2. First pass: Count delimiters to determine array size
        // We add 1 because N delimiters create N+1 fields.
        int fieldCount = 1;
        foreach (char c in line)
        {
            if (c == delimiter) fieldCount++;
        }

        var result = new ReadOnlySpan<char>[fieldCount];
        int resultIndex = 0;
        int start = 0;
        int i = 0;
        int length = line.Length;

        // 3. Second pass: Extract spans
        while (i < length)
        {
            if (line[i] == delimiter)
            {
                // Extract field from 'start' to 'i' (exclusive)
                ReadOnlySpan<char> field = line.Slice(start, i - start);
                result[resultIndex++] = Trim(field);

                // Move start to character after delimiter
                start = i + 1;
            }
            i++;
        }

        // 4. Handle the last field (or the only field if no delimiter found)
        // Slice from 'start' to the end of the line
        ReadOnlySpan<char> lastField = line.Slice(start);
        result[resultIndex] = Trim(lastField);

        return result;
    }

    private static ReadOnlySpan<char> Trim(ReadOnlySpan<char> span)
    {
        // Efficient trimming using span APIs (no allocations)
        int start = 0;
        int end = span.Length - 1;

        // Trim leading
        while (start <= end && char.IsWhiteSpace(span[start]))
        {
            start++;
        }

        // Trim trailing
        while (end >= start && char.IsWhiteSpace(span[end]))
        {
            end--;
        }

        // Return empty span if only whitespace was present
        if (start > end)
        {
            return ReadOnlySpan<char>.Empty;
        }

        return span.Slice(start, end - start + 1);
    }
}
