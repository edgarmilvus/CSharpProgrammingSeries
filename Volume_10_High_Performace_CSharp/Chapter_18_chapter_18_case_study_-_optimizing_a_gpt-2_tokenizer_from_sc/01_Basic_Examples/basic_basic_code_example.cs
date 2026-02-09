
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Text;

public class ZeroAllocationTokenizer
{
    public static void Main()
    {
        // 1. Simulate raw input data (e.g., from a network stream or file).
        // In a real GPT-2 tokenizer, this would be the raw byte stream of the text.
        byte[] rawInputBytes = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog.");

        // 2. Wrap the array in a Memory<T>.
        // Memory<T> is the modern abstraction for memory that can be located on the stack or heap.
        Memory<byte> inputMemory = rawInputBytes.AsMemory();

        // 3. Define the "vocabulary" or a specific token we are looking for.
        // We use ReadOnlySpan<char> to represent this token without allocating a string.
        ReadOnlySpan<char> targetToken = "fox".AsSpan();

        Console.WriteLine($"Processing input of length: {inputMemory.Length} bytes");
        Console.WriteLine($"Looking for token: {targetToken}");

        // 4. Process the memory to find the token.
        // We pass the Memory and the target token to our processing method.
        ProcessAndFindToken(inputMemory, targetToken);
    }

    /// <summary>
    /// Processes a block of memory to find a specific token efficiently.
    /// </summary>
    /// <param name="input">The input data wrapped in Memory.</param>
    /// <param name="token">The token to search for, wrapped in a Span.</param>
    private static void ProcessAndFindToken(Memory<byte> input, ReadOnlySpan<char> token)
    {
        // 5. Convert the token (char span) to bytes for comparison.
        // This conversion is done on the stack (using stackalloc if small, or a rented array).
        // We use UTF8 encoding to match the input data format.
        int maxByteLength = Encoding.UTF8.GetMaxByteCount(token.Length);
        
        // Span<byte> allows us to work with bytes safely without pinning or heap allocation.
        Span<byte> tokenBytes = maxByteLength <= 128 
            ? stackalloc byte[maxByteLength] // Fast stack allocation for small tokens
            : new byte[maxByteLength];       // Fallback to heap for very large tokens (rare)

        // Encode the char span into the byte span
        int bytesWritten = Encoding.UTF8.GetBytes(token, tokenBytes);
        tokenBytes = tokenBytes.Slice(0, bytesWritten); // Resize to actual bytes written

        // 6. Slice the input memory to process it in chunks (simulating tokenization).
        // We will look for the token starting at index 16 (which happens to be 'f' in "fox").
        // Slicing Memory<T> creates a new view, not a copy. Zero allocation.
        Memory<byte> chunk = inputMemory.Slice(16, 3); 

        // 7. Compare the chunk with the token bytes.
        // We need to check if the memory is contiguous (likely true here) to use Span.
        if (chunk.Span.SequenceEqual(tokenBytes))
        {
            Console.WriteLine("\nSuccess! Found the token 'fox' at the expected position.");
            Console.WriteLine($"Chunk data (UTF-8): {Encoding.UTF8.GetString(chunk.Span)}");
        }
        else
        {
            Console.WriteLine("\nToken not found in the specific chunk.");
        }

        // 8. Demonstrate iterating over the whole buffer using Spans.
        // This is how BPE (Byte Pair Encoding) merging loops work.
        Console.WriteLine("\n--- Iterating over the buffer using Spans ---");
        
        // Create a ReadOnlySpan of the entire input for safe, bounds-checked iteration.
        ReadOnlySpan<byte> remainingData = input.Span;

        // We simulate finding a boundary (whitespace) to isolate words.
        int wordStart = 0;
        for (int i = 0; i < remainingData.Length; i++)
        {
            // Check for whitespace (ASCII 32)
            if (remainingData[i] == 32)
            {
                // Slice the word (exclusive of the space)
                ReadOnlySpan<byte> word = remainingData.Slice(wordStart, i - wordStart);
                
                // Convert back to string just for printing (allocation happens here for display only)
                // In a real tokenizer, we would process the byte span directly.
                Console.WriteLine($"Found word: {Encoding.UTF8.GetString(word)}");
                
                wordStart = i + 1; // Move start to next character
            }
        }
        
        // Print the last word
        if (wordStart < remainingData.Length)
        {
            Console.WriteLine($"Found word: {Encoding.UTF8.GetString(remainingData.Slice(wordStart))}");
        }
    }
}
