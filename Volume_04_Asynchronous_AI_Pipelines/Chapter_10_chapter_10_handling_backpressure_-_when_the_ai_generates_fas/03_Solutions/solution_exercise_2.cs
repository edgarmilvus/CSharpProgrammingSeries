
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
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

public class StreamProcessor
{
    public async IAsyncEnumerable<string> FanOutAsync(
        IAsyncEnumerable<string> sourceStream, 
        int workerCount, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 1. Create worker channels
        var workerChannels = new Channel<string>[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            // Using CreateUnbounded for workers to avoid deadlock if the merger is slow,
            // but assuming the merger is fast enough. Alternatively, use bounded.
            workerChannels[i] = Channel.CreateUnbounded<string>();
        }

        // 4. Create merger logic (Output Channel)
        // SingleReader ensures we have only one consumer reading from the output.
        var outputChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(100) 
            { SingleReader = true }); 

        // 2. Create dispatcher task
        var dispatcherTask = Task.Run(async () =>
        {
            int roundRobinIndex = 0;
            await foreach (var item in sourceStream.WithCancellation(cancellationToken))
            {
                // Distribute round-robin
                var targetChannel = workerChannels[roundRobinIndex];
                await targetChannel.Writer.WriteAsync(item, cancellationToken);
                
                roundRobinIndex = (roundRobinIndex + 1) % workerCount;
            }

            // Signal all workers to stop
            foreach (var channel in workerChannels)
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // 3. Create worker tasks
        var workerTasks = new Task[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            var channel = workerChannels[i];
            workerTasks[i] = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    // Simulate CPU-bound work
                    // Example: Calculate Fibonacci to burn CPU cycles
                    long result = 0;
                    for (int k = 0; k < 10000; k++) 
                    {
                        result += CalculateFibonacci(5); // Small calculation repeated
                    }
                    
                    // Write processed result to output
                    await outputChannel.Writer.WriteAsync($"[Worker {i}] Processed: {item} (Hash: {result % 1000})", cancellationToken);
                }
            }, cancellationToken);
        }

        // 5. Yield results
        // We need a task that completes when all workers and the dispatcher are done
        // and the output channel is drained.
        var completionTask = Task.WhenAll(workerTasks.Concat(new[] { dispatcherTask }))
            .ContinueWith(_ => outputChannel.Writer.Complete(), TaskScheduler.Default);

        // Yield from the output channel
        await foreach (var result in outputChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return result;
        }
    }

    private long CalculateFibonacci(int n)
    {
        if (n <= 1) return n;
        return CalculateFibonacci(n - 1) + CalculateFibonacci(n - 2);
    }
}
