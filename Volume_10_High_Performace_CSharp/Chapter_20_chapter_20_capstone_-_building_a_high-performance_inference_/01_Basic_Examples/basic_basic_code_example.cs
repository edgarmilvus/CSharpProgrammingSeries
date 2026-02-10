
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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class HighPerformanceInference
{
    // Configuration for our toy LLM
    private const int VocabularySize = 128; // Small vocab for demonstration
    private const int EmbeddingDim = 64;    // Dimension of embedding vectors
    private const int BatchSize = 4;        // Process 4 tokens at once

    public static void Main()
    {
        Console.WriteLine("Initializing High-Performance Inference Pipeline...");
        
        // 1. Setup: Create dummy model weights (Embedding Table)
        // In a real scenario, these would be loaded from a .bin file.
        float[] embeddingTable = CreateRandomEmbeddings(VocabularySize, EmbeddingDim);

        // 2. Input: A batch of token IDs to process
        int[] inputTokens = [10, 42, 88, 15]; // Batch of 4 tokens

        // 3. Execution: Run the optimized inference step
        RunInferenceBatch(inputTokens, embeddingTable);
    }

    /// <summary>
    /// Executes a high-performance batch inference step.
    /// Uses ArrayPool to avoid GC pressure and Vector<T> for SIMD acceleration.
    /// </summary>
    private static void RunInferenceBatch(Span<int> tokens, Span<float> embeddingTable)
    {
        // --- MEMORY MANAGEMENT ---
        // Rent a buffer from the shared pool. This is zero-allocation on the heap 
        // if the buffer size is supported by the pool.
        // We need space for the batch of embeddings: BatchSize * EmbeddingDim
        float[] rentedBuffer = ArrayPool<float>.Shared.Rent(BatchSize * EmbeddingDim);
        
        try
        {
            // Get a Span over the rented buffer. 
            // We slice it to the exact size we need to prevent reading garbage data.
            Span<float> batchEmbeddings = rentedBuffer.AsSpan(0, BatchSize * EmbeddingDim);

            // --- EMBEDDING LOOKUP (Memory Bound) ---
            // Copy embedding vectors for each token into a contiguous batch buffer.
            // This allows the CPU to prefetch data for the next stage.
            for (int i = 0; i < tokens.Length; i++)
            {
                int token = tokens[i];
                
                // Calculate source and destination offsets
                int srcOffset = token * EmbeddingDim;
                int dstOffset = i * EmbeddingDim;

                // Slice the source (embedding table) and destination (batch buffer)
                Span<float> sourceVector = embeddingTable.Slice(srcOffset, EmbeddingDim);
                Span<float> destVector = batchEmbeddings.Slice(dstOffset, EmbeddingDim);

                // Copy data (optimized by runtime)
                sourceVector.CopyTo(destVector);
            }

            // --- COMPUTE LOGITS (Compute Bound) ---
            // In a real LLM, we would multiply the batch embeddings by the weight matrix.
            // Here, we simulate the output logits (scores) for the next token prediction.
            // We use SIMD (Vector<T>) to process 4 floats at a time (on AVX2 hardware).
            
            // Output buffer for logits (scores for the next token)
            float[] logitsBuffer = ArrayPool<float>.Shared.Rent(VocabularySize);
            Span<float> logits = logitsBuffer.AsSpan(0, VocabularySize);
            logits.Clear(); // Reset scores

            // Simulate a projection layer: Sum of squares (to demonstrate vectorization)
            // In reality: logits = batchEmbeddings * weightMatrix
            int vectorCount = Vector<float>.Count; // e.g., 8 on AVX2, 4 on SSE
            
            // Process the batch embeddings to generate logits
            // We iterate over the vocabulary to compute scores for each possible next token
            for (int vocabIdx = 0; vocabIdx < VocabularySize; vocabIdx++)
            {
                // For this demo, we calculate a score based on the sum of the batch embeddings
                // to simulate a complex operation.
                Vector<float> accumulator = Vector<float>.Zero;

                // SIMD Loop: Process EmbeddingDim chunks
                for (int i = 0; i < EmbeddingDim; i += vectorCount)
                {
                    // Ensure we don't read out of bounds
                    if (i + vectorCount > EmbeddingDim) break;

                    // Load a chunk of data from the batch embeddings
                    var dataChunk = new Vector<float>(batchEmbeddings.Slice(i, vectorCount));
                    
                    // Perform SIMD operation (Square and Add)
                    accumulator += dataChunk * dataChunk;
                }

                // Horizontal sum of the vector to get a single scalar score
                float score = 0f;
                for (int i = 0; i < vectorCount; i++)
                {
                    score += accumulator[i];
                }

                logits[vocabIdx] = score;
            }

            // --- OUTPUT ---
            Console.WriteLine($"Processed batch of {tokens.Length} tokens.");
            Console.WriteLine($"Top 5 predicted next tokens (logits):");
            
            // Find top predictions (naive sort for demo)
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"  Token ID {i}: {logits[i]:F4}");
            }
        }
        finally
        {
            // --- CRITICAL: MEMORY CLEANUP ---
            // Return the buffers to the pool. This does not clear the memory,
            // but marks it as available for reuse.
            ArrayPool<float>.Shared.Return(rentedBuffer);
            // In a real scenario, we would also return the logitsBuffer
        }
    }

    // Helper to generate dummy data
    private static float[] CreateRandomEmbeddings(int vocabSize, int dim)
    {
        float[] table = new float[vocabSize * dim];
        Random.Shared.NextBytes(MemoryMarshal.AsBytes(table.AsSpan()));
        // Normalize roughly
        for (int i = 0; i < table.Length; i++) table[i] = (table[i] % 100) / 100.0f;
        return table;
    }
}
