
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GpuBatching
{
    public class InferenceRequest
    {
        public string Input { get; set; }
        public TaskCompletionSource<string> ResponseSource { get; set; }
    }

    public class BatchInferenceService : BackgroundService
    {
        private readonly ILogger<BatchInferenceService> _logger;
        private readonly Channel<InferenceRequest> _channel;
        // Limit concurrent batches to 1 to prevent GPU OOM, scaling happens via replica count
        private readonly SemaphoreSlim _gpuSemaphore = new SemaphoreSlim(1, 1); 
        private readonly int _maxBatchSize = 8;
        private readonly TimeSpan _batchTimeout = TimeSpan.FromMilliseconds(50);

        public BatchInferenceService(ILogger<BatchInferenceService> logger)
        {
            _logger = logger;
            _channel = Channel.CreateUnbounded<InferenceRequest>();
        }

        public async Task<string> InferAsync(string input)
        {
            var tcs = new TaskCompletionSource<string>();
            await _channel.Writer.WriteAsync(new InferenceRequest { Input = input, ResponseSource = tcs });
            return await tcs.Task;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Batch Inference Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = new List<InferenceRequest>();
                
                try
                {
                    // 1. Wait for the first item to start the batching window
                    var firstItem = await _channel.Reader.ReadAsync(stoppingToken);
                    batch.Add(firstItem);

                    // 2. Fill the batch until timeout or max size
                    var cts = new CancellationTokenSource(_batchTimeout);
                    while (batch.Count < _maxBatchSize)
                    {
                        if (_channel.Reader.TryRead(out var item))
                        {
                            batch.Add(item);
                        }
                        else
                        {
                            // Wait briefly for more items or timeout
                            try
                            {
                                await Task.Delay(1, cts.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                // Timeout reached
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Channel closed
                    break;
                }

                if (batch.Count == 0) continue;

                // 3. Process the batch
                await ProcessBatchAsync(batch, stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(List<InferenceRequest> batch, CancellationToken ct)
        {
            // Acquire GPU lock
            await _gpuSemaphore.WaitAsync(ct);
            try
            {
                _logger.LogInformation($"Executing batch of size: {batch.Count}");
                
                // Simulate GPU inference
                // In a real scenario, inputs would be stacked into a tensor here
                var inputs = batch.Select(b => b.Input).ToList();
                await Task.Delay(50, ct); // Simulate GPU compute time

                // Distribute results
                foreach (var req in batch)
                {
                    // Simulate model output
                    req.ResponseSource.TrySetResult($"Result for: {req.Input}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch inference");
                foreach (var req in batch)
                {
                    req.ResponseSource.TrySetException(ex);
                }
            }
            finally
            {
                _gpuSemaphore.Release();
            }
        }
    }
}
