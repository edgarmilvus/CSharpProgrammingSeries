
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class HighPerformanceSoftmax
{
    /// <summary>
    /// Computes Softmax efficiently using SIMD for exponentiation and reduction.
    /// </summary>
    public static void ComputeSoftmax(Span<float> logits, Span<float> probabilities)
    {
        if (logits.Length != probabilities.Length)
            throw new ArgumentException("Spans must be of equal length.");

        int length = logits.Length;
        if (length == 0) return;

        // 1. Find Max Logit (for numerical stability)
        float maxLogit = float.MinValue;
        
        // Scalar scan for max (SIMD reduction is complex without AVX512, scalar is acceptable for this step)
        // Or use Vector256<T>.ReduceMax if available in newer runtimes, otherwise loop:
        for (int i = 0; i < length; i++)
        {
            if (logits[i] > maxLogit && !float.IsInfinity(logits[i]))
                maxLogit = logits[i];
        }

        // 2. Subtract max and Calculate Exponent
        // We need to handle the tail elements.
        int vectorSize = Vector256<float>.Count;
        int i = 0;
        
        // Accumulator for sum of exponents
        Vector256<float> sumVector = Vector256<float>.Zero;

        unsafe
        {
            fixed (float* logitsPtr = logits)
            fixed (float* probsPtr = probabilities)
            {
                // Process vectors
                if (Avx.IsSupported)
                {
                    Vector256<float> maxVec = Vector256.Create(maxLogit);
                    
                    while (i <= length - vectorSize)
                    {
                        // Load logits
                        Vector256<float> vec = Avx.LoadVector256(logitsPtr + i);
                        
                        // Subtract max
                        vec = Avx.Subtract(vec, maxVec);

                        // Calculate Exp using software approximation or hardware support
                        // Note: AVX doesn't have a native Exp instruction. 
                        // We use a vectorized approximation or call MathF.Exp per element for accuracy.
                        // For true high-performance, we often approximate Exp (e.g., using polynomial expansion).
                        // Here we demonstrate the structure using scalar Exp inside the vector loop for correctness.
                        
                        float* tempStack = stackalloc float[8];
                        Avx.Store(tempStack, vec);
                        
                        for (int k = 0; k < 8; k++)
                        {
                            // Handle edge cases
                            if (float.IsNaN(tempStack[k]) || float.IsNegativeInfinity(tempStack[k]))
                                tempStack[k] = 0;
                            else
                                tempStack[k] = MathF.Exp(tempStack[k]);
                        }

                        Vector256<float> expVec = Avx.LoadVector256(tempStack);
                        
                        // Accumulate sum
                        sumVector = Avx.Add(sumVector, expVec);
                        
                        // Store intermediate exponentials (optional if we want to keep them)
                        // For standard softmax, we usually store them to normalize later.
                        Avx.Store(probsPtr + i, expVec);

                        i += vectorSize;
                    }
                }

                // Handle tail elements (Scalar)
                float sum = 0f;
                for (; i < length; i++)
                {
                    float val = logits[i] - maxLogit;
                    float expVal = MathF.Exp(val);
                    probabilities[i] = expVal;
                    sum += expVal;
                }

                // 3. Sum the vector accumulator (Horizontal Sum)
                if (Avx.IsSupported)
                {
                    // Horizontal add is tricky in AVX2. 
                    // We extract the lower 128 bits, sum, then extract upper, sum, add them.
                    Vector128<float> lower = Vector256.GetLowerHalf(sumVector);
                    Vector128<float> upper = Vector256.GetUpperHalf(sumVector);
                    Vector128<float> sum128 = Avx.Add(lower, upper);
                    
                    // Shuffle and add to get the final scalar sum
                    // (a, b, c, d) -> (c, d, c, d) -> add -> (a+c, b+d, c+c, d+d)
                    Vector128<float> shuffled = Avx.Shuffle(sum128, sum128, 0b_01_00_01_00); 
                    sum128 = Avx.Add(sum128, shuffled);
                    shuffled = Avx.Shuffle(sum128, sum128, 0b_10_11_10_11);
                    sum128 = Avx.Add(sum128, shuffled);
                    
                    sum += sum128.ToScalar();
                }

                // 4. Normalize
                // Reset pointer for normalization
                i = 0;
                if (Avx.IsSupported)
                {
                    Vector256<float> sumVecNorm = Vector256.Create(sum);
                    while (i <= length - vectorSize)
                    {
                        Vector256<float> probs = Avx.LoadVector256(probsPtr + i);
                        probs = Avx.Divide(probs, sumVecNorm);
                        Avx.Store(probsPtr + i, probs);
                        i += vectorSize;
                    }
                }

                // Normalize tail
                for (; i < length; i++)
                {
                    probabilities[i] /= sum;
                }
            }
        }
    }
}
