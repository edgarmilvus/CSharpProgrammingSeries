
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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class BackgroundInferenceService
{
    public async Task RunInferenceAsync(ChannelWriter<string> writer, CancellationToken ct)
    {
        try
        {
            for (int i = 0; i < 20; i++)
            {
                ct.ThrowIfCancellationRequested();
                
                // Simulate inference time
                await Task.Delay(100, ct);
                
                string token = $"Token_{i}";
                
                // Write to channel. If the channel is full (bounded capacity), 
                // this will wait until space is available (Backpressure).
                await writer.WriteAsync(token, ct);
            }
        }
        finally
        {
            // Signal completion to the reader
            writer.TryComplete();
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create a bounded channel to handle backpressure (e.g., capacity of 5)
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        var cts = new CancellationTokenSource();
        var service = new BackgroundInferenceService();

        // Producer Task
        Task producer = service.RunInferenceAsync(channel.Writer, cts.Token);

        // Consumer Task (Simulating UI updates)
        Task consumer = Task.Run(async () =>
        {
            await foreach (var token in channel.Reader.ReadAllAsync(cts.Token))
            {
                // Simulate UI rendering lag (e.g., complex layout calculation)
                // This demonstrates that the producer will pause if the consumer is slow
                // because of the BoundedChannel 'Wait' mode.
                await Task.Delay(300); 
                Console.WriteLine($"UI Updated: {token} at {DateTime.Now:HH:mm:ss.fff}");
            }
        });

        await Task.WhenAll(producer, consumer);
        Console.WriteLine("Streaming complete.");
    }
}
