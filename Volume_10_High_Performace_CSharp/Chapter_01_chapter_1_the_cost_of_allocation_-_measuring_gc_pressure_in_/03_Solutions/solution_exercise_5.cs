
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
using System.Threading.Tasks;

public struct TokenPipeline
{
    // Simulated Embedding Table (External data, not allocated by the pipeline)
    private readonly float[] _embeddingTable;
    private readonly int _embeddingDim;

    public TokenPipeline(float[] embeddingTable, int embeddingDim)
    {
        _embeddingTable = embeddingTable;
        _embeddingDim = embeddingDim;
    }

    // Pipeline entry point
    public void ProcessBatch(Span<int> inputTokens, Span<float> outputLogits, Span<float> tempBuffer)
    {
        // 1. Embedding Lookup
        // Input: Tokens (Integers)
        // Output: Dense Vectors (Floats) written to tempBuffer
        Embed(inputTokens, tempBuffer);

        // 2. Linear Transformation (Matrix * Vector)
        // Input: tempBuffer (Dense Vectors)
        // Output: Overwrites tempBuffer with transformed values
        LinearTransform(tempBuffer);

        // 3. Activation (ReLU)
        // Input: tempBuffer
        // Output: Overwrites tempBuffer
        ReLU(tempBuffer);

        // 4. Copy to final output (if separate buffer required)
        tempBuffer.CopyTo(outputLogits);
    }

    private void Embed(ReadOnlySpan<int> tokens, Span<float> denseOutput)
    {
        int dim = _embeddingDim;
        for (int i = 0; i < tokens.Length; i++)
        {
            int token = tokens[i];
            // Bounds check omitted for brevity, but Span handles it
            int offset = token * dim;
            
            // Copy embedding vector to output
            for (int j = 0; j < dim; j++)
            {
                denseOutput[i * dim + j] = _embeddingTable[offset + j];
            }
        }
    }

    private void LinearTransform(Span<float> data)
    {
        // Simulating a matrix multiplication: data = Weights * data
        // In-place modification to avoid allocation
        for (int i = 0; i < data.Length; i++)
        {
            // Dummy math for demonstration
            data[i] = data[i] * 0.5f + 0.1f; 
        }
    }

    private void ReLU(Span<float> data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] < 0) data[i] = 0;
        }
    }
}

public class PipelineOrchestrator
{
    public void RunBatchProcessing()
    {
        int batchSize = 1000;
        int seqLen = 50;
        int embedDim = 64;
        
        // Allocate inputs (simulating external data)
        int[] tokens = new int[batchSize * seqLen];
        float[] results = new float[batchSize * seqLen * embedDim];
        
        // Pre-calculate required buffer size
        int singleSeqBufferSize = seqLen * embedDim;
        
        // Initialize Pipeline
        // Assume embedding table is a large array (could be ArrayPool or static)
        float[] embeddingTable = new float[1000 * embedDim]; 
        var pipeline = new TokenPipeline(embeddingTable, embedDim);

        // PARALLEL PROCESSING
        // We process each sequence in the batch in parallel.
        // To avoid thread-safe allocation issues (e.g., false sharing on shared buffers),
        // we rely on the loop partitioning and local stack allocation or distinct spans.
        
        Parallel.For(0, batchSize, i =>
        {
            // CRITICAL: Each thread gets its own stack-allocated buffer.
            // This is safe because stack memory is per-thread.
            // We use 'stackalloc' for the temporary processing buffer.
            // Note: If singleSeqBufferSize is large, this might overflow stack.
            // For this exercise, we assume it fits (e.g., 50 * 64 * 4 bytes = 12.8KB).
            
            Span<float> threadLocalBuffer = stackalloc float[singleSeqBufferSize];
            
            // Get the slice for this specific sequence
            Span<int> tokenSlice = new Span<int>(tokens, i * seqLen, seqLen);
            Span<float> resultSlice = new Span<float>(results, i * singleSeqBufferSize, singleSeqBufferSize);

            // Execute pipeline
            pipeline.ProcessBatch(tokenSlice, resultSlice, threadLocalBuffer);
        });
    }
}
