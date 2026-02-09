
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System.Buffers;

public void HighPerformanceVectorAdd(Span<float> a, Span<float> b)
{
    // We need a temporary buffer to store the result before applying activation function.
    // We rent it from the pool.
    float[] rentedArray = ArrayPool<float>.Shared.Rent(a.Length);
    
    try
    {
        // Create a Span over the rented array
        Span<float> result = rentedArray.AsSpan(0, a.Length);
        
        // Perform the addition
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }

        // Apply activation (e.g., ReLU) directly on the result Span
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] < 0) result[i] = 0;
        }
        
        // Now we might pass 'result' to the next stage of the pipeline
        SendToNextLayer(result);
    }
    finally
    {
        // CRITICAL: Always return the array to the pool.
        // If we forget this, the pool runs out of arrays, and we start allocating again.
        ArrayPool<float>.Shared.Return(rentedArray);
    }
}
