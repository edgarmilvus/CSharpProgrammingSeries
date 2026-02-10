
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Wrapper to handle rented buffer disposal
public readonly struct PooledMemoryOwner : IDisposable
{
    private readonly byte[] _rentedArray;
    private readonly int _length;

    public PooledMemoryOwner(byte[] array, int length)
    {
        _rentedArray = array;
        _length = length;
    }

    // Implicit conversion to ReadOnlyMemory<byte> for easy consumption
    public static implicit operator ReadOnlyMemory<byte>(PooledMemoryOwner owner)
    {
        return owner._rentedArray.AsMemory(0, owner._length);
    }

    public void Dispose()
    {
        if (_rentedArray != null)
        {
            ArrayPool<byte>.Shared.Return(_rentedArray);
        }
    }
}

public class TokenStreamer
{
    public async IAsyncEnumerable<PooledMemoryOwner> StreamTokensAsync(int bufferSize, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        // Double buffering: We need 2 buffers to ensure the consumer has valid data 
        // while we fill the next one.
        byte[] buffer1 = ArrayPool<byte>.Shared.Rent(bufferSize);
        byte[] buffer2 = ArrayPool<byte>.Shared.Rent(bufferSize);
        
        try
        {
            bool usingBuffer1 = true;
            var random = new Random();

            while (!ct.IsCancellationRequested)
            {
                // Select current buffer to fill
                byte[] currentBuffer = usingBuffer1 ? buffer1 : buffer2;

                // Simulate network read
                random.NextBytes(currentBuffer); // Fill with random data
                await Task.Delay(50, ct);

                // Yield the result wrapped in our disposable struct
                // The consumer MUST dispose this to return the buffer to the pool
                yield return new PooledMemoryOwner(currentBuffer, bufferSize);

                // Swap buffers for the next iteration
                usingBuffer1 = !usingBuffer1;
            }
        }
        finally
        {
            // Ensure buffers are returned if the loop is cancelled or throws
            ArrayPool<byte>.Shared.Return(buffer1);
            ArrayPool<byte>.Shared.Return(buffer2);
        }
    }
}

// Interactive Challenge: TransformAsync Extension
public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<PooledMemoryOwner> TransformAsync(
        this IAsyncEnumerable<PooledMemoryOwner> source,
        Func<ReadOnlyMemory<byte>, Memory<byte>, Task> transformFunc)
    {
        await foreach (var owner in source)
        {
            // Rent a new buffer for the output
            byte[] outputArray = ArrayPool<byte>.Shared.Rent(owner._rentedArray.Length);
            Memory<byte> outputMemory = outputArray.AsMemory(0, owner._rentedArray.Length);

            // Perform the transformation (e.g., XOR)
            await transformFunc(owner, outputMemory);

            // The input owner is disposed here (returning its buffer)
            owner.Dispose();

            // Yield the output owner. The consumer is now responsible for this new buffer.
            yield return new PooledMemoryOwner(outputArray, outputMemory.Length);
        }
    }
}
