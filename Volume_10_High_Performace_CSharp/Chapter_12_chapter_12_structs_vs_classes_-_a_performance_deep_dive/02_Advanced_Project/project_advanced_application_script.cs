
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace HighPerformanceAITokenProcessing
{
    // ========================================================================
    // PROBLEM CONTEXT: High-Throughput Token Processing for AI Inference
    // ========================================================================
    // Real-World Scenario: An AI inference engine processes millions of tokens per second.
    // Each token requires:
    // 1. ID lookup (int)
    // 2. Probability score (float)
    // 3. Logit bias adjustment (float)
    // 4. Timestamp for latency tracking (long)
    //
    // Performance Bottleneck: Allocating millions of small objects on the Heap causes
    // frequent Garbage Collection (GC) pauses, freezing the inference pipeline.
    //
    // Solution: Use Structs (value types) stored in a contiguous array.
    // Benefits:
    //   - Zero Heap Allocations during processing.
    //   - CPU Cache Locality (Sequential memory access).
    //   - SIMD Vectorization (Process 4-8 tokens in parallel).
    // ========================================================================

    // ------------------------------------------------------------------------
    // SECTION 1: The Performance-Critical Data Structure (Struct vs Class)
    // ------------------------------------------------------------------------

    // BAD PRACTICE: Class (Heap Allocated)
    // If we used this for millions of tokens, the GC would have to traverse
    // millions of random memory references, causing "Stop-the-World" pauses.
    public class TokenClass
    {
        public int Id;
        public float Probability;
        public float LogitBias;
        public long Timestamp;
    }

    // GOOD PRACTICE: Struct (Stack or Inline Allocated)
    // 1. Value Type: Stored contiguously in memory (e.g., inside an array).
    // 2. Layout: Explicit layout ensures no padding, maximizing density.
    // 3. Size: 24 bytes (4 + 4 + 4 + 12 padding? No, 4+4+4=12, +8=20. 
    //    Actually, let's align to 16 bytes for SIMD efficiency).
    [StructLayout(LayoutKind.Sequential)]
    public struct TokenData
    {
        public int Id;          // 4 bytes
        public float Probability; // 4 bytes
        public float LogitBias;   // 4 bytes
        public long Timestamp;    // 8 bytes
        // Total: 20 bytes. 
        // Note: For SIMD, we often pad to Vector<float>.Count * 4 bytes (usually 16 or 32 bytes).
        // Here we keep it realistic, but SIMD will handle the alignment.
    }

    // ------------------------------------------------------------------------
    // SECTION 2: The Processing Engine (SIMD & Memory Optimization)
    // ------------------------------------------------------------------------
    public static class TokenProcessor
    {
        // Constants for simulation
        const int TOKEN_COUNT = 1_000_000; // 1 Million tokens
        const float BIAS_FACTOR = 0.5f;

        /// <summary>
        /// Generates tokens directly into a pre-allocated array (Object Pooling pattern).
        /// Avoids individual allocations.
        /// </summary>
        public static TokenData[] GenerateTokens(int count)
        {
            TokenData[] tokens = new TokenData[count];
            Random rand = new Random(42); // Fixed seed for consistency

            for (int i = 0; i < count; i++)
            {
                // Structs are value types. 'tokens[i]' returns a COPY of the struct.
                // We must modify the copy and re-assign it.
                TokenData token = new TokenData
                {
                    Id = i,
                    Probability = (float)rand.NextDouble(),
                    LogitBias = (float)rand.NextDouble() * 2.0f - 1.0f, // Range -1 to 1
                    Timestamp = Stopwatch.GetTimestamp()
                };
                tokens[i] = token; // Copy back to array
            }
            return tokens;
        }

        /// <summary>
        /// Method A: Scalar Processing (Standard Loop)
        /// </summary>
        public static void ProcessScalar(TokenData[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                // Accessing struct in array requires copying it out
                TokenData token = tokens[i];
                
                // Apply bias calculation
                token.Probability += token.LogitBias * BIAS_FACTOR;
                
                // Clamp probability between 0 and 1
                if (token.Probability > 1.0f) token.Probability = 1.0f;
                if (token.Probability < 0.0f) token.Probability = 0.0f;

                // Simulate some timestamp logic
                long elapsed = Stopwatch.GetTimestamp() - token.Timestamp;

                // Write back
                tokens[i] = token;
            }
        }

        /// <summary>
        /// Method B: SIMD Processing (Vectorized)
        /// Uses System.Numerics.Vector<T> to process multiple floats in parallel.
        /// </summary>
        public static void ProcessSimd(TokenData[] tokens)
        {
            int i = 0;
            int vectorSize = Vector<float>.Count; // Usually 4 (SSE) or 8 (AVX)
            
            // We treat the struct array as a stream of floats for the math part.
            // This is an advanced technique: casting the struct memory to floats.
            // We assume TokenData layout: [Int(4), Float(4), Float(4), Long(8)]
            // We want to vectorize the 'Probability' and 'LogitBias' fields.
            
            // NOTE: Direct memory casting is complex due to struct padding.
            // For this example, we will use a "SoA" (Structure of Arrays) approach 
            // implicitly or just stick to safe SIMD on float arrays if we had them.
            // However, to stick to the 'Struct' requirement, we will manually unroll 
            // and use Vector on specific fields if they align, or fallback to scalar 
            // for the non-float parts.
            
            // A more practical SIMD approach for structs is often processing 
            // float arrays derived from the struct.
            // But let's demonstrate SIMD on the Probabilities by extracting them 
            // into a temporary buffer (for demonstration purity).
            
            float[] probabilities = new float[tokens.Length];
            float[] biases = new float[tokens.Length];

            // Extract data (Simulating memory layout access)
            for(int k=0; k<tokens.Length; k++) 
            {
                probabilities[k] = tokens[k].Probability;
                biases[k] = tokens[k].LogitBias;
            }

            // Vectorized Loop
            for (; i <= tokens.Length - vectorSize; i += vectorSize)
            {
                Vector<float> probs = new Vector<float>(probabilities, i);
                Vector<float> biasVec = new Vector<float>(biases, i);
                Vector<float> factor = new Vector<float>(BIAS_FACTOR);

                // SIMD Math: Adds 4/8 floats in a single CPU instruction
                Vector<float> result = probs + (biasVec * factor);

                // Clamping (SIMD comparison and select)
                Vector<float> max = Vector<float>.One;
                Vector<float> min = Vector<float>.Zero;
                result = Vector.Min(max, Vector.Max(min, result));

                // Store back (Note: In a real high-perf scenario, we would avoid 
                // extracting to float[] and would use Unsafe.Add to access struct fields directly)
                result.CopyTo(probabilities, i);
            }

            // Write back to structs
            for (int k = 0; k < tokens.Length; k++)
            {
                TokenData token = tokens[k];
                token.Probability = probabilities[k];
                tokens[k] = token;
            }
        }
    }

    // ------------------------------------------------------------------------
    // SECTION 3: Main Execution & Benchmarking
    // ------------------------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== AI Token Processing: Struct vs Class Optimization ===");
            
            // 1. Warmup (JIT Compilation)
            var warmup = TokenProcessor.GenerateTokens(100);
            TokenProcessor.ProcessScalar(warmup);
            TokenProcessor.ProcessSimd(warmup);

            // 2. Generate Data
            Console.WriteLine($"Generating {TokenProcessor.TOKEN_COUNT:N0} tokens...");
            TokenData[] tokenBatch = TokenProcessor.GenerateTokens(TokenProcessor.TOKEN_COUNT);
            
            // 3. Benchmark Scalar Processing
            Stopwatch sw = Stopwatch.StartNew();
            TokenProcessor.ProcessScalar(tokenBatch);
            sw.Stop();
            long scalarTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"Scalar Processing Time: {scalarTime} ms");

            // 4. Benchmark SIMD Processing
            // Regenerate to reset state
            tokenBatch = TokenProcessor.GenerateTokens(TokenProcessor.TOKEN_COUNT);
            sw.Restart();
            TokenProcessor.ProcessSimd(tokenBatch);
            sw.Stop();
            long simdTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"SIMD Processing Time:  {simdTime} ms");

            // 5. Analysis
            if (simdTime > 0)
            {
                double speedup = (double)scalarTime / simdTime;
                Console.WriteLine($"Speedup Factor: {speedup:F2}x");
            }

            Console.WriteLine("\n--- Memory Layout Inspection ---");
            InspectMemoryLayout();
        }

        static void InspectMemoryLayout()
        {
            // Demonstrate memory density
            TokenData t = new TokenData();
            int size = Unsafe.SizeOf<TokenData>();
            Console.WriteLine($"Size of TokenData struct: {size} bytes");
            
            // Compare to Class overhead
            // A class has:
            // 1. Object Header (8 bytes on 64-bit)
            // 2. Method Table Pointer (8 bytes on 64-bit)
            // 3. Plus the fields.
            // Total Class overhead: 16 bytes + fields + padding.
            // Struct has 0 overhead when stored in an array.
        }
    }
}
