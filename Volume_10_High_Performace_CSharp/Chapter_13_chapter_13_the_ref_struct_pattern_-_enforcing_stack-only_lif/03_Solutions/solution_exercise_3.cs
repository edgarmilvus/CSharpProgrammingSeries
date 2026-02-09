
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
using System.Collections.Generic;
using System.Text;

// REFACTORED IMPLEMENTATION
public ref struct StreamingTokenizer
{
    // We use a Span to hold the working buffer.
    // Note: In a real high-performance scenario, we might use a fixed buffer 
    // or pass the working slice through the processing loop.
    private Span<byte> _workingBuffer;
    private int _position;

    public StreamingTokenizer(Span<byte> buffer)
    {
        _workingBuffer = buffer;
        _position = 0;
    }

    public void Process(ReadOnlySpan<byte> input, Action<ReadOnlySpan<byte>> onTokenFound)
    {
        foreach (byte b in input)
        {
            if (b == 0x0A) // Newline token
            {
                // Emit the token found so far
                if (_position > 0)
                {
                    // Slice the working buffer to get the current token
                    ReadOnlySpan<byte> tokenSpan = _workingBuffer.Slice(0, _position);
                    onTokenFound(tokenSpan);
                    _position = 0;
                }
            }
            else
            {
                // Check for buffer overflow (safety check)
                if (_position < _workingBuffer.Length)
                {
                    _workingBuffer[_position++] = b;
                }
            }
        }
    }
}

public static class TokenizerRefactorDemo
{
    public static void Run()
    {
        // Simulate a stream of bytes
        byte[] data = System.Text.Encoding.UTF8.GetBytes("token1\ntoken2\ntoken3\n");
        
        // Allocate a buffer on the stack (or use stackalloc in a real unsafe context)
        // Here we use a fixed array for demonstration, but the tokenizer treats it as a Span.
        Span<byte> buffer = new byte[100]; 

        var tokenizer = new StreamingTokenizer(buffer);

        // The 'results' list is allocated outside the hot path (or could be a ValueListBuilder in .NET Core)
        var results = new List<string>();

        tokenizer.Process(data, tokenSpan =>
        {
            // Allocation happens here, but only when a token is actually found.
            // This is acceptable as we need to store the result.
            // The critical part is that the *processing* loop (Process method) 
            // creates zero garbage.
            string tokenString = Encoding.UTF8.GetString(tokenSpan);
            results.Add(tokenString);
            Console.WriteLine($"Found token: {tokenString}");
        });
    }
}
