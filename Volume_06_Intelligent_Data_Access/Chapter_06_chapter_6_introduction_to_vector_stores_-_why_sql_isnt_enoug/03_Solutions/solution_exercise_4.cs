
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

// Project: VectorStore.Local
// File: InMemoryVectorStore.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VectorMath; // Depends on Exercise 3

namespace VectorStore.Local
{
    public class SearchResult<T>
    {
        public T Data { get; set; }
        public float Similarity { get; set; }
    }

    public class InMemoryVectorStore<T>
    {
        // Thread-safe storage for Flat Index
        private readonly ConcurrentBag<(float[] Vector, T Data)> _store = new();

        public Task AddAsync(float[] vector, T data)
        {
            _store.Add((vector, data));
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<SearchResult<T>> SearchAsync(float[] queryVector, int topK)
        {
            var results = new List<SearchResult<T>>();

            // Thread-safe read operation
            foreach (var item in _store)
            {
                // Calculate similarity using the extension method from Exercise 3
                float similarity = queryVector.CosineSimilarity(item.Vector);

                results.Add(new SearchResult<T>
                {
                    Data = item.Data,
                    Similarity = similarity
                });
            }

            // Sort and take top K
            var topResults = results
                .OrderByDescending(r => r.Similarity)
                .Take(topK);

            foreach (var result in topResults)
            {
                // Simulate async streaming
                await Task.Yield(); 
                yield return result;
            }
        }
    }

    // --- Interactive Challenge: Quantized Index Simulation ---

    public class QuantizedVectorStore<T>
    {
        // Stores quantized vectors (byte) to save memory
        private readonly ConcurrentBag<(byte[] QuantizedVector, T Data)> _store = new();
        private const float MinValue = -1.0f;
        private const float MaxValue = 1.0f;
        private const float Range = MaxValue - MinValue;

        public Task AddAsync(float[] vector, T data)
        {
            var quantized = Compress(vector);
            _store.Add((quantized, data));
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<SearchResult<T>> SearchAsync(float[] queryVector, int topK)
        {
            // We must quantize the query vector to match the stored format
            var quantizedQuery = Compress(queryVector);
            
            var results = new List<SearchResult<T>>();

            foreach (var item in _store)
            {
                // 1. Decompress stored vector (CPU cost)
                var decompressedVector = Decompress(item.QuantizedVector);
                
                // 2. Calculate similarity (CPU cost)
                // Note: Accuracy loss occurs here due to quantization.
                float similarity = queryVector.CosineSimilarity(decompressedVector);

                results.Add(new SearchResult<T>
                {
                    Data = item.Data,
                    Similarity = similarity
                });
            }

            var topResults = results.OrderByDescending(r => r.Similarity).Take(topK);

            foreach (var result in topResults)
            {
                await Task.Yield();
                yield return result;
            }
        }

        // Simple linear quantization: Map [-1, 1] -> [0, 255]
        private byte[] Compress(float[] vector)
        {
            var bytes = new byte[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                // Clamp values to range
                float val = Math.Clamp(vector[i], MinValue, MaxValue);
                // Normalize to 0-1, then scale to 0-255
                bytes[i] = (byte)((val - MinValue) / Range * 255);
            }
            return bytes;
        }

        private float[] Decompress(byte[] bytes)
        {
            var vector = new float[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                // Map 0-255 back to [-1, 1]
                vector[i] = (bytes[i] / 255.0f) * Range + MinValue;
            }
            return vector;
        }
    }
}
