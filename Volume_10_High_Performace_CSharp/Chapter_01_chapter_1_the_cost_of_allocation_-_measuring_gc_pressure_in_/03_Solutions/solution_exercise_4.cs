
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

public class AttentionMechanism
{
    // Threshold for stack allocation (e.g., 128KB). 
    // Stack size is typically 1MB per thread, so we stay well within limits.
    private const int StackAllocThresholdBytes = 128 * 1024; 

    public void ComputeAttention(ReadOnlySpan<float> queries, ReadOnlySpan<float> keys, Span<float> output, int sequenceLength)
    {
        // 1. Validate inputs
        if (output.Length != sequenceLength * sequenceLength)
            throw new ArgumentException("Output buffer size mismatch.");

        // Calculate matrix size: SequenceLength x SequenceLength
        // Assuming float (4 bytes)
        int matrixSize = sequenceLength * sequenceLength;
        int bytesRequired = matrixSize * sizeof(float);

        // 2. Determine allocation strategy
        Span<float> scoreMatrix;
        bool useArrayPool = false;
        float[]? rentedArray = null;

        if (bytesRequired <= StackAllocThresholdBytes)
        {
            // Safe to use stackalloc
            scoreMatrix = stackalloc float[matrixSize];
        }
        else
        {
            // Fallback to ArrayPool
            useArrayPool = true;
            rentedArray = ArrayPool<float>.Shared.Rent(matrixSize);
            scoreMatrix = rentedArray.AsSpan(0, matrixSize);
        }

        try
        {
            // 3. Calculate Scores (Dot Product: Queries * Keys^T)
            // Simplified logic for demonstration
            for (int i = 0; i < sequenceLength; i++)
            {
                for (int j = 0; j < sequenceLength; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < 64; k++) // Assuming embedding dimension is 64
                    {
                        sum += queries[i * 64 + k] * keys[j * 64 + k];
                    }
                    scoreMatrix[i * sequenceLength + j] = sum;
                }
            }

            // 4. Apply Softmax (using logic from Exercise 3)
            // We process each row of the score matrix
            for (int i = 0; i < sequenceLength; i++)
            {
                int rowStart = i * sequenceLength;
                ApplySoftmax(scoreMatrix.Slice(rowStart, sequenceLength));
            }

            // 5. Multiply by Values (Simplified output assignment)
            scoreMatrix.CopyTo(output);
        }
        finally
        {
            // 6. Clean up if ArrayPool was used
            if (useArrayPool && rentedArray != null)
            {
                ArrayPool<float>.Shared.Return(rentedArray);
            }
        }
    }

    private void ApplySoftmax(Span<float> values)
    {
        // Simple scalar softmax for brevity (reuse Exercise 3 logic here in production)
        float max = float.MinValue;
        foreach (var v in values) if (v > max) max = v;

        float sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = MathF.Exp(values[i] - max);
            sum += values[i];
        }

        for (int i = 0; i < values.Length; i++)
        {
            values[i] /= sum;
        }
    }
}
