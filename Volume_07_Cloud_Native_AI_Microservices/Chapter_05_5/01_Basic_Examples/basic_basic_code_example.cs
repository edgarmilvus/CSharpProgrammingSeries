
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNativeAI.Microservices.Inference
{
    /// <summary>
    /// Represents a single inference request containing input data and a completion source
    /// to signal the result back to the caller.
    /// </summary>
    /// <typeparam name="TInput">The type of input data (e.g., a string or tensor).</typeparam>
    /// <typeparam name="TOutput">The type of output data (e.g., a prediction or embedding).</typeparam>
    public class InferenceRequest<TInput, TOutput>
    {
        public TInput Input { get; init; }
        public TaskCompletionSource<TOutput> CompletionSource { get; init; }
    }

    /// <summary>
    /// Simulates an AI Model Inference Engine that processes requests in batches.
    /// In a real scenario, this would wrap a TensorFlow.NET or Torch.NET model.
    /// </summary>
    public class InferenceEngine<TInput, TOutput>
    {
        private readonly BlockingCollection<InferenceRequest<TInput, TOutput>> _requestQueue;
        private readonly int _batchSize;
        private readonly int _batchTimeoutMs;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _processingTask;

        // Simulate a model that takes time to run
        private readonly Func<List<TInput>, List<TOutput>> _modelInferenceFunc;

        public InferenceEngine(int batchSize, int batchTimeoutMs, Func<List<TInput>, List<TOutput>> modelInferenceFunc)
        {
            _batchSize = batchSize;
            _batchTimeoutMs = batchTimeoutMs;
            _modelInferenceFunc = modelInferenceFunc;
            
            // Bounded capacity prevents memory exhaustion if the queue grows too large
            _requestQueue = new BlockingCollection<InferenceRequest<TInput, TOutput>>(boundedCapacity: 1000);
            _cancellation = new CancellationTokenSource();

            // Start the background processor
            _processingTask = Task.Run(() => ProcessBatchesAsync(_cancellation.Token));
        }

        /// <summary>
        /// Enqueues a request to be processed.
        /// </summary>
        public Task<TOutput> InferAsync(TInput input)
        {
            var tcs = new TaskCompletionSource<TOutput>();
            var request = new InferenceRequest<TInput, TOutput>
            {
                Input = input,
                CompletionSource = tcs
            };

            // Non-blocking addition to the queue
            _requestQueue.Add(request);
            
            return tcs.Task;
        }

        /// <summary>
        /// Background loop that collects requests and executes them in batches.
        /// </summary>
        private async Task ProcessBatchesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 1. Wait for the first request to arrive (blocking)
                if (!_requestQueue.TryTake(out var firstRequest, TimeSpan.FromMilliseconds(100), token))
                {
                    continue;
                }

                var batch = new List<InferenceRequest<TInput, TOutput>> { firstRequest };
                var sw = Stopwatch.StartNew();

                // 2. Try to fill the batch until size limit or timeout is reached
                while (batch.Count < _batchSize && sw.ElapsedMilliseconds < _batchTimeoutMs)
                {
                    // Non-blocking peek/take with short timeout
                    if (_requestQueue.TryTake(out var nextRequest, TimeSpan.FromMilliseconds(10)))
                    {
                        batch.Add(nextRequest);
                    }
                }

                // 3. Execute the batch inference
                await ExecuteBatchAsync(batch);
            }
        }

        private async Task ExecuteBatchAsync(List<InferenceRequest<TInput, TOutput>> batch)
        {
            try
            {
                // Extract inputs
                var inputs = batch.Select(r => r.Input).ToList();
                
                // Simulate network/IO latency or heavy GPU computation
                await Task.Delay(50); 

                // Run the model inference (CPU/GPU bound)
                // In a real container, this is where the GPU memory is utilized.
                var results = _modelInferenceFunc(inputs);

                // Map results back to individual requests
                for (int i = 0; i < batch.Count; i++)
                {
                    batch[i].CompletionSource.TrySetResult(results[i]);
                }
            }
            catch (Exception ex)
            {
                // Propagate errors to all requests in the batch
                foreach (var req in batch)
                {
                    req.CompletionSource.TrySetException(ex);
                }
            }
        }

        public void Stop()
        {
            _cancellation.Cancel();
            _requestQueue.CompleteAdding();
            _processingTask.Wait();
        }
    }

    // --- Main Program to Demonstrate Usage ---
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Cloud-Native Inference Batching Demo...");

            // 1. Initialize the Engine with a batch size of 4 and a timeout of 200ms
            // This simulates a model that processes 4 items at once, or waits max 200ms.
            var engine = new InferenceEngine<string, string>(
                batchSize: 4,
                batchTimeoutMs: 200,
                modelInferenceFunc: inputs =>
                {
                    // Simulate AI Model processing (e.g., Sentiment Analysis)
                    Console.WriteLine($"[Model] Processing Batch of {inputs.Count} inputs...");
                    return inputs.Select(i => $"Processed: {i}").ToList();
                });

            // 2. Simulate multiple clients sending requests concurrently
            var tasks = new List<Task>();
            for (int i = 1; i <= 10; i++)
            {
                int id = i;
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    Console.WriteLine($"[Client {id}] Sending request...");
                    
                    var result = await engine.InferAsync($"Input_{id}");
                    
                    sw.Stop();
                    Console.WriteLine($"[Client {id}] Received: {result} in {sw.ElapsedMilliseconds}ms");
                }));
                
                // Small delay to stagger requests slightly
                await Task.Delay(20); 
            }

            await Task.WhenAll(tasks);
            engine.Stop();
            
            Console.WriteLine("Demo Complete.");
        }
    }
}
