
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class InferenceRequest
{
    public Guid Id { get; set; }
    public string Input { get; set; }
}

public class InferenceService : IAsyncDisposable
{
    private readonly Channel<InferenceRequest> _channel;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _pendingRequests;
    private readonly Task _processingTask;
    private readonly CancellationTokenSource _cts;

    public InferenceService()
    {
        _channel = Channel.CreateBounded<InferenceRequest>(100);
        _pendingRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<string>>();
        _cts = new CancellationTokenSource();
        
        // Start the background processor immediately
        _processingTask = Task.Run(() => BatchProcessorLoop(_cts.Token));
    }

    public async Task<string> InferAsync(string input)
    {
        var id = Guid.NewGuid();
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        // Store the TCS before writing to channel to avoid race conditions
        if (!_pendingRequests.TryAdd(id, tcs))
        {
            throw new InvalidOperationException("Failed to register request.");
        }

        try
        {
            await _channel.Writer.WriteAsync(new InferenceRequest { Id = id, Input = input });
        }
        catch (ChannelClosedException)
        {
            // Handle case where service is shutting down
            _pendingRequests.TryRemove(id, out _);
            throw new OperationCanceledException("Inference service is shutting down.");
        }

        return await tcs.Task;
    }

    private async Task BatchProcessorLoop(CancellationToken token)
    {
        var batch = new List<InferenceRequest>(16);

        try
        {
            while (!token.IsCancellationRequested)
            {
                // Wait for the first item to start the batch
                InferenceRequest firstItem = await _channel.Reader.ReadAsync(token);
                batch.Add(firstItem);

                // Start a timer for the dynamic timeout
                using var timeoutCts = new CancellationTokenSource(50); // 50ms timeout
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);

                try
                {
                    // Attempt to fill the batch up to 16 items
                    while (batch.Count < 16)
                    {
                        // TryRead is non-blocking and fast
                        if (_channel.Reader.TryRead(out var item))
                        {
                            batch.Add(item);
                        }
                        else
                        {
                            // If channel is empty, wait for next item OR timeout
                            await _channel.Reader.WaitToReadAsync(linkedCts.Token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout occurred or cancellation requested. 
                    // We proceed to process whatever is in the batch.
                }

                // Process the batch
                if (batch.Count > 0)
                {
                    await ProcessBatchAsync(batch);
                    batch.Clear();
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Graceful shutdown
        }
        finally
        {
            // Flush remaining items before exiting
            while (_channel.Reader.TryRead(out var item))
            {
                batch.Add(item);
            }
            if (batch.Count > 0)
            {
                await ProcessBatchAsync(batch);
            }
            
            // Close the channel writer to signal no more writes
            _channel.Writer.TryComplete();
        }
    }

    private async Task ProcessBatchAsync(List<InferenceRequest> batch)
    {
        // Simulate heavy ML inference
        await Task.Delay(100); 

        var results = new Dictionary<Guid, string>();
        
        // Generate mock results
        foreach (var req in batch)
        {
            results[req.Id] = $"Processed: {req.Input}";
        }

        // Complete the tasks
        foreach (var kvp in results)
        {
            if (_pendingRequests.TryRemove(kvp.Key, out var tcs))
            {
                tcs.TrySetResult(kvp.Value);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        _cts.Cancel();
        await _processingTask;
        _cts.Dispose();
    }
}
