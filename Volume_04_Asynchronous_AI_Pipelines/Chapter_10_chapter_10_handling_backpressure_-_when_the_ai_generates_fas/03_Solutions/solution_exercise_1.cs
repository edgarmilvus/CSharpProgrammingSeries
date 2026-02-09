
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
using System.Threading.Channels;
using System.Threading.Tasks;

public class StreamingTextGenerator
{
    private readonly Channel<string> _channel;
    private readonly ChannelWriter<string> _writer;
    private readonly ChannelReader<string> _reader;

    public StreamingTextGenerator()
    {
        // 1. Channel Configuration: Create a bounded channel with capacity 10.
        // FullMode.Wait ensures the producer blocks when the buffer is full.
        var options = new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<string>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public IAsyncEnumerable<string> GetStreamAsync(CancellationToken cancellationToken = default)
        => _reader.ReadAllAsync(cancellationToken);

    public async Task GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Simulate AI generation
        var tokens = new[] { "The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog." };
        
        foreach (var token in tokens)
        {
            // 2. Producer Logic: Check if we can write (optimistic check).
            // If the channel is full, WaitToWriteAsync will suspend execution 
            // until the consumer reads an item, creating backpressure.
            if (!await _writer.WaitToWriteAsync(cancellationToken))
            {
                // 3. Edge Case Handling: If WaitToWriteAsync returns false, 
                // the channel has been closed (e.g., via Complete() or an error).
                // We should stop producing.
                break;
            }

            // Write the token. Since we awaited WaitToWriteAsync, 
            // this write is guaranteed to succeed without blocking indefinitely.
            await _writer.WriteAsync(token, cancellationToken);
            
            // Simulate network latency
            await Task.Delay(50, cancellationToken); 
        }

        // Signal completion to the reader
        _writer.Complete();
    }
}
