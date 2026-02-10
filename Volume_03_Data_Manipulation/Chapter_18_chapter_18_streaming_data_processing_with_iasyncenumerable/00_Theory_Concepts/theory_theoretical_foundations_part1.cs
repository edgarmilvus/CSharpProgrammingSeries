
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System;

public class EmbeddingProcessor
{
    public void ProcessBatch(float[] tensorBuffer, int vectorsCount, int dimension)
    {
        // This is a zero-allocation operation.
        // We are not creating new arrays. We are creating 'views' into the existing buffer.
        
        for (int i = 0; i < vectorsCount; i++)
        {
            // Calculate the start index for this vector
            int startIndex = i * dimension;
            
            // Create a Span representing just this vector
            // No memory is allocated on the Heap here.
            Span<float> currentVector = tensorBuffer.AsSpan(startIndex, dimension);
            
            // We can now pass this 'currentVector' to other methods
            // that expect Span<float>, and it costs us nothing.
            NormalizeVector(currentVector);
        }
    }

    private void NormalizeVector(Span<float> vector)
    {
        // We can read and write to the Span directly
        // This modifies the original tensorBuffer!
        for(int i = 0; i < vector.Length; i++)
        {
            vector[i] = vector[i] * 0.5f; 
        }
    }
}
