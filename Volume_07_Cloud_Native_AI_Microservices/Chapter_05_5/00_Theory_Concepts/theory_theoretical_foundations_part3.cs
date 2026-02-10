
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System.Threading.Channels;

public class BatchingService
{
    private readonly Channel<InferenceRequest> _channel;
    private readonly ModelRunner _modelRunner;

    public BatchingService(ModelRunner modelRunner)
    {
        // Create a bounded channel to prevent memory exhaustion.
        _channel = Channel.CreateBounded<InferenceRequest>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait // Apply backpressure
        });
        _modelRunner = modelRunner;
    }

    public async ValueTask EnqueueAsync(InferenceRequest request)
    {
        await _channel.Writer.WriteAsync(request);
    }

    public async Task ProcessBatchesAsync(CancellationToken stoppingToken)
    {
        await foreach (var batch in ReadBatchesAsync(stoppingToken))
        {
            // Execute the batch on the GPU
            await _modelRunner.ExecuteBatchAsync(batch);
        }
    }

    private async IAsyncEnumerable<IReadOnlyList<InferenceRequest>> ReadBatchesAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var batch = new List<InferenceRequest>(capacity: 32);
        var timer = Task.Delay(TimeSpan.FromMilliseconds(10), ct);

        await foreach (var request in _channel.Reader.ReadAllAsync(ct))
        {
            batch.Add(request);

            // Condition 1: Batch is full
            if (batch.Count >= 32)
            {
                yield return batch;
                batch = new List<InferenceRequest>(32);
                timer = Task.Delay(TimeSpan.FromMilliseconds(10), ct);
            }
            // Condition 2: Timeout (latency optimization)
            else if (batch.Count > 0 && await Task.WhenAny(timer, Task.CompletedTask) == timer)
            {
                yield return batch;
                batch = new List<InferenceRequest>(32);
                timer = Task.Delay(TimeSpan.FromMilliseconds(10), ct);
            }
        }
    }
}
