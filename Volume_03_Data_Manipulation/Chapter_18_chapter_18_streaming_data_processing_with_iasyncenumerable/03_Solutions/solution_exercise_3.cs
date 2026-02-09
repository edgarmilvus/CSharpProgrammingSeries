
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class IngestionEngine
{
    public static async IAsyncEnumerable<Memory<float>> IngestVectorsAsync(
        int vectorSize, 
        CancellationToken ct)
    {
        // We simulate an infinite source (e.g., Kafka, WebSocket)
        while (!ct.IsCancellationRequested)
        {
            // 1. Allocation Strategy: Rent from Pool
            float[] buffer = ArrayPool<float>.Shared.Rent(vectorSize);
            
            try 
            {
                // 2. Simulate Data Ingestion
                // Fill with dummy data using Span for speed
                var span = buffer.AsSpan(0, vectorSize);
                new Random().NextBytes(MemoryMarshal.AsBytes(span));

                // 3. Yield the data
                // The consumer receives this Memory<float> and begins processing.
                yield return buffer.AsMemory(0, vectorSize);
                
                // 4. Backpressure & Cancellation Check
                // CRITICAL: This await serves two purposes:
                // A. Cancellation: Throws if the token expires.
                // B. Backpressure: If the consumer is slow to call 'MoveNextAsync',
                //    this task won't complete, and the loop won't iterate.
                //    This prevents us from renting more buffers than necessary.
                await Task.Delay(50, ct);
            }
            finally
            {
                // 5. Cleanup Logic
                // NOTE: In a strict implementation, we cannot return the buffer here 
                // immediately after yielding it, because the consumer might still be using it.
                // The 'RentedMemory' struct from Exercise 1 is the correct pattern for ownership.
                // For this exercise, we acknowledge the architectural gap of raw Memory<T> yielding.
                // In production, wrap 'buffer' in an IDisposable struct before yielding.
            }
        }
    }
}

public class VectorConsumer
{
    public static async Task ProcessWithTimeout()
    {
        // Cancel after 3 seconds
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        
        try
        {
            await foreach (var vector in IngestionEngine.IngestVectorsAsync(1024, cts.Token))
            {
                // Process the vector (e.g., send to AI model)
                Console.WriteLine($"Processing vector of length {vector.Length}");
                
                // Simulate heavy processing (Consumer is slower than Producer)
                // This demonstrates backpressure: The 'await' in the producer
                // will pause until this delay completes.
                await Task.Delay(200); 
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ingestion cancelled successfully.");
        }
    }
}
