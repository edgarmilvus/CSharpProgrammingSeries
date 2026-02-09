
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System.Threading.Channels;
using System.Threading.Tasks;
using System.Collections.Generic;

// Conceptual definition of a message passing system for inference requests
public class InferenceRequest
{
    public string Prompt { get; set; }
    public TaskCompletionSource<string> ResponseSource { get; set; }
}

public class InferenceOrchestrator
{
    private readonly Channel<InferenceRequest> _channel;

    public InferenceOrchestrator(int capacity)
    {
        // Bounded channel creates backpressure when the GPU is saturated
        _channel = Channel.CreateBounded<InferenceRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(string prompt)
    {
        var request = new InferenceRequest { Prompt = prompt };
        
        // Non-blocking write to the channel
        await _channel.Writer.WriteAsync(request);

        // Awaiting the result from the consumer side
        var result = await request.ResponseSource.Task;
        
        // Simulating streaming tokens
        foreach (var token in result.Split(' '))
        {
            yield return token + " ";
        }
    }
}
