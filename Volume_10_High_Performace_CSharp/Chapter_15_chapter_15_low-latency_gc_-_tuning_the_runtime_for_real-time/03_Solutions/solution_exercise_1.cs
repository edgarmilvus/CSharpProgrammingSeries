
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

public class TokenProcessorBenchmark
{
    public void RunBaseline(int tokenCount)
    {
        // Warm up
        RunBaselineInternal(tokenCount, true);
        
        // Measure
        RunBaselineInternal(tokenCount, false);
    }

    private void RunBaselineInternal(int tokenCount, bool isWarmup)
    {
        long startMemory = GC.GetTotalAllocatedBytes(true);
        int startGen0 = GC.CollectionCount(0);
        int startGen1 = GC.CollectionCount(1);
        int startGen2 = GC.CollectionCount(2);

        var sw = Stopwatch.StartNew();
        
        // Baseline: String concatenation (creates a new string object every iteration)
        string result = "";
        for (int i = 0; i < tokenCount; i++)
        {
            // Simulate token processing - appending a unique token
            result += $"Token_{i}_";
        }

        sw.Stop();

        if (!isWarmup)
        {
            long endMemory = GC.GetTotalAllocatedBytes(true);
            Console.WriteLine("--- Baseline (String Concatenation) ---");
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Gen 0 Collections: {GC.CollectionCount(0) - startGen0}");
            Console.WriteLine($"Gen 1 Collections: {GC.CollectionCount(1) - startGen1}");
            Console.WriteLine($"Gen 2 Collections: {GC.CollectionCount(2) - startGen2}");
            Console.WriteLine($"Allocated Memory: {endMemory - startMemory} bytes");
            Console.WriteLine();
        }
    }

    public void RunOptimized(int tokenCount)
    {
        // Warm up
        RunOptimizedInternal(tokenCount, true);
        
        // Measure
        RunOptimizedInternal(tokenCount, false);
    }

    private void RunOptimizedInternal(int tokenCount, bool isWarmup)
    {
        long startMemory = GC.GetTotalAllocatedBytes(true);
        int startGen0 = GC.CollectionCount(0);
        int startGen1 = GC.CollectionCount(1);
        int startGen2 = GC.CollectionCount(2);

        var sw = Stopwatch.StartNew();

        // Optimized: ArrayPool + Span
        // Estimate max capacity needed to avoid resizing. 
        // Average token length approx 10 chars * count + separators.
        int estimatedCapacity = tokenCount * 12; 
        char[] buffer = ArrayPool<char>.Shared.Rent(estimatedCapacity);
        int position = 0;

        try
        {
            Span<char> span = buffer.AsSpan();
            for (int i = 0; i < tokenCount; i++)
            {
                string token = $"Token_{i}_";
                int len = token.Length;
                
                // Copy token to the buffer safely using Span
                token.AsSpan().CopyTo(span.Slice(position, len));
                position += len;
            }

            // If needed, we could create the final string here, but the goal is to measure 
            // the accumulation loop without heap thrashing.
            // string finalResult = new string(buffer.AsSpan(0, position)); 
        }
        finally
        {
            // Critical: Return the buffer to the pool immediately
            ArrayPool<char>.Shared.Return(buffer);
        }

        sw.Stop();

        if (!isWarmup)
        {
            long endMemory = GC.GetTotalAllocatedBytes(true);
            Console.WriteLine("--- Optimized (ArrayPool + Span) ---");
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Gen 0 Collections: {GC.CollectionCount(0) - startGen0}");
            Console.WriteLine($"Gen 1 Collections: {GC.CollectionCount(1) - startGen1}");
            Console.WriteLine($"Gen 2 Collections: {GC.CollectionCount(2) - startGen2}");
            Console.WriteLine($"Allocated Memory: {endMemory - startMemory} bytes");
            Console.WriteLine();
        }
    }
}
