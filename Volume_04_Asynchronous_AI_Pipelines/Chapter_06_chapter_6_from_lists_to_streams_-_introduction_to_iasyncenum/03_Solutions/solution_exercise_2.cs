
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

public class ChannelBackpressureDemo
{
    // 4. Configure the channel with BoundedChannelOptions
    private static Channel<string> CreateBoundedChannel()
    {
        var options = new BoundedChannelOptions(capacity: 5)
        {
            // Wait mode blocks the producer if the channel is full, 
            // creating natural backpressure.
            FullMode = BoundedChannelFullMode.Wait 
        };
        return Channel.CreateBounded<string>(options);
    }

    // 2. Producer Method
    public static async Task ProduceTokensAsync(ChannelWriter<string> writer, int tokenCount)
    {
        try
        {
            for (int i = 1; i <= tokenCount; i++)
            {
                string token = $"Token_{i}";
                
                // Simulate fast generation (e.g., 10ms)
                await Task.Delay(10); 
                
                Console.WriteLine($"[Producer] Generating: {token}");
                
                // Write to channel. If capacity is full (5 items), 
                // this await blocks until space is available.
                await writer.WriteAsync(token);
            }
        }
        finally
        {
            // Signal that no more data will be written
            writer.Complete();
        }
    }

    // 3. Consumer Method (Slow)
    public static async Task ConsumeAndSummarizeAsync(ChannelReader<string> reader)
    {
        // Read until the channel is closed and empty
        while (await reader.WaitToReadAsync())
        {
            if (reader.TryRead(out string? token))
            {
                // Simulate slow processing (e.g., 50ms)
                // This delay causes the channel buffer to fill up.
                await Task.Delay(50); 
                Console.WriteLine($"[Consumer] Processed: {token}");
            }
        }
    }

    // 5. Wrapper for IAsyncEnumerable (Optional, but good for integration)
    public static async IAsyncEnumerable<string> GetStreamAsync()
    {
        var channel = CreateBoundedChannel();
        
        // Start producer and consumer tasks
        var producer = ProduceTokensAsync(channel.Writer, 20);
        var consumer = ConsumeAndSummarizeAsync(channel.Reader);

        // We can yield values directly from the reader if we want to expose 
        // the channel as an IAsyncEnumerable to external callers.
        while (await channel.Reader.WaitToReadAsync())
        {
            if (channel.Reader.TryRead(out string? item))
            {
                yield return item;
            }
        }

        // Ensure tasks complete (though in this specific demo structure, 
        // we usually await them in Main or a wrapper task).
        await Task.WhenAll(producer, consumer);
    }

    public static async Task RunDemo()
    {
        var channel = CreateBoundedChannel();

        // Run Producer and Consumer concurrently
        var producerTask = ProduceTokensAsync(channel.Writer, 20);
        var consumerTask = ConsumeAndSummarizeAsync(channel.Reader);

        Console.WriteLine("--- Starting Producer/Consumer with Backpressure ---");
        await Task.WhenAll(producerTask, consumerTask);
        Console.WriteLine("--- Finished ---");
    }
}

// Entry point for execution
// await ChannelBackpressureDemo.RunDemo();
