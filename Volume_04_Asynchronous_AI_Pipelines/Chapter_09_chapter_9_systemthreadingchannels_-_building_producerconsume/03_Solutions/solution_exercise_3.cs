
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
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class StreamingLLMWithCancellation
{
    public static async Task RunAsync()
    {
        // 5. Interactive Challenge: Optimized channel for single reader/writer
        var options = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        };
        var channel = Channel.CreateUnbounded<string>(options);

        // Create a cancellation token source to simulate user clicking "Stop"
        var cts = new CancellationTokenSource();

        // Start the TokenGenerator (Producer)
        var producerTask = TokenGenerator(channel.Writer, cts.Token);

        // Start the BufferingConsumer
        var consumerTask = BufferingConsumer(channel.Reader);

        // Simulate user interaction: wait for a few tokens then cancel
        await Task.Delay(1500); 
        Console.WriteLine("\n[User Action] Triggering Cancellation...\n");
        cts.Cancel();

        // Wait for tasks to handle cancellation
        try
        {
            await Task.WhenAll(producerTask, consumerTask);
        }
        catch (TaskCanceledException)
        {
            // Expected behavior when awaiting the producer directly
        }

        Console.WriteLine("Stream stopped safely.");
    }

    private static async Task TokenGenerator(ChannelWriter<string> writer, CancellationToken token)
    {
        int count = 1;
        try
        {
            while (!token.IsCancellationRequested)
            {
                string tokenText = $"Token {count++}";
                
                // Write to channel. 
                // Note: Unbounded channel WriteAsync is not truly async but returns a ValueTask.
                await writer.WriteAsync(tokenText, token);
                
                await Task.Delay(300, token); // Delay between tokens
            }
        }
        catch (OperationCanceledException)
        {
            // Clean exit on cancellation
        }
        finally
        {
            // Signal completion to the consumer
            writer.Complete();
        }
    }

    private static async Task BufferingConsumer(ChannelReader<string> reader)
    {
        var buffer = new StringBuilder();
        int tokensProcessed = 0;

        // Read loop handles cancellation automatically when writer completes
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var token))
            {
                buffer.Append(token + " ");
                tokensProcessed++;

                // Periodic flush simulation
                if (tokensProcessed % 5 == 0)
                {
                    Console.WriteLine($"[Buffer Flush] {buffer.ToString().TrimEnd()}");
                    buffer.Clear();
                    await Task.Delay(100); // Simulate rendering delay
                }
            }
        }

        // Flush remaining tokens
        if (buffer.Length > 0)
        {
            Console.WriteLine($"[Final Flush] {buffer.ToString().TrimEnd()}");
        }
        Console.WriteLine($"[Consumer] Total tokens processed: {tokensProcessed}");
    }
}
