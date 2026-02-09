
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// A 'Hello World' level demonstration of memory allocation pressure in AI token processing.
/// This example simulates a common AI pattern: processing a stream of tokens (strings)
/// and building a response. We will compare a naive, allocation-heavy approach against
/// a memory-efficient approach using StackAlloc and Span<T>.
/// </summary>
public class TokenProcessingBenchmark
{
    // Simulate a common AI scenario: processing a batch of tokens (e.g., from a tokenizer)
    // to generate a response or calculate statistics.
    public static void Main()
    {
        Console.WriteLine("=== AI Token Processing: Allocation Pressure Demo ===\n");

        // 1. Setup: Create a dataset of "tokens" (strings) to process.
        // In a real AI model, these might be sub-word units like "▁Hello" or "world".
        string[] tokens = GenerateMockTokens(10_000);
        
        // 2. Run the Naive Approach (Heavy Heap Allocation)
        // This simulates the "easy" way to write C# code, which inadvertently creates
        // massive garbage collection (GC) pressure.
        Console.WriteLine("1. Running Naive Approach (High Allocation)...");
        long naiveTime = RunNaiveApproach(tokens);
        
        // 3. Run the Optimized Approach (Zero Heap Allocation)
        // This simulates high-performance C# using Span<T> and StackAlloc.
        Console.WriteLine("\n2. Running Optimized Approach (Zero Allocation)...");
        long optimizedTime = RunOptimizedApproach(tokens);

        // 4. Compare Results
        Console.WriteLine("\n=== Results ===");
        Console.WriteLine($"Naive Approach Time:    {naiveTime} ms");
        Console.WriteLine($"Optimized Approach Time: {optimizedTime} ms");
        Console.WriteLine($"Speedup: {(double)naiveTime / optimizedTime:F2}x");
        
        Console.WriteLine("\nContext: In a real-time AI inference server (e.g., LLM serving),");
        Console.WriteLine("the Naive approach would cause frequent GC pauses (Gen 0/1 collections),");
        Console.WriteLine("stalling the CPU and reducing throughput by 20-50%.");
    }

    // ---------------------------------------------------------
    // SCENARIO: Processing a stream of tokens to build a summary
    // ---------------------------------------------------------
    // Context: An AI model generates tokens one by one. We need to process them
    // to filter out special tokens (like [PAD]) and calculate a running hash/checksum.
    // In a naive implementation, we often convert these tokens to new strings or 
    // allocate buffers for every single operation.
    // ---------------------------------------------------------

    /// <summary>
    /// The Naive Approach: Allocates new objects on the Heap for every operation.
    /// </summary>
    private static long RunNaiveApproach(string[] tokens)
    {
        // Stopwatch to measure execution time (Wall-clock time).
        Stopwatch sw = Stopwatch.StartNew();
        
        // Simulate a result accumulator.
        int accumulatedValue = 0;
        
        // Iterate through the tokens.
        foreach (string token in tokens)
        {
            // ALLOCATION 1: ToUpper() creates a NEW string on the heap.
            // In AI, this might be normalizing text or removing special markers.
            string processedToken = token.ToUpper();
            
            // ALLOCATION 2: Substring() creates ANOTHER new string on the heap.
            // We take the first half of the token to simulate feature extraction.
            string feature = processedToken.Substring(0, processedToken.Length / 2);
            
            // Simulate a calculation (e.g., token ID lookup or hashing).
            // We use the string's length to simulate unique processing.
            accumulatedValue += feature.Length;
            
            // In a real scenario, 'processedToken' and 'feature' become "garbage"
            // immediately after the loop iteration. This fills up Memory Gen 0.
        }
        
        sw.Stop();
        Console.WriteLine($"   Result Checksum: {accumulatedValue}");
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// The Optimized Approach: Uses Span<T> and StackAlloc to avoid Heap Allocations.
    /// </summary>
    private static long RunOptimizedApproach(string[] tokens)
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        int accumulatedValue = 0;
        
        foreach (string token in tokens)
        {
            // STEP 1: Get a Span<char> from the existing string.
            // Span<T> is a type-safe window over memory. It can point to the stack, heap, or unmanaged memory.
            // Here, it points to the heap memory of the existing string, but without overhead.
            ReadOnlySpan<char> tokenSpan = token.AsSpan();
            
            // STEP 2: Create a buffer on the Stack.
            // StackAlloc allocates memory directly on the current stack frame.
            // It is extremely fast (just moving the stack pointer) and creates ZERO garbage.
            // We allocate enough space for the uppercase conversion.
            Span<char> buffer = stackalloc char[tokenSpan.Length];
            
            // STEP 3: Copy and Transform in-place.
            // We iterate manually or use a helper to fill the buffer.
            // No new strings are created.
            for (int i = 0; i < tokenSpan.Length; i++)
            {
                char c = tokenSpan[i];
                // Simple ToUpper logic for demonstration (ASCII only).
                if (c >= 'a' && c <= 'z')
                    buffer[i] = (char)(c - 32);
                else
                    buffer[i] = c;
            }
            
            // STEP 4: Slice the buffer to simulate "Substring".
            // Slicing a Span is free (it just adjusts start index and length).
            // It does not copy memory or allocate.
            int halfLength = buffer.Length / 2;
            ReadOnlySpan<char> featureSpan = buffer.Slice(0, halfLength);
            
            // STEP 5: Process the Span directly.
            // We calculate length (trivial) or we could process the characters directly.
            accumulatedValue += featureSpan.Length;
        }
        
        sw.Stop();
        Console.WriteLine($"   Result Checksum: {accumulatedValue}");
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Helper to generate mock data.
    /// </summary>
    private static string[] GenerateMockTokens(int count)
    {
        string[] tokens = new string[count];
        Random rnd = new Random(42); // Fixed seed for consistency
        for (int i = 0; i < count; i++)
        {
            // Generate random strings of varying lengths to simulate real tokens.
            int length = rnd.Next(5, 15);
            char[] chars = new char[length];
            for (int j = 0; j < length; j++)
            {
                chars[j] = (char)rnd.Next('a', 'z' + 1);
            }
            tokens[i] = new string(chars);
        }
        return tokens;
    }
}
