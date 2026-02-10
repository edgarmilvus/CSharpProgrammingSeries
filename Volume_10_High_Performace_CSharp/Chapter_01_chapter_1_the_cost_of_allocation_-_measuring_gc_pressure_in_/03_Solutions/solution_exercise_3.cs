
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
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class OptimizedSoftmax
{
    // Naive scalar implementation for benchmarking
    public static void SoftmaxScalar(Span<float> input)
    {
        float max = float.MinValue;
        foreach (var val in input) if (val > max) max = val;

        float sum = 0;
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = MathF.Exp(input[i] - max);
            sum += input[i];
        }

        for (int i = 0; i < input.Length; i++)
        {
            input[i] /= sum;
        }
    }

    // Vectorized implementation using AVX (Hardware Intrinsics)
    public static void SoftmaxAvx(Span<float> input)
    {
        // 1. Find Max (Vectorized)
        int i = 0;
        int length = input.Length;
        int vectorWidth = Vector256<float>.Count; // Usually 8 floats on AVX2
        
        // Initialize max vector with float.MinValue
        Vector256<float> maxVector = Vector256.Create(float.MinValue);

        // Process aligned blocks
        int lastBlockIndex = length - vectorWidth;
        for (; i <= lastBlockIndex; i += vectorWidth)
        {
            Vector256<float> data = Avx.LoadVector256(&input[i]);
            maxVector = Avx.Max(maxVector, data);
        }

        // Reduce maxVector to a single scalar max value
        float max = maxVector.GetElement(0);
        for (int k = 1; k < vectorWidth; k++) 
        {
            float val = maxVector.GetElement(k);
            if (val > max) max = val;
        }

        // Handle remainder for Max calculation
        for (; i < length; i++) 
        {
            if (input[i] > max) max = input[i];
        }

        // 2. Calculate Exponentials and Sum (Vectorized)
        // Note: AVX does not have a native Exp instruction. We use a polynomial approximation 
        // or rely on the scalar MathF.Exp for the vector lanes. 
        // For true high-performance SIMD Exp, we would use Avx.Exp, but standard .NET intrinsics 
        // do not expose a hardware Exp instruction directly. 
        // We will simulate vectorized math by processing spans and using Vector<T> where possible,
        // or simply unroll the scalar Exp inside the vector loop for this exercise context.
        
        // Re-loop for exponentiation and summing
        Vector256<float> sumVector = Vector256<float>.Zero;
        i = 0;
        
        for (; i <= lastBlockIndex; i += vectorWidth)
        {
            Vector256<float> data = Avx.LoadVector256(&input[i]);
            Vector256<float> shifted = Avx.Subtract(data, Vector256.Create(max));
            
            // Since we lack a direct AVX Exp, we calculate scalar Exp for each lane
            // In a real AVX-512 context or with manual polynomial implementation, this would be vectorized.
            // For this exercise, we demonstrate the structure of zero-allocation processing.
            for (int j = 0; j < vectorWidth; j++)
            {
                float val = shifted.GetElement(j);
                float expVal = MathF.Exp(val);
                input[i + j] = expVal; // Store back
                // Accumulate to scalar sum for now to keep code standard-library compliant
                // (Or we could vectorize the addition)
            }
        }

        // Fallback for remainder
        float sum = 0;
        for (; i < length; i++)
        {
            input[i] = MathF.Exp(input[i] - max);
            sum += input[i];
        }

        // 3. Normalize (Vectorized)
        Vector256<float> sumVec = Vector256.Create(sum);
        i = 0;
        for (; i <= lastBlockIndex; i += vectorWidth)
        {
            Vector256<float> data = Avx.LoadVector256(&input[i]);
            Vector256<float> result = Avx.Divide(data, sumVec);
            Avx.Store(&input[i], result);
        }

        // Fallback for remainder
        for (; i < length; i++)
        {
            input[i] /= sum;
        }
    }
}
