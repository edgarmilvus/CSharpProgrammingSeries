
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

// A lightweight struct wrapper to manage the lifetime of a rented array.
// Being a struct, it lives on the stack (or inline in the iterator state machine),
// avoiding heap allocation for the wrapper itself.
public readonly struct RentedMemory<T> : IDisposable
{
    private readonly T[]? _array;
    public Memory<T> Memory { get; }

    public RentedMemory(int minimumLength)
    {
        // Rent a buffer from the shared pool. 
        // This retrieves an array from a bucket of reusable arrays.
        _array = ArrayPool<T>.Shared.Rent(minimumLength);
        
        // Wrap the valid portion of the array in Memory<T>.
        // We slice it to the exact size requested (Rent may return a larger array).
        Memory = _array.AsMemory(0, minimumLength);
    }

    public void Dispose()
    {
        if (_array != null)
        {
            // Return the array to the pool. 
            // CRITICAL: This must happen only after the consumer is done processing.
            ArrayPool<T>.Shared.Return(_array);
        }
    }
}

public class SensorStream
{
    // Simulates an infinite stream of sensor data chunks.
    public static async IAsyncEnumerable<RentedMemory<float>> GenerateSensorDataAsync(int chunkSize, int delayMs)
    {
        var random = new Random();
        
        while (true)
        {
            // 1. ALLOCATION STRATEGY:
            // Instead of 'new float[chunkSize]' (Heap allocation + GC pressure),
            // we rent from ArrayPool. This is O(1) and reuses memory.
            var rented = new RentedMemory<float>(chunkSize);
            
            // 2. PROCESSING:
            // We work directly on the Span of the Memory.
            // Span provides type-safe window into the memory without bounds checks overhead in release builds.
            var span = rented.Memory.Span;
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (float)random.NextDouble();
            }

            // 3. YIELD:
            // Yield the struct. The iterator state machine captures this value.
            yield return rented;

            // 4. BACKPRESSURE / SIMULATION:
            // Await allows the consumer to process the data before the next iteration.
            await Task.Delay(delayMs);
        }
    }
}

// Consumer usage example
public class Processor
{
    public static async Task ConsumeStream()
    {
        var stream = SensorStream.GenerateSensorDataAsync(1024, 100);
        
        int count = 0;
        await foreach (var chunk in stream)
        {
            // Access data via chunk.Memory.Span
            // Process data...
            Console.WriteLine($"Processed chunk with {chunk.Memory.Length} items.");
            
            // IMPORTANT: The struct implements IDisposable.
            // We must call Dispose() to return the array to the pool.
            // If we forget this, the array is effectively leaked from the pool.
            chunk.Dispose();

            if (count++ > 5) break; // Stop infinite stream for demo
        }
    }
}
