
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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class StreamingLlmClient
{
    private readonly ChannelReader<string> _channelReader;
    private readonly Memory<char> _internalBuffer;
    private int _bufferOffset = 0;
    private int _validDataLength = 0; // Tracks how much data is currently in the buffer

    public StreamingLlmClient(ChannelReader<string> channelReader)
    {
        _channelReader = channelReader;
        // Rent buffer from ArrayPool instead of new allocation
        char[] rentedArray = ArrayPool<char>.Shared.Rent(1024);
        _internalBuffer = new Memory<char>(rentedArray);
    }

    // Optimized Method
    public async ValueTask<string> GetNextToken()
    {
        // 1. Synchronous Fast Path: Check if we have data in the internal buffer
        if (_bufferOffset < _validDataLength)
        {
            // Extract token from buffer logic (simplified for exercise: return a chunk)
            // In a real scenario, we would parse delimiters here.
            // For this exercise, we simulate consuming the buffer.
            int start = _bufferOffset;
            int length = Math.Min(5, _validDataLength - _bufferOffset); // Simulate reading 5 chars
            _bufferOffset += length;
            
            // Convert buffer segment to string
            string token = _internalBuffer.Slice(start, length).ToString();
            return token;
        }

        // 2. Asynchronous Path: Buffer is empty, wait for channel
        if (await _channelReader.WaitToReadAsync())
        {
            if (_channelReader.TryRead(out string? token))
            {
                // Simulate filling the internal buffer with the read token
                // In a real scenario, we might fill the buffer and then parse.
                // Here, we just return the token directly for simplicity, 
                // but we acknowledge the buffer logic exists.
                return token;
            }
        }

        // Return array to pool to prevent leaks
        if (_internalBuffer.Length > 0)
        {
            ArrayPool<char>.Shared.Return(_internalBuffer.ToArray());
        }
        
        return "[END]";
    }

    public async Task ConsumeStream()
    {
        string token;
        do
        {
            // Handle ValueTask correctly
            token = await GetNextToken();
            Console.WriteLine(token);
        } while (token != "[END]");
    }
}

// Example usage setup (not part of the class, but needed to run)
public static class PipelineDemo
{
    public static async Task Run()
    {
        var channel = Channel.CreateUnbounded<string>();
        var writer = channel.Writer;
        var reader = channel.Reader;

        // Producer Task
        var producer = Task.Run(async () =>
        {
            for (int i = 0; i < 5; i++)
            {
                await writer.WriteAsync($"Token{i}");
                await Task.Delay(50);
            }
            writer.Complete();
        });

        var client = new StreamingLlmClient(reader);
        await client.ConsumeStream();
        await producer;
    }
}
