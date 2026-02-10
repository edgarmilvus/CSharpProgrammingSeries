
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class BatchedStream<T>
{
    private readonly IAsyncEnumerable<T> _source;
    private readonly int _maxBatchSize;
    private readonly TimeSpan _flushInterval;

    public BatchedStream(IAsyncEnumerable<T> source, int maxBatchSize, TimeSpan flushInterval)
    {
        _source = source;
        _maxBatchSize = maxBatchSize;
        _flushInterval = flushInterval;
    }

    public async IAsyncEnumerable<IReadOnlyList<T>> GetBatchesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new List<T>(_maxBatchSize);
        var flushCts = new CancellationTokenSource();
        // Link the internal flush token with the external cancellation token
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, flushCts.Token);

        // Timer task to trigger flush after interval
        var timerTask = Task.Delay(_flushInterval, linkedCts.Token)
            .ContinueWith(t => 
            {
                // Ignore cancellation exceptions, handle only timeout
                if (!t.IsCanceled) 
                    TriggerFlush(); 
            }, TaskScheduler.Default);

        // Local function to flush the buffer
        void TriggerFlush()
        {
            if (buffer.Count > 0)
            {
                // We yield the current buffer content.
                // Note: In a real async enumerator, we can't 'yield' inside a local function directly 
                // unless we are using C# 8+ async streams with specific patterns.
                // To handle this cleanly, we will manage flushing via the main loop logic 
                // or a shared state mechanism. 
                // However, for this specific implementation, we will handle the flush 
                // by resetting the buffer and signaling the main loop to yield.
            }
        }

        // Since we cannot yield from inside the timer callback, 
        // we will use a slightly different approach: 
        // We will check the timer status in the loop or use a Channel to bridge the gap.
        // A cleaner approach for AsyncStreams is to use a Producer/Consumer pattern internally.
        
        // Let's refactor to use an internal Channel for the batcher logic to handle concurrency safely.
        var batchChannel = Channel.CreateBounded<IReadOnlyList<T>>(new BoundedChannelOptions(10) { SingleReader = true, SingleWriter = true });

        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in _source.WithCancellation(cancellationToken))
                {
                    buffer.Add(item);
                    if (buffer.Count >= _maxBatchSize)
                    {
                        await batchChannel.Writer.WriteAsync(buffer, cancellationToken);
                        buffer = new List<T>(_maxBatchSize);
                        // Reset timer
                        flushCts.Cancel(); 
                        flushCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        timerTask = Task.Delay(_flushInterval, flushCts.Token)
                            .ContinueWith(t => { if (!t.IsCanceled) batchChannel.Writer.WriteAsync(new List<T>()).AsTask().Wait(); }, TaskScheduler.Default);
                    }
                }
                
                // Flush remaining items
                if (buffer.Count > 0)
                {
                    await batchChannel.Writer.WriteAsync(buffer, cancellationToken);
                }
            }
            catch (OperationCanceledException) { /* Graceful exit */ }
            finally
            {
                batchChannel.Writer.Complete();
            }
        }, cancellationToken);

        await foreach (var batch in batchChannel.Reader.ReadAllAsync(cancellationToken))
        {
            if (batch.Count > 0)
                yield return batch;
        }
    }
}
