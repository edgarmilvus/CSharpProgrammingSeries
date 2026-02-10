
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;

public class EmbeddingNormalizer
{
    public static void CalculateNormalizedEmbedding(ReadOnlySpan<float> input, Span<float> output)
    {
        if (input.Length != output.Length)
            throw new ArgumentException("Input and output lengths must match.");

        // Safety check: Stack allocation is dangerous for large sizes.
        // We limit to 128 floats (512 bytes) to be safe on the default stack size (1MB).
        const int MaxStackSize = 128;
        if (input.Length > MaxStackSize)
            throw new ArgumentOutOfRangeException("Input too large for stack allocation.");

        // 1. Allocate on the stack.
        // 'tempBuffer' is a Span<float> pointing to stack memory.
        // This memory is reclaimed automatically when the method returns.
        Span<float> tempBuffer = stackalloc float[input.Length];

        double sumSquares = 0;

        // 2. Perform intermediate calculations
        for (int i = 0; i < input.Length; i++)
        {
            // Subtract mean (assuming mean is 0.5f for this example)
            float diff = input[i] - 0.5f;
            
            // Store in stack buffer
            tempBuffer[i] = diff * diff;
            
            // Accumulate sum (scalar sum is often faster than vectorized sum for small N)
            sumSquares += tempBuffer[i];
        }

        // 3. Normalize
        if (sumSquares == 0) return; // Avoid division by zero

        double norm = Math.Sqrt(sumSquares);
        
        for (int i = 0; i < output.Length; i++)
        {
            // Write to output (which might be on heap or stack, doesn't matter to us)
            output[i] = (float)(tempBuffer[i] / norm);
        }
        
        // 'tempBuffer' memory is automatically reclaimed here when the stack frame pops.
        // No GC involved.
    }
}
