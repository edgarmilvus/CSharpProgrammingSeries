
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
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Wrapper to handle ownership and prevent memory leaks
public readonly struct PooledBuffer : IDisposable
{
    private readonly float[] _array;
    public Memory<float> Memory { get; }

    public PooledBuffer(int size)
    {
        _array = ArrayPool<float>.Shared.Rent(size);
        Memory = _array.AsMemory(0, size);
    }

    public void Dispose()
    {
        // Return the array to the pool for reuse
        ArrayPool<float>.Shared.Return(_array);
    }
}

public class OptimizedStream
{
    public static async IAsyncEnumerable<PooledBuffer> GetOptimizedDataAsync()
    {
        const int size = 1024;
        
        while (true)
        {
            // 1. Allocation: Rent from Pool (Stack allocation for the struct wrapper)
            var buffer = new PooledBuffer(size);
            var span = buffer.Memory.Span;

            try
            {
                // 2. Simulation: Fill with data
                // Using a simple loop for simulation (avoids overhead of Random for this example)
                for (int i = 0; i < size; i++) span[i] = i;

                // 3. Vectorization: Hardware Accelerated Math
                // Apply scaling factor of 2.0f using SIMD
                ApplyScaleInPlace(span, 2.0f);

                // 4. Yield: Return the buffer wrapper
                // The consumer must call .Dispose() on this struct after processing
                yield return buffer;
            }
            catch
            {
                // Ensure disposal on error
                buffer.Dispose();
                throw;
            }
            
            // Simulate data arrival rate
            await Task.Delay(10);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void ApplyScaleInPlace(Span<float> data, float scale)
    {
        var scaleVec = new Vector<float>(scale);
        int i = 0;
        int width = Vector<float>.Count;

        // Vectorized loop: Process multiple floats per instruction
        for (; i <= data.Length - width; i += width)
        {
            var vec = new Vector<float>(data.Slice(i, width));
            var result = vec * scaleVec;
            result.CopyTo(data.Slice(i, width));
        }

        // Scalar tail: Handle remaining elements
        for (; i < data.Length; i++)
        {
            data[i] *= scale;
        }
    }
}
