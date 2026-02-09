
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Numerics;
using System.Runtime.InteropServices;

public static class EmbeddingCalculator
{
    // Dimensions: VocabularySize=1000, Dimension=128
    public static void ComputeEmbeddings(
        ReadOnlyMemory<float> weights, 
        ReadOnlyMemory<int> tokenIds, 
        Memory<float> output, 
        int dimension)
    {
        var weightsSpan = weights.Span;
        var tokenIdsSpan = tokenIds.Span;
        var outputSpan = output.Span;

        int vectorSize = Vector<float>.Count;
        
        // Iterate over each token in the batch
        for (int i = 0; i < tokenIdsSpan.Length; i++)
        {
            int tokenId = tokenIdsSpan[i];
            
            // Calculate offsets
            int weightOffset = tokenId * dimension;
            int outputOffset = i * dimension;

            // Get slices for this specific token's embedding
            ReadOnlySpan<float> currentWeights = weightsSpan.Slice(weightOffset, dimension);
            Span<float> currentOutput = outputSpan.Slice(outputOffset, dimension);

            // Cast to Vector<float> for SIMD processing
            // We use MemoryMarshal.Cast to reinterpret the span as vectors
            var weightVectors = MemoryMarshal.Cast<float, Vector<float>>(currentWeights);
            var outputVectors = MemoryMarshal.Cast<float, Vector<float>>(currentOutput);

            int j = 0;
            // Process aligned chunks
            for (; j < weightVectors.Length; j++)
            {
                // Assuming a simple operation: Output = Input * Weight (or just copy weights for this demo)
                // In a real scenario, this might be accumulation or specific math.
                // Here we simulate: output[j] = weights[j] * (tokenId + 1.0f) for demonstration
                outputVectors[j] = weightVectors[j] * (tokenId + 1.0f);
            }

            // Handle tail elements (remainder)
            for (int k = j * vectorSize; k < dimension; k++)
            {
                currentOutput[k] = currentWeights[k] * (tokenId + 1.0f);
            }
        }
    }
}
