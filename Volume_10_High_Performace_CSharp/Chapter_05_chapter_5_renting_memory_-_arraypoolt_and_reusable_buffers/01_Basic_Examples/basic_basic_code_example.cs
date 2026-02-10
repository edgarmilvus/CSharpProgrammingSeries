
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
using System.Buffers;
using System.Text;

public class TokenProcessor
{
    // Simulate a high-volume stream of tokens (e.g., from a tokenizer)
    private static readonly string[] _inputTokens = new[]
    {
        "Hello", "World", "AI", "Inference", "Optimization",
        "Memory", "Pooling", "C#", "Performance", "Token"
    };

    public static void Main()
    {
        Console.WriteLine("Starting Token Processing with ArrayPool...\n");

        // We expect tokens to be small, but let's rent a generous buffer 
        // to handle multiple tokens or larger ones without resizing.
        int bufferSize = 1024; 

        // 1. RENT: Get a shared array from the pool.
        // WARNING: The array is not zeroed out by default (it contains dirty data).
        char[] buffer = ArrayPool<char>.Shared.Rent(bufferSize);

        try
        {
            // Simulate processing a stream of tokens
            foreach (var token in _inputTokens)
            {
                ProcessToken(token, buffer);
            }
        }
        finally
        {
            // 3. RETURN: Crucial! Return the array to the pool so it can be reused.
            // If we forget this, the memory leaks from the pool perspective 
            // (though the GC will eventually clean it up if the pool is abandoned).
            ArrayPool<char>.Shared.Return(buffer);
            Console.WriteLine("\nBuffer returned to pool successfully.");
        }
    }

    private static void ProcessToken(string token, char[] buffer)
    {
        // 2. CLEAR/RESET: Because the buffer might contain data from a previous rental,
        // we must ensure we don't read stale bytes. 
        // For this simple string copy, we overwrite the necessary range.
        
        // Convert string to char array directly into the rented buffer
        // Note: In a real scenario, you might use Utf8Formatter or direct byte operations.
        token.AsSpan().CopyTo(buffer);

        // Simulate some work (e.g., encoding or hashing)
        // We only use the portion of the buffer actually containing data.
        Console.WriteLine($"Processing Token: '{token}' (Buffer ID: {buffer.GetHashCode()})");

        // Example of using the buffer content
        ReadOnlySpan<char> validData = buffer.AsSpan(0, token.Length);
        // ... perform operations on validData ...
    }
}
