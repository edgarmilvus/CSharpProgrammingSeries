
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
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace HighPerformanceLLM
{
    // Real-world context: We are building the core "Logits Processor" for a local LLM.
    // When the model generates a response, it outputs raw numbers (logits) for every possible token in the vocabulary.
    // We must convert these to probabilities (Softmax) and pick the best token. 
    // This must happen thousands of times per second with minimal memory allocation to avoid garbage collection pauses.

    public static class Program
    {
        // Simulation constants for our "Toy" LLM
        const int VocabularySize = 128; // A small subset of ASCII tokens for demonstration
        const int BatchSize = 4;        // Process 4 tokens in parallel (Batched Processing)

        public static void Main()
        {
            Console.WriteLine("--- High-Performance LLM Inference Pipeline (CPU-Optimized) ---\n");

            // 1. Setup: Allocate memory using ArrayPool to avoid Heap Allocations.
            // We need space for: Raw Logits (Model Output), Probabilities (Softmax Output), and a Buffer for Top-K selection.
            float[] logitsBuffer = ArrayPool<float>.Shared.Rent(VocabularySize * BatchSize);
            float[] probsBuffer = ArrayPool<float>.Shared.Rent(VocabularySize * BatchSize);
            int[] topTokens = ArrayPool<int>.Shared.Rent(BatchSize);

            try
            {
                // 2. Mock Data Generation: Simulate the raw output from the Neural Network.
                // In a real scenario, this comes from Matrix Multiplication (covered in Ch 19).
                SimulateModelOutput(logitsBuffer, BatchSize, VocabularySize);

                // 3. Vectorized Softmax: Apply Softmax function using SIMD (Vector<T>) for high throughput.
                // This calculates exp(x) / sum(exp(x)) for all logits in parallel hardware registers.
                ProcessBatch(logitsBuffer, probsBuffer, BatchSize, VocabularySize);

                // 4. Greedy Decoding: Find the token with the highest probability for each item in the batch.
                // We use unsafe pointers for raw speed on the inner loop.
                DecodeBatch(probsBuffer, topTokens, BatchSize, VocabularySize);

                // 5. Output: Map token IDs back to characters (ASCII for this demo).
                Console.WriteLine("Generated Tokens:");
                for (int i = 0; i < BatchSize; i++)
                {
                    Console.WriteLine($"Batch {i}: Token ID {topTokens[i]} -> '{(char)topTokens[i]}'");
                }
            }
            finally
            {
                // 6. Memory Safety: Return arrays to the pool immediately.
                // Critical for long-running inference loops to keep GC pressure at zero.
                ArrayPool<float>.Shared.Return(logitsBuffer);
                ArrayPool<float>.Shared.Return(probsBuffer);
                ArrayPool<int>.Shared.Return(topTokens);
            }
        }

        /// <summary>
        /// Generates mock logits. In reality, this runs the Transformer model layers.
        /// </summary>
        static void SimulateModelOutput(float[] buffer, int batch, int vocab)
        {
            Random rand = new Random(42); // Deterministic random for reproducibility
            for (int b = 0; b < batch; b++)
            {
                int offset = b * vocab;
                for (int i = 0; i < vocab; i++)
                {
                    // Random values between -5.0 and 5.0 to simulate unnormalized logits
                    buffer[offset + i] = (float)(rand.NextDouble() * 10.0 - 5.0);
                }
            }
        }

        /// <summary>
        /// Calculates Softmax probabilities using SIMD (Single Instruction, Multiple Data).
        /// This processes multiple floats simultaneously in CPU vector registers.
        /// </summary>
        static void ProcessBatch(float[] logits, float[] probs, int batch, int vocab)
        {
            // We process the batch in chunks of Vector<float>.Count (usually 4, 8, or 16 depending on AVX support)
            // This is the core optimization: processing 4+ numbers in one CPU cycle.
            int vectorSize = Vector<float>.Count;
            
            for (int b = 0; b < batch; b++)
            {
                int offset = b * vocab;
                
                // --- STEP A: Find Max Logit (for numerical stability) ---
                // Standard Softmax requires subtracting the max value to prevent overflow.
                float maxVal = float.MinValue;
                
                // Pointer arithmetic for raw speed
                unsafe
                {
                    float* ptr = (float*)(&logits[offset]);
                    for (int i = 0; i < vocab; i++)
                    {
                        if (ptr[i] > maxVal) maxVal = ptr[i];
                    }
                }

                // --- STEP B: Calculate Exponentials and Sum ---
                float sum = 0.0f;
                
                // We loop in increments of the Vector width
                int i = 0;
                unsafe
                {
                    float* lPtr = (float*)(&logits[offset]);
                    float* pPtr = (float*)(&probs[offset]);

                    // SIMD Loop: Processes 4 floats at once (if Vector<float>.Count == 4)
                    for (; i <= vocab - vectorSize; i += vectorSize)
                    {
                        // Load current chunk of logits
                        Vector<float> vec = new Vector<float>(lPtr + i);
                        
                        // Subtract max
                        vec = Vector.Subtract(vec, new Vector<float>(maxVal));
                        
                        // Exp approximation (hardware intrinsics often accelerate this, but we do standard MathF here)
                        // Note: Vectorized MathF.Exp is not directly available in .NET standard libraries without custom intrinsics,
                        // so we operate on the vector structure but might fall back to scalar for the Exp in pure managed code 
                        // unless we use Avx.Exp. For this educational example, we simulate the vectorized flow.
                        // In a real production app, we would use `Avx.Exp` or similar.
                        
                        for (int k = 0; k < vectorSize; k++)
                        {
                            float val = vec[k];
                            float expVal = MathF.Exp(val);
                            pPtr[i + k] = expVal;
                            sum += expVal;
                        }
                    }

                    // --- STEP C: Scalar Cleanup (Tail End) ---
                    // Handle any remaining elements that didn't fit into a full Vector
                    for (; i < vocab; i++)
                    {
                        float val = lPtr[i] - maxVal;
                        float expVal = MathF.Exp(val);
                        pPtr[i] = expVal;
                        sum += expVal;
                    }

                    // --- STEP D: Normalization ---
                    // Divide every probability by the sum
                    Vector<float> sumVec = new Vector<float>(sum);
                    i = 0;
                    for (; i <= vocab - vectorSize; i += vectorSize)
                    {
                        Vector<float> pVec = new Vector<float>(pPtr + i);
                        pVec = Vector.Divide(pVec, sumVec);
                        
                        // Store back to memory
                        pVec.CopyTo(pPtr + i);
                    }

                    // Scalar cleanup for normalization
                    for (; i < vocab; i++)
                    {
                        pPtr[i] /= sum;
                    }
                }
            }
        }

        /// <summary>
        /// Decodes the batch by finding the index of the maximum probability (Greedy Search).
        /// Uses unsafe pointers to avoid array bounds checking overhead.
        /// </summary>
        static void DecodeBatch(float[] probs, int[] results, int batch, int vocab)
        {
            for (int b = 0; b < batch; b++)
            {
                int offset = b * vocab;
                int bestToken = 0;
                float maxProb = 0.0f;

                unsafe
                {
                    float* ptr = (float*)(&probs[offset]);
                    
                    // Single pass to find max
                    for (int i = 0; i < vocab; i++)
                    {
                        if (ptr[i] > maxProb)
                        {
                            maxProb = ptr[i];
                            bestToken = i;
                        }
                    }
                }

                results[b] = bestToken;
            }
        }
    }
}
