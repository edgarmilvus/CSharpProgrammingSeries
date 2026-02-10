
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

// The request record includes a Priority property for the interactive challenge.
public record InferenceRequest(Guid Id, byte[] InputData, int Priority);

public class BatchingService : BackgroundService
{
    private readonly Channel<InferenceRequest> _channel;
    private readonly TimeSpan _timeout;
    private readonly int _maxBatchSize;
    private readonly PriorityQueue<InferenceRequest, int> _priorityQueue;

    public BatchingService(TimeSpan timeout, int maxBatchSize)
    {
        // Create an unbounded channel to handle high throughput without backpressure.
        _channel = Channel.CreateUnbounded<InferenceRequest>();
        _timeout = timeout;
        _maxBatchSize = maxBatchSize;
        // PriorityQueue is used for the Interactive Challenge to sort by priority (lower int = higher priority).
        _priorityQueue = new PriorityQueue<InferenceRequest, int>();
    }

    public async Task EnqueueAsync(InferenceRequest request)
    {
        // Write to the channel asynchronously.
        await _channel.Writer.WriteAsync(request);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Loop until cancellation is requested.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Attempt to read the first item for the batch.
                // WaitAsync respects the cancellation token.
                var firstItem = await _channel.Reader.WaitAsync(stoppingToken);
                
                // Add the first item to our priority queue.
                _priorityQueue.Enqueue(firstItem, firstItem.Priority);

                // Start the timeout countdown.
                var batchDeadline = DateTime.UtcNow.Add(_timeout);

                // Try to fill the batch until size or time limit is reached.
                while (_priorityQueue.Count < _maxBatchSize)
                {
                    var timeRemaining = batchDeadline - DateTime.UtcNow;
                    
                    if (timeRemaining <= TimeSpan.Zero) break;

                    // Try to peek at the next item without waiting indefinitely.
                    if (_channel.Reader.TryRead(out var nextItem))
                    {
                        _priorityQueue.Enqueue(nextItem, nextItem.Priority);
                    }
                    else
                    {
                        // If channel is empty, wait briefly for new items or timeout.
                        try
                        {
                            await Task.Delay(timeRemaining, stoppingToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }

                // Flush the batch: Dequeue all items currently in the priority queue.
                var batch = new List<InferenceRequest>();
                while (_priorityQueue.Count > 0)
                {
                    batch.Add(_priorityQueue.Dequeue());
                }

                if (batch.Count > 0)
                {
                    // Process the batch (Simulate Inference).
                    await ProcessBatchAsync(batch);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation token triggered, exit loop.
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(List<InferenceRequest> batch)
    {
        // Simulate GPU inference call.
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] Starting inference for batch of {batch.Count} items.");
        
        // Simulate processing time (e.g., 50ms).
        await Task.Delay(50);

        // In a real scenario, results would be returned via a callback or response stream.
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] Completed inference for batch. Priorities processed: {string.Join(", ", batch.Select(b => b.Priority))}");
    }
}

// Mock BackgroundService base class for context
public abstract class BackgroundService : IHostedService
{
    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
    public virtual Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
