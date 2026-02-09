
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BertEdgeExercises
{
    public class DynamicBatcher
    {
        // 1. Method to create dynamic input
        public (long[] FlatMemory, int BatchSize, int SeqLen) CreateDynamicInputTensor(List<string> sentences, int maxSeqLen)
        {
            int batchSize = sentences.Count;
            int totalElements = batchSize * maxSeqLen;
            
            // 2. Allocate single contiguous block
            long[] flatMemory = new long[totalElements];

            // 3. Populate memory block
            // Note: In a real scenario, we would tokenize here. 
            // For this exercise, we simulate token IDs just to fill the array.
            for (int i = 0; i < batchSize; i++)
            {
                int offset = i * maxSeqLen;
                
                // Simulate [CLS] token
                flatMemory[offset] = 101;

                // Simulate content tokens (just filling with dummy IDs for demonstration)
                // In reality, you'd run the tokenizer logic here.
                for (int j = 1; j < maxSeqLen - 1; j++)
                {
                    flatMemory[offset + j] = (long)(j + i); // Dummy data
                }

                // Simulate [SEP] token
                flatMemory[offset + maxSeqLen - 1] = 102;
            }

            return (flatMemory, batchSize, maxSeqLen);
        }

        // 4. Advanced Memory Manipulation using Span
        public ReadOnlySpan2D<long> ReshapeToTensor(long[] flatMemory, int batchSize, int seqLen)
        {
            // MemoryMarshal allows treating a flat array as a multidimensional structure
            // without copying data. This is zero-copy and extremely fast.
            ReadOnlySpan<long> span = flatMemory.AsSpan();
            
            // Create a 2D Span view: Rows = BatchSize, Columns = SeqLen
            return MemoryMarshal.CreateReadOnlySpan2D(
                ref MemoryMarshal.GetReference(span),
                rows: batchSize,
                columns: seqLen);
        }
    }

    public class PerformanceAnalyzer
    {
        public void CompareAllocationOverhead()
        {
            Console.WriteLine("Performance Check: Single vs Dynamic Batch");
            
            // Scenario 1: Single Sentence (Batch Size = 1)
            // Memory allocated: 1 array of 128 longs
            long memorySingle = 1 * 128 * sizeof(long); 
            Console.WriteLine($"Single Batch Allocation: {memorySingle} bytes");

            // Scenario 2: Dynamic Batch (Batch Size = 4)
            // Memory allocated: 1 array of 512 longs (4 * 128)
            long memoryBatch = 4 * 128 * sizeof(long);
            Console.WriteLine($"Dynamic Batch (4) Allocation: {memoryBatch} bytes");

            // Analysis
            Console.WriteLine("\nAnalysis:");
            Console.WriteLine("1. GC Pressure: Creating 4 separate tensors for 4 sentences requires 4 separate heap allocations.");
            Console.WriteLine("   This increases Garbage Collection (GC) pressure significantly.");
            Console.WriteLine("   The dynamic batch creates 1 allocation, reducing GC overhead.");
            Console.WriteLine("\n2. Cache Locality: In the dynamic batch (flat array), the data for sentence 1");
            Console.WriteLine("   is immediately followed by sentence 2. This is contiguous in memory.");
            Console.WriteLine("   The CPU prefetcher can load this into cache lines efficiently.");
            Console.WriteLine("   Separate arrays might be scattered across the heap, causing cache misses.");
        }
    }
}
