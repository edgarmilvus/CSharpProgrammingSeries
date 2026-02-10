
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

# Source File: theory_theoretical_foundations_part4.cs
# Description: Theoretical Foundations
# ==========================================

using System.Threading.Channels;
using System.Threading.Tasks;

// The entry point for high-throughput ingestion.
public class InferenceOrchestrator
{
    private readonly Channel<InferenceRequest> _queue;

    public InferenceOrchestrator()
    {
        // Bounded channel prevents memory overflow under backpressure.
        _queue = Channel.CreateBounded<InferenceRequest>(1000);
    }

    public async Task SubmitRequestAsync(InferenceRequest request)
    {
        // Non-blocking write to the queue.
        await _queue.Writer.WriteAsync(request);
    }

    public async Task ProcessQueueAsync()
    {
        // Worker loop consuming the queue.
        await foreach (var request in _queue.Reader.ReadAllAsync())
        {
            // Delegate to the GPU-bound worker.
            await ProcessInferenceAsync(request);
        }
    }

    private async Task ProcessInferenceAsync(InferenceRequest request)
    {
        // Simulate GPU inference latency.
        await Task.Delay(1000); 
        // Result is pushed to a notification service.
    }
}
