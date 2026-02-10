
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

public class BatchedInferenceService : BackgroundService
{
    private readonly Channel<string> _requestChannel;
    private readonly InferenceSession _session;
    private const int MaxBatchSize = 32;
    private const int MaxWaitMs = 50;

    public BatchedInferenceService()
    {
        // Bounded channel to prevent memory overflow under high load
        _requestChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        
        // Initialize ONNX Session (omitted for brevity)
    }

    // Called by API Controller
    public async Task<string> EnqueueRequest(string input)
    {
        await _requestChannel.Writer.WriteAsync(input);
        // In a real scenario, we'd return a Task<string> that completes when processing finishes
        return "Accepted";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<string>();
        var timer = Task.Delay(MaxWaitMs, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Try to read from channel without waiting indefinitely
            if (_requestChannel.Reader.TryRead(out var item))
            {
                batch.Add(item);
            }

            // Trigger batch if limits reached
            if (batch.Count >= MaxBatchSize)
            {
                await ProcessBatch(batch);
                batch.Clear();
                timer = Task.Delay(MaxWaitMs, stoppingToken);
                continue;
            }

            // Check if timer expired
            if (timer.IsCompleted)
            {
                if (batch.Count > 0)
                {
                    await ProcessBatch(batch);
                    batch.Clear();
                }
                timer = Task.Delay(MaxWaitMs, stoppingToken);
            }
            
            // Small yield to prevent tight spinning
            await Task.Yield();
        }
    }

    private async Task ProcessBatch(List<string> batch)
    {
        // 1. Construct Batched Tensor
        // Shape: [BatchSize, SequenceLength]
        // Logic to tokenize and pad inputs would go here
        var inputTensor = new DenseTensor<long>(new long[batch.Count * 512], new[] { batch.Count, 512 });
        
        // 2. Run Inference
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", inputTensor) };
        using var results = await Task.Run(() => _session.Run(inputs)); // Run on thread pool
        
        // 3. Demultiplex Results
        var outputs = results.First().AsTensor<long>();
        // Logic to map tensor rows back to individual requests
        // (e.g., via continuation tasks or callbacks)
    }
}
