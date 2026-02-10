
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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class StreamGenerators
{
    public static async IAsyncEnumerable<string> GenerateServerLogsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        int count = 0;
        while (!ct.IsCancellationRequested)
        {
            yield return $"Log Entry #{++count}: User login at {DateTime.Now:HH:mm:ss}";
            await Task.Delay(200, ct);
        }
    }

    public static async IAsyncEnumerable<string> GenerateMetricsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        int count = 0;
        while (!ct.IsCancellationRequested)
        {
            yield return $"Metric Update #{++count}: CPU: {new Random().Next(10, 90)}%";
            await Task.Delay(350, ct);
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        
        // Create a channel to merge the streams
        var channel = Channel.CreateUnbounded<string>();

        // Producer 1: Logs
        var logProducer = Task.Run(async () =>
        {
            await foreach (var log in StreamGenerators.GenerateServerLogsAsync(cts.Token))
            {
                await channel.Writer.WriteAsync($"[LOG] {log}", cts.Token);
            }
        });

        // Producer 2: Metrics
        var metricProducer = Task.Run(async () =>
        {
            await foreach (var metric in StreamGenerators.GenerateMetricsAsync(cts.Token))
            {
                await channel.Writer.WriteAsync($"[METRIC] {metric}", cts.Token);
            }
        });

        // Consumer
        var consumer = Task.Run(async () =>
        {
            // Read from the channel until the writers complete and the channel is empty/closed
            await foreach (var item in channel.Reader.ReadAllAsync(cts.Token))
            {
                Console.WriteLine(item);
            }
        });

        // Interactive: Run for 3 seconds then cancel
        Console.WriteLine("Merging streams for 3 seconds...");
        await Task.Delay(3000);
        cts.Cancel();

        // Wait for producers to finish writing
        await Task.WhenAll(logProducer, metricProducer);
        
        // Close the channel writer to allow the consumer loop to exit gracefully
        channel.Writer.Complete();
        
        // Wait for consumer to finish processing remaining items
        await consumer;

        Console.WriteLine("Stream merging complete.");
    }
}
