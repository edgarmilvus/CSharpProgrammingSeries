
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Numerics; // Required for SIMD operations (Vector<T>)
using System.Runtime.CompilerServices; // Required for MethodImplOptions.AggressiveInlining

namespace HighPerformanceAITokenProcessing
{
    /// <summary>
    /// Represents a single token in our AI pipeline with its metadata.
    /// This struct is small (16 bytes on 64-bit) and perfect for stack allocation.
    /// </summary>
    public readonly struct Token
    {
        public readonly int Id;
        public readonly float LogProbability;
        public readonly short EmbeddingIndex;

        public Token(int id, float logProb, short embeddingIndex)
        {
            Id = id;
            LogProbability = logProb;
            EmbeddingIndex = embeddingIndex;
        }

        /// <summary>
        /// A readonly method guarantees that the state of 'this' cannot be modified.
        /// This allows the JIT to optimize heavily and avoids defensive copies when
        /// calling methods on readonly structs.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetScaledProbability(float temperature)
        {
            // Math operations are pure and do not modify state.
            return LogProbability / temperature;
        }
    }

    /// <summary>
    /// Represents a tensor slice using Span<T>. This avoids heap allocations
    /// for the underlying data storage. It acts as a view over memory.
    /// </summary>
    public ref struct TensorSlice
    {
        private readonly Span<float> _data;
        public readonly int Rows;
        public readonly int Columns;

        public TensorSlice(Span<float> data, int rows, int columns)
        {
            _data = data;
            Rows = rows;
            Columns = columns;
        }

        public ref float this[int row, int col]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _data[row * Columns + col];
        }
    }

    public class Program
    {
        // Configuration constants
        private const int TokenCount = 1024; // Simulating a batch of tokens
        private const int EmbeddingDim = 128; // Vector size for embeddings
        private const float Temperature = 0.7f; // Softmax temperature

        public static void Main()
        {
            Console.WriteLine("Initializing High-Performance Token Processor...");
            
            // 1. ALLOCATION: Stackalloc for performance-critical small buffers.
            // We avoid the GC by allocating on the stack. This is safe because
            // the tokens array does not escape the method scope.
            Span<Token> tokens = stackalloc Token[TokenCount];
            
            // 2. SIMULATION: Populate data.
            // We use a standard loop. No LINQ to adhere to constraints.
            Random rand = new Random(42);
            for (int i = 0; i < TokenCount; i++)
            {
                tokens[i] = new Token(
                    id: i,
                    logProb: (float)rand.NextDouble() * -5.0f, // Negative log probs
                    embeddingIndex: (short)(i % 10)
                );
            }

            // 3. MEMORY POOLING: Simulate a reusable buffer for tensor data.
            // In a real app, this would come from ArrayPool<T>.Shared.
            // We use a fixed-size array here for simplicity.
            float[] embeddingBuffer = new float[TokenCount * EmbeddingDim];
            Span<float> embeddingSpan = embeddingBuffer.AsSpan();

            // Initialize tensor data
            for (int i = 0; i < embeddingSpan.Length; i++)
            {
                embeddingSpan[i] = (float)rand.NextDouble();
            }

            // Create a view over the memory (Zero-copy)
            var embeddings = new TensorSlice(embeddingSpan, TokenCount, EmbeddingDim);

            Console.WriteLine($"Processing {TokenCount} tokens with SIMD and 'in' parameters...");

            // 4. OPTIMIZATION: Process tokens using 'in' parameters.
            // Passing the struct by reference ('in') avoids copying the struct data
            // (16 bytes per token * 1024 = 16KB of memory copy saved per call).
            // Since 'Token' is readonly, we guarantee safety.
            float totalLogProb = ProcessTokensWithInParameter(in tokens, in embeddings, Temperature);

            Console.WriteLine($"Batch Processing Complete. Total Scaled LogProb: {totalLogProb:F4}");
            
            // 5. SIMD OPTIMIZATION: Vectorized calculation.
            // We demonstrate how 'in' parameters work with Vector<T> for SIMD.
            float vectorizedResult = CalculateVectorizedSum(in embeddingSpan);
            Console.WriteLine($"Vectorized Embedding Sum (SIMD): {vectorizedResult:F4}");
        }

        /// <summary>
        /// Processes tokens using 'in' parameters to avoid defensive copying.
        /// </summary>
        /// <param name="tokens">Read-only reference to the token span.</param>
        /// <param name="embeddings">Read-only reference to the tensor slice.</param>
        /// <param name="temp">The temperature parameter.</param>
        /// <returns>The sum of scaled probabilities.</returns>
        private static float ProcessTokensWithInParameter(
            in Span<Token> tokens, 
            in TensorSlice embeddings, 
            float temp)
        {
            float sum = 0.0f;

            // We iterate through the tokens.
            // Because 'tokens' is passed as 'in', we are not copying the Span reference 
            // (which is cheap, but the pattern is consistent).
            // The real benefit is when passing structs, but we apply the pattern here too.
            for (int i = 0; i < tokens.Length; i++)
            {
                // Accessing the token via the read-only reference.
                // The JIT knows 'tokens[i]' cannot change the underlying data 
                // (because Token is readonly), allowing safe optimization.
                ref readonly Token t = ref tokens[i];

                // Calculate scaled probability
                float scaled = t.GetScaledProbability(temp);
                
                // Accessing the embedding data via the TensorSlice.
                // The 'in TensorSlice' ensures we don't copy the struct (Rows, Cols, Span).
                // We access the data directly.
                float embeddingValue = embeddings[t.EmbeddingIndex, 0];

                // Simple logic: Weight the embedding value by the probability
                sum += scaled * embeddingValue;
            }

            return sum;
        }

        /// <summary>
        /// Demonstrates SIMD optimization using Vector<T> with an 'in' Span parameter.
        /// </summary>
        private static float CalculateVectorizedSum(in Span<float> data)
        {
            float sum = 0.0f;
            int i = 0;
            
            // Determine the Vector<T> width (depends on hardware, e.g., 256-bit on AVX2).
            int vectorWidth = Vector<float>.Count;

            // Create a vector accumulator
            Vector<float> vectorSum = Vector<float>.Zero;

            // SIMD Loop: Process multiple floats simultaneously
            for (; i <= data.Length - vectorWidth; i += vectorWidth)
            {
                // Load data into a vector. 
                // 'new Vector<float>(data, i)' reads from the memory location.
                // Since 'data' is passed as 'in', we ensure we are reading from 
                // the correct source without accidental modification.
                Vector<float> v = new Vector<float>(data, i);
                vectorSum += v;
            }

            // Horizontal reduction: Sum the elements within the vector
            for (int j = 0; j < vectorWidth; j++)
            {
                sum += vectorSum[j];
            }

            // Process the remaining elements (tail)
            for (; i < data.Length; i++)
            {
                sum += data[i];
            }

            return sum;
        }
    }
}
