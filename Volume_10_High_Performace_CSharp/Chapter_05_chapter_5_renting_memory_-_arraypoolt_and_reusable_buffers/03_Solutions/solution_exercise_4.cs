
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public struct PooledBuffer
{
    public byte[] Array { get; }
    public int Id { get; }
    public PooledBuffer(byte[] array, int id) { Array = array; Id = id; }
}

public class ConcurrentTokenBufferPool
{
    private readonly ArrayPool<byte> _pool;
    private int _idCounter = 0;

    public ConcurrentTokenBufferPool(ArrayPool<byte> customPool = null)
    {
        _pool = customPool ?? ArrayPool<byte>.Shared;
    }

    public PooledBuffer Acquire(int size)
    {
        // ArrayPool.Rent is thread-safe
        byte[] array = _pool.Rent(size);
        int id = Interlocked.Increment(ref _idCounter);
        return new PooledBuffer(array, id);
    }

    public void Release(PooledBuffer buffer)
    {
        if (buffer.Array != null)
        {
            _pool.Return(buffer.Array, clearArray: true);
        }
    }
}

public class ConcurrentSimulator
{
    public static async Task RunSimulation()
    {
        // Requirement 5: Custom Pool Configuration
        // Configure custom pool: max array length 8192, 50 arrays per bucket
        var customPool = ArrayPool<byte>.Create(new ArrayPoolPolicy
        {
            MaxArrayLength = 8192,
            MaxArraysPerBucket = 50
        });

        var poolWrapper = new ConcurrentTokenBufferPool(customPool);
        var tasks = new List<Task>();
        var results = new ConcurrentBag<int>();

        // Requirement 5: Burst Scenario (100 threads)
        Console.WriteLine("Starting burst simulation...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                // Acquire
                var buffer = poolWrapper.Acquire(1024);
                
                // Simulate processing
                results.Add(buffer.Id);
                
                // Release
                poolWrapper.Release(buffer);
            }));
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"Processed {results.Count} buffers in {sw.ElapsedMilliseconds}ms.");
    }
}

// Helper class to simulate ArrayPool configuration (Conceptual)
// Note: ArrayPool<T>.Create() takes specific arguments, but for the exercise, 
// we assume a policy object for readability.
public class ArrayPoolPolicy
{
    public int MaxArrayLength { get; set; }
    public int MaxArraysPerBucket { get; set; }
}
