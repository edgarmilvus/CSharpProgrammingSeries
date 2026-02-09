
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

public class EmbeddingDotProduct
{
    public static void ComputeDotProducts(
        ReadOnlySpan<float> embeddingMatrix, 
        ReadOnlySpan<float> queryVector, 
        ReadOnlySpan<int> tokenIds, 
        Span<float> results)
    {
        if (results.Length != tokenIds.Length)
            throw new ArgumentException("Results span length must match tokenIds length.");

        int embeddingDim = queryVector.Length;
        int vectorWidth = Vector<float>.Count;

        // Outer Loop: Iterate through each token in the batch
        for (int b = 0; b < tokenIds.Length; b++)
        {
            int tokenId = tokenIds[b];
            
            // Calculate the offset in the flattened embedding matrix
            int offset = tokenId * embeddingDim;
            
            // Get the slice for the specific embedding
            ReadOnlySpan<float> embeddingSlice = embeddingMatrix.Slice(offset, embeddingDim);

            float dotProduct = 0.0f;
            int i = 0;

            // Inner Loop: Vectorized Dot Product
            // We use Vector.Dot for maximum efficiency (hardware acceleration)
            for (; i <= embeddingDim - vectorWidth; i += vectorWidth)
            {
                var embedVec = new Vector<float>(embeddingSlice.Slice(i, vectorWidth));
                var queryVec = new Vector<float>(queryVector.Slice(i, vectorWidth));
                
                // Vector.Dot returns a scalar float
                dotProduct += Vector.Dot(embedVec, queryVec);
            }

            // Tail Handling: Scalar loop for remaining elements
            for (; i < embeddingDim; i++)
            {
                dotProduct += embeddingSlice[i] * queryVector[i];
            }

            results[b] = dotProduct;
        }
    }
}
