
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Threading.Channels;
using System.Threading.Tasks;

public class BoundedChannelBackpressure
{
    public static async Task RunAsync()
    {
        // 1. Create a BoundedChannel with a capacity of 5 items.
        // We use FullMode.Wait to block the producer when the channel is full.
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        // Start Producer and Consumer tasks
        var producerTask = ProducerAsync(channel.Writer);
        var consumerTask = ConsumerAsync(channel.Reader);

        // Wait for both to complete
        await Task.WhenAll(producerTask, consumerTask);
        
        Console.WriteLine("Pipeline finished.");
    }

    private static async Task ProducerAsync(ChannelWriter<string> writer)
    {
        try
        {
            for (int i = 1; i <= 20; i++)
            {
                string imageId = $"Image_{i}";
                
                // 2. Write to channel. If full (capacity 5), WaitToWriteAsync will pause here.
                await writer.WriteAsync(imageId);
                
                Console.WriteLine($"[Producer] Sent: {imageId}");
                
                // Simulate fast production (faster than consumer)
                await Task.Delay(200); 
            }
        }
        finally
        {
            // Signal that no more data will be written
            writer.Complete();
        }
    }

    private static async Task ConsumerAsync(ChannelReader<string> reader)
    {
        // 3. Read until the channel is completed and empty
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var imageId))
            {
                Console.WriteLine($"[Consumer] Processing: {imageId}");
                
                // Simulate CPU-intensive work (slower than production)
                await Task.Delay(1000); 
            }
        }
    }
}
