
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
using System.Buffers;
using System.Text;

// A real-world context: High-frequency tokenization in an AI inference engine.
// We need to parse a raw byte stream representing a sentence into tokens (integers)
// without triggering any heap allocations or Garbage Collection (GC) pauses,
// ensuring predictable low latency.

public ref struct TokenSpanParser
{
    // The 'ref struct' constraint enforces that this type can only live on the stack.
    // It CANNOT be a field in a class, CANNOT be boxed, and CANNOT be used in async methods.
    // This guarantees zero heap allocations for this parser instance itself.

    private ReadOnlySpan<byte> _inputBuffer;

    public TokenSpanParser(ReadOnlySpan<byte> input)
    {
        _inputBuffer = input;
    }

    // This method demonstrates processing a slice of data without copying.
    // It returns a tuple containing the parsed token and the number of bytes consumed.
    public (int Token, int BytesConsumed) ReadNextToken()
    {
        // 1. Trim leading whitespace (common in token streams)
        _inputBuffer = _inputBuffer.TrimStart((byte)' ');

        if (_inputBuffer.IsEmpty)
        {
            return (0, 0);
        }

        // 2. Find the end of the current token (delimited by space or end of buffer)
        int delimiterIndex = _inputBuffer.IndexOf((byte)' ');
        
        // If no space is found, the token extends to the end of the buffer.
        ReadOnlySpan<byte> tokenSpan = delimiterIndex == -1 
            ? _inputBuffer 
            : _inputBuffer.Slice(0, delimiterIndex);

        // 3. "Parse" the token. 
        // In a real AI scenario, this might look up a dictionary value.
        // Here, we simulate it by summing byte values (purely for demonstration).
        int tokenValue = 0;
        foreach (byte b in tokenSpan)
        {
            tokenValue += b;
        }

        // 4. Calculate bytes consumed (token length + 1 for the delimiter, if exists)
        int bytesConsumed = tokenSpan.Length;
        if (delimiterIndex != -1)
        {
            bytesConsumed++; // Consume the space
        }

        // 5. Advance the internal span for the next read
        // We use Slice to move the 'window' forward. 
        // This is a pointer arithmetic operation; no data is copied.
        _inputBuffer = _inputBuffer.Slice(bytesConsumed);

        return (tokenValue, bytesConsumed);
    }
}

public class Program
{
    public static void Main()
    {
        // Simulate a raw byte stream from a network packet or file.
        // In a real AI scenario, this might be UTF-8 encoded text.
        byte[] rawData = Encoding.UTF8.GetBytes("prompt: Hello AI response: World");
        
        // We pass a Span<T> to the parser. 
        // Notice we are NOT allocating a new string or array.
        TokenSpanParser parser = new TokenSpanParser(rawData.AsSpan());

        Console.WriteLine("Processing tokens from Span without Heap Allocation:");
        Console.WriteLine("-----------------------------------------------------");

        while (true)
        {
            // Read the next token.
            // The 'ref struct' lives entirely on the stack. 
            // The loop processes data in place.
            var result = parser.ReadNextToken();

            if (result.BytesConsumed == 0) break;

            // Output the simulated token value.
            Console.WriteLine($"Token Value: {result.Token} (Bytes read: {result.BytesConsumed})");
        }
    }
}
