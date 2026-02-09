
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HighPerformanceAI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Scenario: Processing a batch of token embeddings for a small language model.
            // We have 3 embeddings (Batch Size = 3), each with 8 dimensions (Embedding Size = 8).
            // We need to calculate the Softmax probability distribution for each embedding.
            // Softmax requires: 1. Find Max, 2. Subtract Max (Numerical Stability), 3. Exp, 4. Sum, 5. Normalize.

            int batchSize = 3;
            int embeddingSize = 8;
            float[] inputBatch = new float[batchSize * embeddingSize];

            // Initialize with dummy data representing raw token logits
            // Row 1: Increasing values
            // Row 2: Randomish values
            // Row 3: Decreasing values
            float[] rawData = {
                1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f,
                8.0f, 1.0f, 0.5f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f,
                8.0f, 7.0f, 6.0f, 5.0f, 4.0f, 3.0f, 2.0f, 1.0f
            };
            Array.Copy(rawData, inputBatch, rawData.Length);

            Console.WriteLine("--- Processing Batch Softmax using SIMD ---");

            // Allocate output buffer
            float[] outputBatch = new float[inputBatch.Length];

            // 1. Process using Scalar Method (Baseline)
            float[] outputScalar = new float[inputBatch.Length];
            ProcessBatchSoftmaxScalar(inputBatch, outputScalar, batchSize, embeddingSize);

            // 2. Process using Vector<T> (SIMD) Method
            // Note: Vector<T> is hardware accelerated. It uses the underlying CPU registers (AVX, SSE, etc.).
            ProcessBatchSoftmaxSimd(inputBatch, outputBatch, batchSize, embeddingSize);

            // Display Results
            Console.WriteLine("\nComparison of Results:");
            for (int i = 0; i < inputBatch.Length; i++)
            {
                // Check if results are close (floating point math can have tiny differences)
                bool match = Math.Abs(outputScalar[i] - outputBatch[i]) < 1e-5;
                Console.WriteLine($"Idx {i}: Scalar={outputScalar[i]:F5}, SIMD={outputBatch[i]:F5} {(match ? "✓" : "✗")}");
            }
        }

        /// <summary>
        /// Standard Scalar implementation of Softmax.
        /// Used as a baseline for performance and correctness comparison.
        /// </summary>
        static void ProcessBatchSoftmaxScalar(float[] input, float[] output, int rows, int cols)
        {
            for (int r = 0; r < rows; r++)
            {
                int offset = r * cols;

                // Step 1: Find Max Value for numerical stability
                float max = float.MinValue;
                for (int c = 0; c < cols; c++)
                {
                    if (input[offset + c] > max) max = input[offset + c];
                }

                // Step 2: Exponentiate and Sum
                float sum = 0.0f;
                for (int c = 0; c < cols; c++)
                {
                    float val = (float)Math.Exp(input[offset + c] - max);
                    output[offset + c] = val; // Store temporarily
                    sum += val;
                }

                // Step 3: Normalize
                for (int c = 0; c < cols; c++)
                {
                    output[offset + c] /= sum;
                }
            }
        }

        /// <summary>
        /// High-Performance SIMD implementation of Softmax using Vector<T>.
        /// This method processes multiple float values in parallel using CPU registers.
        /// </summary>
        static void ProcessBatchSoftmaxSimd(float[] input, float[] output, int rows, int cols)
        {
            // Determine the hardware vector size (e.g., 256-bit on AVX2 = 8 floats, 128-bit = 4 floats)
            int vectorSize = Vector<float>.Count;

            for (int r = 0; r < rows; r++)
            {
                int offset = r * cols;

                // --- PHASE 1: Find Max (Vectorized) ---
                // We initialize a vector with the smallest possible float value.
                Vector<float> maxVector = new Vector<float>(float.MinValue);
                
                // Iterate through the data in vector-sized chunks
                int i = 0;
                for (; i <= cols - vectorSize; i += vectorSize)
                {
                    // Load data from memory into a Vector register
                    var dataVector = new Vector<float>(input, offset + i);
                    // Compare and get the maximum values element-wise
                    maxVector = Vector.Max(maxVector, dataVector);
                }

                // Reduce the vector to a single scalar max value
                float max = float.MinValue;
                for (int j = 0; j < vectorSize; j++)
                {
                    if (maxVector[j] > max) max = maxVector[j];
                }

                // Handle the "tail" (remaining elements that didn't fit in a full vector)
                for (; i < cols; i++)
                {
                    if (input[offset + i] > max) max = input[offset + i];
                }

                // --- PHASE 2: Exponentiate and Sum (Vectorized) ---
                Vector<float> sumVector = Vector<float>.Zero;
                Vector<float> maxVectorBroadcast = new Vector<float>(max); // Broadcast max to all slots

                // Reuse index 'i' from previous phase (it's already at the tail start or aligned)
                i = 0;
                for (; i <= cols - vectorSize; i += vectorSize)
                {
                    var dataVector = new Vector<float>(input, offset + i);
                    
                    // Numerical stability: Subtract max
                    var stableVector = dataVector - maxVectorBroadcast;
                    
                    // Hardware Intrinsics: Exp is expensive. 
                    // In strict SIMD, we might map to hardware approximations (e.g., AVX512 ERANGEB), 
                    // but Vector<T> doesn't expose Exp directly. 
                    // We must scalarize the Exp or use a lookup. 
                    // For this example, we calculate Exp on the vector elements to maintain correctness 
                    // while keeping the memory access pattern vectorized.
                    
                    // NOTE: True SIMD 'Exp' requires hardware intrinsics (System.Runtime.Intrinsics.X86.Avx512F.Scalef).
                    // Since we are restricted to Vector<T> portability, we process the elements 
                    // but keep the loop structure aligned.
                    
                    // To strictly use Vector<T> operations, we would only do arithmetic (Add/Sub/Mul).
                    // For Exp, we unroll slightly or process scalars within the vector context.
                    
                    float[] tempVals = new float[vectorSize];
                    for(int k=0; k<vectorSize; k++) 
                    {
                        tempVals[k] = (float)Math.Exp(stableVector[k]);
                    }
                    
                    var expVector = new Vector<float>(tempVals);
                    sumVector += expVector;
                    
                    // Store intermediate exp values into output buffer temporarily
                    expVector.CopyTo(output, offset + i);
                }

                // Reduce Sum Vector to Scalar
                float sum = 0.0f;
                for (int j = 0; j < vectorSize; j++) sum += sumVector[j];

                // Handle Tail for Exp and Sum
                for (; i < cols; i++)
                {
                    float val = (float)Math.Exp(input[offset + i] - max);
                    output[offset + i] = val;
                    sum += val;
                }

                // --- PHASE 3: Normalize (Vectorized) ---
                Vector<float> sumVectorBroadcast = new Vector<float>(sum);
                i = 0;
                for (; i <= cols - vectorSize; i += vectorSize)
                {
                    // Load the stored exp values
                    var expVector = new Vector<float>(output, offset + i);
                    // Divide by sum
                    var resultVector = expVector / sumVectorBroadcast;
                    // Store final result
                    resultVector.CopyTo(output, offset + i);
                }

                // Handle Tail for Normalization
                for (; i < cols; i++)
                {
                    output[offset + i] /= sum;
                }
            }
        }
    }
}
