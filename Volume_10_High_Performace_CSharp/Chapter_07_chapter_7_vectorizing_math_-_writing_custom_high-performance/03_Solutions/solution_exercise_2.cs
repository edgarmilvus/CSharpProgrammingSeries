
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Numerics;

public static class SoftmaxOps
{
    public static float[][] ComputeSoftmax(float[][] logits)
    {
        // 1. Hardware Check
        if (!Vector.IsHardwareAccelerated)
        {
            return ComputeSoftmaxScalar(logits);
        }

        var result = new float[logits.Length][];
        int vectorCount = Vector<float>.Count;

        for (int b = 0; b < logits.Length; b++)
        {
            float[] row = logits[b];
            int len = row.Length;
            float[] outRow = new float[len];
            result[b] = outRow;

            // 2. Find Max Value (Numerical Stability)
            // We do this to avoid large exponents causing overflow/underflow.
            float maxVal = float.MinValue;
            
            // Vectorized Max Search
            int i = 0;
            Vector<float> maxVec = new Vector<float>(float.MinValue);
            
            for (; i <= len - vectorCount; i += vectorCount)
            {
                Vector<float> vec = new Vector<float>(row, i);
                maxVec = Vector.Max(maxVec, vec);
            }
            
            // Horizontal max of the vector
            maxVal = Vector.Max(maxVal, Vector.Sum(maxVec));

            // Scalar tail for max
            for (; i < len; i++)
            {
                if (row[i] > maxVal) maxVal = row[i];
            }

            // 3. Calculate Exponentials and Sum
            float sumExp = 0.0f;
            
            // Note: Vector<T> does not have a direct Exp intrinsic. 
            // We calculate Exp on scalars inside the loop, but the loop overhead is reduced.
            // We process chunks to keep memory access coherent.
            i = 0;
            for (; i <= len - vectorCount; i += vectorCount)
            {
                Vector<float> vec = new Vector<float>(row, i);
                vec = Vector.Subtract(vec, maxVal); // Subtract max for stability
                
                // Extract to array to apply MathF.Exp (Scalar operation per element)
                // In AVX/AVX2, we would ideally use AVX.Exp intrinsic, but Vector<T> abstraction doesn't expose it.
                float[] temp = new float[vectorCount];
                vec.CopyTo(temp);
                
                for (int k = 0; k < vectorCount; k++)
                {
                    temp[k] = MathF.Exp(temp[k]);
                    sumExp += temp[k];
                }
                
                new Vector<float>(temp).CopyTo(outRow, i);
            }

            // Scalar tail for exp and sum
            for (; i < len; i++)
            {
                float val = MathF.Exp(row[i] - maxVal);
                outRow[i] = val;
                sumExp += val;
            }

            // 4. Normalize (Divide by sum)
            // Vectorized division
            i = 0;
            Vector<float> sumVec = new Vector<float>(sumExp);
            for (; i <= len - vectorCount; i += vectorCount)
            {
                Vector<float> vec = new Vector<float>(outRow, i);
                vec = Vector.Divide(vec, sumVec);
                vec.CopyTo(outRow, i);
            }

            // Scalar tail for normalization
            for (; i < len; i++)
            {
                outRow[i] /= sumExp;
            }
        }

        return result;
    }

    private static float[][] ComputeSoftmaxScalar(float[][] logits)
    {
        var result = new float[logits.Length][];
        for (int b = 0; b < logits.Length; b++)
        {
            float[] row = logits[b];
            float max = float.MinValue;
            foreach (var val in row) if (val > max) max = val;

            float sum = 0;
            float[] exps = new float[row.Length];
            for (int i = 0; i < row.Length; i++)
            {
                exps[i] = MathF.Exp(row[i] - max);
                sum += exps[i];
            }

            result[b] = new float[row.Length];
            for (int i = 0; i < row.Length; i++)
            {
                result[b][i] = exps[i] / sum;
            }
        }
        return result;
    }
}
