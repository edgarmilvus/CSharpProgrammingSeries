
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

public class AsyncEnumerableBridge
{
    public static async Task RunAsync()
    {
        // 1. Create the async stream source
        var asyncStream = GenerateAsyncStream();

        // 2. Create a channel to bridge the data
        // Using unbounded for simplicity in this bridge
        var channel = Channel.CreateUnbounded<int>();

        // 3. Start the bridge task (Producer side)
        var bridgeTask = BridgeAsyncEnumerableToChannel(asyncStream, channel.Writer);

        // 4. Start the consumer (Consumer side)
        var consumerTask = ProcessChannelData(channel.Reader);

        await Task.WhenAll(bridgeTask, consumerTask);
    }

    // Source returning IAsyncEnumerable
    private static async IAsyncEnumerable<int> GenerateAsyncStream()
    {
        for (int i = 1; i <= 10; i++)
        {
            await Task.Delay(200); // Simulate async generation
            yield return i;
        }
    }

    // Bridge: Reads from IAsyncEnumerable, Writes to Channel
    private static async Task BridgeAsyncEnumerableToChannel(
        IAsyncEnumerable<int> source, 
        ChannelWriter<int> writer)
    {
        try
        {
            await foreach (var item in source)
            {
                await writer.WriteAsync(item);
                Console.WriteLine($"[Bridge] Pushed {item} to channel");
            }
        }
        finally
        {
            // Signal completion to the channel reader
            writer.Complete();
        }
    }

    // Consumer: Reads from Channel
    private static async Task ProcessChannelData(ChannelReader<int> reader)
    {
        Console.WriteLine("--- Consumer Started ---");
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var number))
            {
                // 3. Process integers (Square them)
                int result = number * number;
                Console.WriteLine($"[Consumer] Received {number}, Squared: {result}");
            }
        }
    }
}
