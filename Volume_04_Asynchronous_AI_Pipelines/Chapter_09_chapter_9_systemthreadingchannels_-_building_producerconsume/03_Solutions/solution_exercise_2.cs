
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

public class FanOutFanInLLM
{
    public static async Task RunAsync()
    {
        // 1. Unbounded input channel for prompts, Bounded output channel for results
        var promptChannel = Channel.CreateUnbounded<string>();
        var resultChannel = Channel.CreateBounded<string>(10);

        // 2. Producer populates prompts
        var producerTask = ProducerAsync(promptChannel.Writer);

        // 3. Start 3 parallel consumers
        var consumerTasks = new List<Task>();
        for (int i = 0; i < 3; i++)
        {
            consumerTasks.Add(ConsumerAsync(i, promptChannel.Reader, resultChannel.Writer));
        }

        // 4. Aggregator reads results
        var aggregatorTask = AggregatorAsync(resultChannel.Reader);

        // Wait for producer to finish sending
        await producerTask;
        
        // Wait for all consumers to finish processing and signal completion
        await Task.WhenAll(consumerTasks);
        resultChannel.Writer.Complete();

        // Wait for aggregator to finish
        await aggregatorTask;

        Console.WriteLine("All processing complete.");
    }

    private static async Task ProducerAsync(ChannelWriter<string> writer)
    {
        for (int i = 1; i <= 20; i++)
        {
            await writer.WriteAsync($"Prompt_{i}");
        }
        writer.Complete(); // Signal no more prompts
    }

    private static async Task ConsumerAsync(int id, ChannelReader<string> reader, ChannelWriter<string> resultWriter)
    {
        // Read until channel is empty and completed
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var prompt))
            {
                // Simulate LLM API call
                var delay = new Random().Next(50, 200);
                await Task.Delay(delay);
                
                var result = $"[Consumer {id}] Response to {prompt} (took {delay}ms)";
                
                // Write to result channel (handles backpressure if bounded)
                await resultWriter.WriteAsync(result);
            }
        }
    }

    private static async Task AggregatorAsync(ChannelReader<string> reader)
    {
        Console.WriteLine("--- Aggregator Started ---");
        int count = 0;
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var result))
            {
                count++;
                Console.WriteLine($"[Aggregator] Collected: {result}");
            }
        }
        Console.WriteLine($"--- Aggregator Finished. Total Results: {count} ---");
    }
}
