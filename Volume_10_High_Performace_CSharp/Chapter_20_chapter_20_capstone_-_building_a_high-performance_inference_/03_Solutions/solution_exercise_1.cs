
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

public class TokenStreamProcessor
{
    /// <summary>
    /// Parses a raw byte sequence into token IDs, writing directly to the output buffer.
    /// Simulates tokenization by splitting on a comma delimiter (',').
    /// </summary>
    /// <param name="rawInput">The raw byte data to tokenize.</param>
    /// <param name="outputBuffer">The pre-allocated buffer to write token IDs to.</param>
    /// <returns>The number of tokens written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if outputBuffer is too small.</exception>
    public int ProcessTokens(ReadOnlySpan<byte> rawInput, Span<int> outputBuffer)
    {
        // Convert bytes to string for simulation (in a real scenario, this would be a custom byte parser).
        // Note: This allocation is for simulation only; the core logic uses Spans.
        string inputString = Encoding.UTF8.GetString(rawInput);
        string[] parts = inputString.Split(',');

        if (parts.Length > outputBuffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(outputBuffer), "Output buffer is too small to hold all tokens.");
        }

        // Parse parts into integers and write to outputBuffer.
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out int tokenId))
            {
                // Handle parsing error (e.g., invalid token format)
                throw new FormatException($"Invalid token format at index {i}.");
            }
            outputBuffer[i] = tokenId;
        }

        return parts.Length;
    }

    /// <summary>
    /// Example method demonstrating the use of ArrayPool for intermediate processing.
    /// </summary>
    public int ProcessTokensWithPool(ReadOnlySpan<byte> rawInput, Span<int> outputBuffer)
    {
        // Rent a temporary buffer from the shared pool.
        // In a real scenario, this might be used for complex preprocessing before final output.
        int[] tempBuffer = ArrayPool<int>.Shared.Rent(1024); // Assume max tokens is 1024 for this example

        try
        {
            // Reuse the logic from ProcessTokens but target the temp buffer first.
            int tokenCount = ProcessTokens(rawInput, tempBuffer.AsSpan(0, Math.Min(tempBuffer.Length, outputBuffer.Length)));

            if (tokenCount > outputBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(outputBuffer), "Output buffer is too small.");
            }

            // Copy from temp buffer to final output buffer.
            tempBuffer.AsSpan(0, tokenCount).CopyTo(outputBuffer);
            return tokenCount;
        }
        finally
        {
            // CRITICAL: Always return the rented array to the pool.
            ArrayPool<int>.Shared.Return(tempBuffer);
        }
    }
}

// Unit Test (using xUnit style for demonstration)
public static class TokenizerTests
{
    public static void RunTest()
    {
        var processor = new TokenStreamProcessor();
        
        // Test with stackalloc buffer (zero heap allocation)
        Span<int> stackBuffer = stackalloc int[10];
        byte[] inputBytes = Encoding.UTF8.GetBytes("101,102,103,104,105");

        int count = processor.ProcessTokens(inputBytes.AsSpan(), stackBuffer);

        Console.WriteLine($"Test Passed. Tokens processed: {count}");
        Console.WriteLine($"Tokens: {stackBuffer[0]}, {stackBuffer[1]}, {stackBuffer[2]}...");
        
        // Verify results
        if (count != 5 || stackBuffer[0] != 101) throw new Exception("Unit test failed.");
    }
}
