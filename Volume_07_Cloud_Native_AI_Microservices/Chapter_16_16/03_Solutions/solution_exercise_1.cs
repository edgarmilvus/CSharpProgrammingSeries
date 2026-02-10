
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System.Collections.Concurrent;
using System.Threading.Channels;

public class BatchingProcessor<TInput, TOutput>
{
    private readonly Channel<TInput> _channel;
    private readonly Func<List<TInput>, Task<List<TOutput>>> _batchProcessor;
    private readonly int _maxBatchSize;
    private readonly TimeSpan _flushInterval;
    private readonly CancellationTokenSource _cts = new();

    public BatchingProcessor(
        Func<List<TInput>, Task<List<TOutput>>> batchProcessor, 
        int maxBatchSize = 32, 
        TimeSpan? flushInterval = null)
    {
        _batchProcessor = batchProcessor;
        _maxBatchSize = maxBatchSize;
        _flushInterval = flushInterval ?? TimeSpan.FromMilliseconds(50);
        
        // Unbounded channel to prevent blocking producers
        _channel = Channel.CreateUnbounded<TInput>();

        // Start the processing loop
        Task.Run(() => ProcessLoopAsync(_cts.Token));
    }

    public async Task EnqueueAsync(TInput item)
    {
        await _channel.Writer.WriteAsync(item);
    }

    private async Task ProcessLoopAsync(CancellationToken token)
    {
        var batch = new List<TInput>(_maxBatchSize);
        using var timer = new PeriodicTimer(_flushInterval);

        while (!token.IsCancellationRequested)
        {
            // Wait for either batch size limit or timer tick
            // We use TryRead to drain the channel
            while (batch.Count < _maxBatchSize && _channel.Reader.TryRead(out var item))
            {
                batch.Add(item);
            }

            // Check if we should flush based on size
            if (batch.Count >= _maxBatchSize)
            {
                await FlushBatchAsync(batch);
                continue; // Continue immediately if full
            }

            // Check if we should flush based on time
            if (batch.Count > 0 && await timer.WaitForNextTickAsync(token))
            {
                await FlushBatchAsync(batch);
            }
        }
    }

    private async Task FlushBatchAsync(List<TInput> batch)
    {
        if (batch.Count == 0) return;

        // Capture the batch and clear the list immediately
        var currentBatch = new List<TInput>(batch);
        batch.Clear();

        // Process asynchronously without blocking the reader
        _ = Task.Run(async () => 
        {
            try 
            {
                await _batchProcessor(currentBatch);
            }
            catch (Exception ex)
            {
                // Log error
            }
        });
    }
}
