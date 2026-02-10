
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
using System.Runtime.CompilerServices;

public static class AttentionOptimizer
{
    public static float[] CalculateAttentionScores(float[] query, float[][] keyMatrix)
    {
        int dim = query.Length;
        int seqLen = keyMatrix.Length;
        float[] scores = new float[seqLen];
        int vectorCount = Vector<float>.Count;

        // Optimization Strategy: 
        // We iterate over the sequence length (rows of keyMatrix).
        // For each row, we compute the dot product with the query vector.
        // Accessing keyMatrix[i] sequentially improves cache locality (Row-Major order).

        for (int i = 0; i < seqLen; i++)
        {
            float[] keyRow = keyMatrix[i];
            float sum = 0;

            // Vectorized Dot Product
            int j = 0;
            
            // Using unsafe pointers to avoid bounds checking in the inner loop
            unsafe
            {
                fixed (float* qPtr = query)
                fixed (float* kPtr = keyRow)
                {
                    // Process in chunks of Vector<float>.Count
                    for (; j <= dim - vectorCount; j += vectorCount)
                    {
                        Vector<float> qVec = Unsafe.Read<Vector<float>>(qPtr + j);
                        Vector<float> kVec = Unsafe.Read<Vector<float>>(kPtr + j);
                        
                        // Multiply and Add (Accumulate)
                        // Note: Vector.Dot is available, but explicit Multiply + Add 
                        // demonstrates FMA (Fused Multiply-Add) potential if hardware supports it.
                        Vector<float> prod = qVec * kVec;
                        sum += Vector.Sum(prod);
                    }

                    // Scalar tail for remaining dimensions
                    for (; j < dim; j++)
                    {
                        sum += qPtr[j] * kPtr[j];
                    }
                }
            }
            scores[i] = sum;
        }

        return scores;
    }

    // Interactive Element: Calculation of iterations for BERT (768 dim)
    public static void AnalyzeIterations()
    {
        int dim = 768;
        int vectorWidth = Vector<float>.Count; // e.g., 8 (AVX) or 16 (AVX512)
        
        int scalarIterations = dim;
        int vectorIterations = (int)Math.Ceiling((double)dim / vectorWidth);
        
        Console.WriteLine($"Dimension: {dim}");
        Console.WriteLine($"Vector Width: {vectorWidth}");
        Console.WriteLine($"Scalar Loop Iterations: {scalarIterations}");
        Console.WriteLine($"Vectorized Loop Iterations: {vectorIterations}");
        Console.WriteLine($"Reduction Factor: {(float)scalarIterations / vectorIterations:F2}x");
    }
}
