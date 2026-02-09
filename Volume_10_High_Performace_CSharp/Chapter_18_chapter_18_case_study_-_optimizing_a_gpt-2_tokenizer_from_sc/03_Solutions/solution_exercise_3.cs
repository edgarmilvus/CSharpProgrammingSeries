
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

public class StreamingTokenizer
{
    /// <summary>
    /// Reads a stream in chunks, tokenizes them, and yields token IDs.
    /// </summary>
    public static IEnumerable<int> TokenizeStream(Stream stream, int bufferSize = 4096)
    {
        // Rent a buffer from the shared pool to avoid heap allocations.
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            int bytesRead;
            // Read asynchronously or synchronously. Loop until stream ends.
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Create a Memory<T> view of the valid data in the buffer.
                // This is safer than Span<T> if we were to pass this to async methods,
                // but for synchronous processing, it provides a convenient wrapper.
                Memory<byte> validMemory = buffer.AsMemory(0, bytesRead);

                // --- MOCK TOKENIZATION LOGIC ---
                // In a real scenario, this would call a UTF-8 decoder or BPE merger.
                // Here, we simply yield the byte values as tokens for demonstration.
                for (int i = 0; i < bytesRead; i++)
                {
                    yield return validMemory.Span[i];
                }
            }
        }
        finally
        {
            // CRITICAL: Return the buffer to the pool regardless of exceptions.
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
