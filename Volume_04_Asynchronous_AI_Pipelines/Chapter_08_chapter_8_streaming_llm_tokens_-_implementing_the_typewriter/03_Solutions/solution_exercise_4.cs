
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class BackpressureManager
{
    private readonly Channel<string> _channel;

    public BackpressureManager(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait // Blocks writer when full
        };
        _channel = Channel.CreateBounded<string>(options);
    }

    public async Task RunPipelineAsync()
    {
        var cts = new CancellationTokenSource();
        
        // Start Producer and Consumer tasks
        var producerTask = ProducerAsync(cts.Token);
        var consumerTask = ConsumerAsync(cts.Token);

        // Wait for the producer to finish (simulating end of generation)
        await producerTask;
        
        // Signal consumer to finish after channel is empty
        _channel.Writer.Complete();
        await consumerTask;
    }

    private async Task ProducerAsync(CancellationToken ct)
    {
        int tokenCount = 0;
        while (tokenCount < 50) // Generate 50 tokens
        {
            // Attempt to write to the channel
            // If the channel is full (consumer is slow), this awaits until space is available
            await _channel.Writer.WriteAsync($"Token_{tokenCount}", ct);
            
            Console.WriteLine($"[Producer] Wrote Token_{tokenCount}. Channel Count: {_channel.Reader.Count}");
            
            // Fast production speed (100ms)
            await Task.Delay(100, ct);
            tokenCount++;
        }
    }

    private async Task ConsumerAsync(CancellationToken ct)
    {
        await foreach (var token in _channel.Reader.ReadAllAsync(ct))
        {
            // Simulate slow processing
            Console.WriteLine($"[Consumer] Processing {token}...");
            await Task.Delay(500, ct); // Slower than producer (500ms vs 100ms)
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Capacity of 10 items
        var manager = new BackpressureManager(10);
        await manager.RunPipelineAsync();
    }
}
