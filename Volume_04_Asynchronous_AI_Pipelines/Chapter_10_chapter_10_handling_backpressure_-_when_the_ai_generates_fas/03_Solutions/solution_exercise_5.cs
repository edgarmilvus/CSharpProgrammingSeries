
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class PriorityStreamManager
{
    private readonly int _maxItemsPerSecond;

    public PriorityStreamManager(int maxItemsPerSecond)
    {
        _maxItemsPerSecond = maxItemsPerSecond;
    }

    public async IAsyncEnumerable<string> ProcessStreamsAsync(
        IEnumerable<IAsyncEnumerable<string>> streams, 
        IEnumerable<int> priorities, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 1. Setup PriorityQueue
        // PriorityQueue<TElement, TPriority>. Lower number = Higher priority.
        var priorityQueue = new PriorityQueue<string, int>();
        var queueLock = new object();
        
        // Signal for the consumer loop to proceed when items are available
        var itemAvailableSignal = new SemaphoreSlim(0, int.MaxValue);

        // 2. Setup Producer tasks
        var producerTasks = streams.Zip(priorities, (stream, priority) => (stream, priority))
            .Select(async t =>
            {
                var (stream, priority) = t;
                try
                {
                    await foreach (var item in stream.WithCancellation(cancellationToken))
                    {
                        lock (queueLock)
                        {
                            priorityQueue.Enqueue(item, priority);
                        }
                        // Signal the consumer that an item is ready
                        itemAvailableSignal.Release();
                    }
                }
                catch (OperationCanceledException) { /* Ignore */ }
            }).ToArray();

        // 3. Setup Consumer loop with rate limiting
        // Calculate delay between items to enforce rate limit (e.g., 20 items/sec = 50ms delay)
        int delayMs = _maxItemsPerSecond > 0 ? 1000 / _maxItemsPerSecond : 0;

        // Task to monitor completion of producers
        var allProducersFinished = Task.WhenAll(producerTasks);

        while (true)
        {
            // Wait for an item to be enqueued
            await itemAvailableSignal.WaitAsync(cancellationToken);

            // Check if all producers are done and queue is empty
            if (priorityQueue.Count == 0 && allProducersFinished.IsCompleted)
            {
                break;
            }

            string item;
            lock (queueLock)
            {
                // Dequeue the highest priority item (lowest priority number)
                // Note: PriorityQueue in .NET 6+ dequeues the item with the lowest priority value.
                // If we want High Priority = 1, Low Priority = 10, this works naturally.
                // If we want High Priority = 10, Low Priority = 1, we need to invert the priority value.
                item = priorityQueue.Dequeue();
            }

            // 4. Rate Limiting
            if (delayMs > 0)
            {
                await Task.Delay(delayMs, cancellationToken);
            }

            // 5. Yield items
            yield return item;
        }
    }
}
