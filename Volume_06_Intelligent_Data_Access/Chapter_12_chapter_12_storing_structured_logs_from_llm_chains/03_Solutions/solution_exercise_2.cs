
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

using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;

// 1. Service Interface and Implementation
public interface ILogBufferService
{
    void EnqueueLog(Step step, bool isCritical = false);
    Task FlushAsync();
}

public class LogBufferService : ILogBufferService, IDisposable
{
    private readonly ConcurrentQueue<Step> _buffer = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _batchSize = 50;
    private readonly Timer _flushTimer;

    public LogBufferService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        // Auto-flush every 30 seconds if buffer isn't full
        _flushTimer = new Timer(async _ => await FlushAsync(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public void EnqueueLog(Step step, bool isCritical = false)
    {
        _buffer.Enqueue(step);

        // Critical Path Logic
        if (isCritical)
        {
            // We must flush immediately. 
            // Note: We fire-and-forget the flush, but we must handle potential overlaps.
            // We use a separate Task.Run to avoid blocking the caller.
            Task.Run(async () => await FlushWithPriorityAsync());
            return;
        }

        // Standard Batch Logic
        if (_buffer.Count >= _batchSize)
        {
            // Fire and forget, but ensure we don't block the enqueue thread
            Task.Run(async () => await FlushAsync());
        }
    }

    // Standard flush with retry logic
    public async Task FlushAsync()
    {
        // Acquire semaphore to prevent concurrent flushes from different threads/timers
        await _semaphore.WaitAsync();
        try
        {
            if (_buffer.IsEmpty) return;

            // Dequeue batch
            var batch = new List<Step>();
            while (batch.Count < _batchSize && _buffer.TryDequeue(out var item))
            {
                batch.Add(item);
            }

            if (batch.Count == 0) return;

            await SaveBatchWithRetry(batch);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Priority flush handles race conditions by waiting for the semaphore
    private async Task FlushWithPriorityAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Flush whatever is in the buffer first
            if (!_buffer.IsEmpty)
            {
                var batch = new List<Step>();
                while (_buffer.TryDequeue(out var item))
                {
                    batch.Add(item);
                }
                await SaveBatchWithRetry(batch);
            }
            // Note: The critical item itself was already added to the queue in EnqueueLog.
            // If we wanted to save ONLY the critical item immediately, we would need a separate list.
            // But here we treat the critical item as the trigger to flush the whole queue immediately.
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveBatchWithRetry(List<Step> batch)
    {
        int retryCount = 0;
        while (retryCount < 3)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<LlmLogContext>();
                    await context.Steps.AddRangeAsync(batch);
                    await context.SaveChangesAsync();
                    return; // Success
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                // Exponential backoff
                await Task.Delay(100 * (int)Math.Pow(2, retryCount));
            }
        }
    }

    public void Dispose()
    {
        _flushTimer?.Dispose();
        _semaphore?.Dispose();
    }
}

// 2. Registration (Example usage in Program.cs)
// services.AddSingleton<ILogBufferService, LogBufferService>();
// services.AddDbContext<LlmLogContext>(options => options.UseSqlServer(...)); // Scoped
