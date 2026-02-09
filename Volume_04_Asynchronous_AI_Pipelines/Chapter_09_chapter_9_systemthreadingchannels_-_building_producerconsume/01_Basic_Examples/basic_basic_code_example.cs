
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
using System.Threading.Channels;
using System.Threading.Tasks;

// Simulated AI service that generates responses (simulating an LLM call)
public class SimpleAIService
{
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Simulate network latency and processing time
        await Task.Delay(500); 
        return $"AI Response to: '{prompt}'";
    }
}

// The core pipeline using System.Threading.Channels
public class SimpleAIPipeline
{
    private readonly Channel<string> _channel;
    private readonly SimpleAIService _aiService;

    public SimpleAIPipeline()
    {
        // 1. Create a bounded channel to handle backpressure
        // Capacity = 5: If the producer fills 5 items and the consumer is slow,
        // the producer will wait (backpressure) instead of crashing memory.
        var options = new BoundedChannelOptions(capacity: 5)
        {
            // Drop the oldest item if full (optional strategy), or wait (default).
            // Here we use Wait mode for strict ordering and reliability.
            FullMode = BoundedChannelFullMode.Wait
        };

        _channel = Channel.CreateBounded<string>(options);
        _aiService = new SimpleAIService();
    }

    // PRODUCER: Simulates receiving user requests (e.g., from a web API endpoint)
    public async Task EnqueueRequestAsync(string prompt)
    {
        // WriteAsync respects backpressure. If the channel is full, it waits.
        await _channel.Writer.WriteAsync(prompt);
        Console.WriteLine($"[Producer] Enqueued: '{prompt}'");
    }

    // CONSUMER: Simulates a background worker processing the queue
    public async Task ProcessQueueAsync()
    {
        // ReadAsync returns a ValueTask<Optional<T>>. 
        // We iterate until the channel is completed and empty.
        await foreach (var prompt in _channel.Reader.ReadAllAsync())
        {
            // Process the item
            var response = await _aiService.GenerateResponseAsync(prompt);
            Console.WriteLine($"[Consumer] Processed: {response}");
        }
    }

    // Signal that no more items will be produced
    public void CompleteProducer() => _channel.Writer.Complete();
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var pipeline = new SimpleAIPipeline();

        // Start the consumer on a background thread (fire and forget)
        var consumerTask = pipeline.ProcessQueueAsync();

        // Simulate a burst of incoming requests (The Producer)
        var prompts = new[]
        {
            "Explain quantum computing",
            "Write a haiku about code",
            "Summarize the history of the internet"
        };

        foreach (var prompt in prompts)
        {
            await pipeline.EnqueueRequestAsync(prompt);
        }

        // Signal that we are done producing items
        pipeline.CompleteProducer();

        // Wait for the consumer to finish processing all items in the channel
        await consumerTask;

        Console.WriteLine("Pipeline processing complete.");
    }
}
