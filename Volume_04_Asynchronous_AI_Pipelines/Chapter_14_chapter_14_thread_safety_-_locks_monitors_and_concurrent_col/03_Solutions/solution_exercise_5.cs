
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// Using System.Threading.Channels (Modern .NET approach)
public class BlockingStream<T>
{
    private readonly Channel<T> _channel;

    public BlockingStream(int capacity)
    {
        // Bounded channel creates backpressure
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait // Blocks producer if full
        };
        _channel = Channel.CreateBounded<T>(options);
    }

    // Producer API
    public async Task WriteAsync(T item, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(item, cancellationToken);
    }

    public void CompleteWriting() => _channel.Writer.Complete();

    // Consumer API (IAsyncEnumerable)
    public IAsyncEnumerable<T> ReadAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}

public class StreamingPipeline
{
    public static async Task RunPipeline()
    {
        // Create a stream with a small buffer to demonstrate backpressure
        var stream = new BlockingStream<string>(capacity: 5);

        // 1. Producer Task (Simulates LLM Stream)
        var producer = Task.Run(async () =>
        {
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(50); // Simulate network latency
                    string chunk = $"Chunk {i}";
                    Console.WriteLine($"[Producer] Sending: {chunk}");
                    await stream.WriteAsync(chunk);
                }
            }
            finally
            {
                stream.CompleteWriting();
                Console.WriteLine("[Producer] Finished.");
            }
        });

        // 2. Consumer Tasks (Simulates UI Renderers)
        var consumer1 = Task.Run(async () =>
        {
            await foreach (var chunk in stream.ReadAsync())
            {
                Console.WriteLine($"  [Consumer 1] Processed: {chunk}");
                await Task.Delay(100); // Consumer 1 is slower
            }
        });

        var consumer2 = Task.Run(async () =>
        {
            await foreach (var chunk in stream.ReadAsync())
            {
                Console.WriteLine($"  [Consumer 2] Processed: {chunk}");
                await Task.Delay(80); // Consumer 2 is moderately slow
            }
        });

        await Task.WhenAll(producer, consumer1, consumer2);
    }
}
