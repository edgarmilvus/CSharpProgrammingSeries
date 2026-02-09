
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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

// Simulating an AI model that generates text tokens rapidly
public class FastAiModel
{
    private readonly Random _random = new();
    
    // Generates tokens faster than a typical UI can render comfortably
    public async IAsyncEnumerable<string> GenerateTokensAsync(
        string prompt, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        string[] tokens = ["The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog.", " This", " is", " a", " demonstration", " of", " backpressure."];
        
        for (int i = 0; i < 100; i++) // Generate 100 tokens rapidly
        {
            ct.ThrowIfCancellationRequested();
            
            // Simulate variable generation speed (some tokens take longer)
            int delay = _random.Next(10, 50); // 10-50ms per token
            await Task.Delay(delay, ct);
            
            string token = tokens[_random.Next(tokens.Length)];
            yield return token;
        }
    }
}

// The UI layer that renders tokens (simulated as slow)
public class SlowUiRenderer
{
    public async Task RenderTokenAsync(string token)
    {
        // Simulate UI rendering latency (e.g., DOM updates, layout calculations)
        // This is intentionally slow to demonstrate backpressure
        await Task.Delay(100); // 100ms per token (slower than AI generation)
        
        // In a real UI, this would update the DOM, Canvas, etc.
        Console.Write(token);
    }
}

// The backpressure manager using a bounded Channel (Producer-Consumer pattern)
public class BackpressureManager
{
    private readonly Channel<string> _buffer;
    private readonly FastAiModel _aiModel;
    private readonly SlowUiRenderer _uiRenderer;
    private readonly CancellationTokenSource _cts;

    public BackpressureManager(int bufferSize = 5)
    {
        // Create a bounded channel with a capacity of 5 tokens
        // This is the core of backpressure: when full, the producer (AI) waits
        _buffer = Channel.CreateBounded<string>(new BoundedChannelOptions(bufferSize)
        {
            // DropOldest: When full, remove the oldest item to make room for new ones
            // Useful for real-time streaming where recent data is more important
            FullMode = BoundedChannelFullMode.DropOldest,
            
            // SingleReader/SingleWriter optimizations for performance
            SingleReader = true,
            SingleWriter = true
        });

        _aiModel = new FastAiModel();
        _uiRenderer = new SlowUiRenderer();
        _cts = new CancellationTokenSource();
    }

    // Producer: AI generates tokens and writes to the buffer
    private async Task ProduceAsync()
    {
        try
        {
            await foreach (var token in _aiModel.GenerateTokensAsync("Hello", _cts.Token))
            {
                // WriteAsync will block if the channel is full (backpressure applied)
                await _buffer.Writer.WriteAsync(token, _cts.Token);
                
                // Log buffer status for demonstration
                Console.WriteLine($"\n[Producer] Wrote '{token}'. Buffer count: {_buffer.Reader.Count}");
            }
            
            // Signal that no more data will be written
            _buffer.Writer.Complete();
        }
        catch (OperationCanceledException)
        {
            _buffer.Writer.Complete(new OperationCanceledException());
        }
    }

    // Consumer: UI reads from buffer and renders
    private async Task ConsumeAsync()
    {
        try
        {
            // ReadAsync will return immediately if data is available, 
            // or wait if the channel is empty (waiting for producer)
            await foreach (var token in _buffer.Reader.ReadAllAsync(_cts.Token))
            {
                await _uiRenderer.RenderTokenAsync(token);
                
                // Log buffer status for demonstration
                Console.WriteLine($" [Consumer] Rendered '{token}'. Buffer count: {_buffer.Reader.Count}");
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully
        }
    }

    // Start both producer and consumer concurrently
    public async Task RunAsync()
    {
        Console.WriteLine("Starting Backpressure Demo...");
        Console.WriteLine($"Buffer capacity: 5 tokens");
        Console.WriteLine($"AI generation: ~20-50ms per token");
        Console.WriteLine($"UI rendering: 100ms per token");
        Console.WriteLine("================================\n");

        // Run producer and consumer in parallel
        var producerTask = ProduceAsync();
        var consumerTask = ConsumeAsync();

        // Wait for both to complete
        await Task.WhenAll(producerTask, consumerTask);

        Console.WriteLine("\n\nDemo completed.");
    }

    // Graceful shutdown
    public void Stop()
    {
        _cts.Cancel();
        _buffer.Writer.Complete();
    }
}

// Main program entry point
public class Program
{
    public static async Task Main(string[] args)
    {
        // Create the backpressure manager with a buffer of 5 tokens
        var manager = new BackpressureManager(bufferSize: 5);
        
        // Run the demo
        await manager.RunAsync();
        
        // Wait for user input to exit
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
