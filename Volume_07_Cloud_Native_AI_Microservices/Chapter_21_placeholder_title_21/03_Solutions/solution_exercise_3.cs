
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

// Services/BatchingService.cs
using System.Threading.Channels;
using InferenceService.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace InferenceService.Services;

public class BatchingService : BackgroundService
{
    private readonly Channel<InferenceTask> _channel;
    private readonly ModelLoaderService _modelService;
    private readonly ILogger<BatchingService> _logger;
    private const int MaxBatchSize = 4;

    public BatchingService(ModelLoaderService modelService, ILogger<BatchingService> logger)
    {
        _channel = Channel.CreateUnbounded<InferenceTask>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });
        _modelService = modelService;
        _logger = logger;
    }

    public async ValueTask SubmitAsync(DenseTensor<float> tensor, TaskCompletionSource<float[]> tcs)
    {
        var task = new InferenceTask { Tensor = tensor, CompletionSource = tcs };
        await _channel.Writer.WriteAsync(task);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var batch in ReadBatchesAsync(stoppingToken))
        {
            await ProcessBatchAsync(batch, stoppingToken);
        }
    }

    private async IAsyncEnumerable<IReadOnlyList<InferenceTask>> ReadBatchesAsync([EnumeratorCancellation] CancellationToken ct)
    {
        var batch = new List<InferenceTask>(MaxBatchSize);
        
        while (!ct.IsCancellationRequested)
        {
            // Wait for the first item
            if (!await _channel.Reader.WaitToReadAsync(ct)) yield break;

            // Try to fill the batch
            while (batch.Count < MaxBatchSize && _channel.Reader.TryRead(out var item))
            {
                batch.Add(item);
            }

            if (batch.Count > 0)
            {
                yield return batch;
                batch.Clear();
            }
        }
    }

    private async Task ProcessBatchAsync(IReadOnlyList<InferenceTask> batch, CancellationToken ct)
    {
        try
        {
            // Note: In a real scenario, we would stack tensors into a single batch tensor here.
            // For simplicity, we process sequentially but under a single lock/synchronization context
            // to simulate the constraint of limited GPU memory access.
            
            var results = new float[batch.Count][];
            
            // Thread safety: The InferenceSession is thread-safe for concurrent Run() calls,
            // but we lock here to strictly adhere to "processing up to 4 images concurrently" 
            // without overwhelming the underlying native execution provider if it has limitations.
            lock (_modelService.Session) 
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    var inputName = _modelService.Session.InputMetadata.Keys.First();
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(inputName, batch[i].Tensor)
                    };

                    using var resultsContainer = _modelService.Session.Run(inputs);
                    var output = resultsContainer.First().AsTensor<float>().ToArray();
                    results[i] = output;
                }
            }

            for (int i = 0; i < batch.Count; i++)
            {
                batch[i].CompletionSource.TrySetResult(results[i]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            foreach (var item in batch)
            {
                item.CompletionSource.TrySetException(ex);
            }
        }
    }
}

public class InferenceTask
{
    public DenseTensor<float> Tensor { get; set; }
    public TaskCompletionSource<float[]> CompletionSource { get; set; }
}
