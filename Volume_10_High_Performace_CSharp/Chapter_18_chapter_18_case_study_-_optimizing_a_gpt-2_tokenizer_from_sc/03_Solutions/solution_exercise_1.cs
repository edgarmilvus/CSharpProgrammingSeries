
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
using System.Buffers;
using System.Text;

public class Utf8Processor
{
    /// <summary>
    /// Processes a UTF-8 byte span by decoding it into Runes without allocating strings.
    /// </summary>
    /// <param name="utf8Bytes">The source byte span containing valid UTF-8 data.</param>
    /// <param name="processRune">The callback to execute for each decoded Rune.</param>
    public static void ProcessUtf8Tokens(ReadOnlySpan<byte> utf8Bytes, Action<Rune> processRune)
    {
        // Use the high-performance System.Text.Encoding.UTF8 which operates on spans.
        // We iterate manually to avoid LINQ or string allocations.
        int index = 0;
        while (index < utf8Bytes.Length)
        {
            // Attempt to decode a single UTF-8 character starting at the current index.
            // Rune.DecodeFromUtf8 returns:
            // 1. The status (Success, NeedMoreData, InvalidData)
            // 2. The decoded Rune
            // 3. The number of bytes consumed (usually 1-4)
            OperationStatus status = Rune.DecodeFromUtf8(
                utf8Bytes.Slice(index), 
                out Rune rune, 
                out int bytesConsumed);

            if (status == OperationStatus.Done)
            {
                // Successfully decoded a rune. Process it.
                processRune(rune);
                index += bytesConsumed;
            }
            else if (status == OperationStatus.NeedMoreData)
            {
                // This indicates the span ended with an incomplete sequence.
                // Since we assume valid UTF-8 for this exercise, we break.
                break;
            }
            else // OperationStatus.InvalidData
            {
                // Invalid UTF-8 sequence encountered.
                // In a production tokenizer, we might handle this by replacing with a 
                // specific token (e.g.,  or  ). 
                // For this exercise, we skip the invalid byte to continue processing.
                index += 1;
            }
        }
    }
}
