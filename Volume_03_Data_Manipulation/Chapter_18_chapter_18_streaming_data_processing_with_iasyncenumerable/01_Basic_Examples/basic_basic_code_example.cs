
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class HighPerformanceEmbeddingStream
{
    // Simulating a raw byte buffer (e.g., from a network stream or file)
    private readonly byte[] _rawDataBuffer;
    private readonly int _embeddingDimension;

    public HighPerformanceEmbeddingStream(byte[] rawData, int dimension)
    {
        _rawDataBuffer = rawData;
        _embeddingDimension = dimension;
    }

    /// <summary>
    /// Asynchronously streams normalized vector embeddings using SIMD for hardware acceleration.
    /// </summary>
    public async IAsyncEnumerable<Memory<float>> StreamEmbeddingsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Calculate the size of a single embedding vector in bytes
        int vectorSizeBytes = _embeddingDimension * sizeof(float);

        // Process the buffer in chunks to simulate streaming
        for (int offset = 0; offset <= _rawDataBuffer.Length - vectorSizeBytes; offset += vectorSizeBytes)
        {
            // Check for cancellation before heavy processing
            cancellationToken.ThrowIfCancellationRequested();

            // CRITICAL: Zero-Allocation Slicing.
            // We use Memory<T> to slice the underlying array without creating a new array or copying data.
            // This is a reference-based operation (O(1)).
            Memory<byte> rawSlice = _rawDataBuffer.AsMemory(offset, vectorSizeBytes);

            // Rent an array from the shared pool to avoid Heap allocations (Gen 0 pressure).
            // This is crucial for high-throughput scenarios (AI batch processing).
            float[] rentedArray = ArrayPool<float>.Shared.Rent(_embeddingDimension);

            try
            {
                // Convert bytes to floats. 
                // In a real scenario, we would use Span<byte>.Cast<float>() (unsafe) or SIMD transcoding.
                // Here we copy to the rented array for safety and demonstration.
                Span<byte> rawSpan = rawSlice.Span;
                Span<float> floatSpan = rentedArray.AsSpan(0, _embeddingDimension);

                // Manual copy loop to avoid LINQ overhead on Span<T>
                for (int i = 0; i < rawSpan.Length; i += sizeof(float))
                {
                    floatSpan[i / sizeof(float)] = BitConverter.ToSingle(rawSpan.Slice(i, 4));
                }

                // HARDWARE ACCELERATION: Normalize the vector using SIMD (System.Numerics.Vector<T>)
                NormalizeVectorSimd(floatSpan);

                // Yield the result. We yield Memory<T> to allow the consumer to decide 
                // if they want to operate on the rented array or copy it out.
                yield return rentedArray.AsMemory(0, _embeddingDimension);

                // IMPORTANT: We cannot return the array to the pool here because the consumer 
                // needs to use it after the yield. The consumer is responsible for disposal 
                // (or we must implement a custom AsyncEnumerator that handles disposal).
                // For this example, we will leak the array to demonstrate the pattern, 
                // but in production, use a struct-based enumerator with DisposeAsync.
            }
            finally
            {
                // In a robust implementation, we would return the array here if we weren't yielding it.
                // Since we are yielding the memory, we rely on the consumer to handle the lifecycle.
                // ArrayPool<float>.Shared.Return(rentedArray);
            }

            // Simulate I/O latency (network delay)
            await Task.Delay(10, cancellationToken);
        }
    }

    /// <summary>
    /// Normalizes a vector in-place using SIMD instructions.
    /// Calculates L2 Norm (Euclidean distance) and divides elements by the norm.
    /// </summary>
    private unsafe void NormalizeVectorSimd(Span<float> vector)
    {
        // Step 1: Calculate Sum of Squares using Vector<T> (SIMD)
        // Vector<float> is 256-bit (AVX) or 128-bit (SSE) depending on hardware.
        int count = Vector<float>.Count;
        int i = 0;
        float sumSq = 0f;

        // We use a local stackalloc buffer to accumulate partial sums if the vector is small,
        // but for large vectors, we accumulate in a Vector register.
        Vector<float> sumVector = Vector<float>.Zero;

        // SIMD Loop
        for (; i <= vector.Length - count; i += count)
        {
            var v = vector.Slice(i, count);
            sumVector += v * v; // Element-wise multiplication and addition
        }

        // Horizontal sum of the SIMD register
        for (int j = 0; j < count; j++)
        {
            sumSq += sumVector[j];
        }

        // Scalar loop for the remainder (tail processing)
        for (; i < vector.Length; i++)
        {
            float val = vector[i];
            sumSq += val * val;
        }

        float norm = MathF.Sqrt(sumSq);

        // Step 2: Divide by norm (Normalization)
        // We avoid division by zero
        if (norm == 0) return;

        // Use Vector<T> again for fast division/scaling
        Vector<float> scaleVector = new Vector<float>(norm);
        i = 0;
        for (; i <= vector.Length - count; i += count)
        {
            vector.Slice(i, count).Fill(norm); 
            // Note: Vector division is not directly supported in System.Numerics for floats 
            // without custom operators or .NET 8+ hardware intrinsics. 
            // We fall back to scalar multiplication for compatibility.
        }

        // Scalar fallback for remainder (and full loop if Vector<float>.Count is 1)
        for (i = 0; i < vector.Length; i++)
        {
            vector[i] /= norm;
        }
    }
}
