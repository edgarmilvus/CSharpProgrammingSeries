
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

public class BufferLifecycleManager
{
    // Requirement 1 & 2: Renting and handling size
    public static void ProcessTokenChunk(ReadOnlySpan<char> tokenData, int requiredSize)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(requiredSize);
        
        try
        {
            // Check if the rented array is large enough
            if (buffer.Length < requiredSize)
            {
                // If rented buffer is too small (unlikely with Rent(minSize) but possible if pool is constrained),
                // we must resize manually.
                char[] newBuffer = ArrayPool<char>.Shared.Rent(requiredSize);
                Array.Copy(buffer, newBuffer, Math.Min(buffer.Length, requiredSize));
                
                // Return the old, smaller buffer immediately
                ArrayPool<char>.Shared.Return(buffer);
                buffer = newBuffer;
            }

            // Simulate processing: Copy token data into the buffer
            // Using Span for efficient copying
            tokenData.CopyTo(buffer.AsSpan());

            // Simulate some processing work
            // In a real scenario, we might decode or analyze the token here.
            // For this exercise, we just ensure the data is accessible.
            Console.WriteLine($"Processing chunk of size {tokenData.Length} in buffer of size {buffer.Length}");
        }
        finally
        {
            // Requirement 3: Guaranteed return
            ArrayPool<char>.Shared.Return(buffer, clearArray: true); // clearArray is good practice for sensitive data
        }
    }

    // Requirement 4: Benchmarking
    public static void RunBenchmarks()
    {
        const int iterations = 10000;
        const int bufferSize = 2048;
        var testData = new string('a', 1024).AsSpan();

        // Warm up
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Approach A: Direct Allocation
        var swA = Stopwatch.StartNew();
        int gcCountAStart = GC.CollectionCount(0);
        
        for (int i = 0; i < iterations; i++)
        {
            char[] buffer = new char[bufferSize]; // Allocates on heap
            testData.CopyTo(buffer);
            // No return needed; GC handles it (pressure increases)
        }
        
        swA.Stop();
        int gcCountAEnd = GC.CollectionCount(0);

        // Approach B: ArrayPool
        // Collect GC before starting to ensure fair comparison
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var swB = Stopwatch.StartNew();
        int gcCountBStart = GC.CollectionCount(0);

        for (int i = 0; i < iterations; i++)
        {
            char[] buffer = ArrayPool<char>.Shared.Rent(bufferSize);
            try
            {
                testData.CopyTo(buffer);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        swB.Stop();
        int gcCountBEnd = GC.CollectionCount(0);

        Console.WriteLine("--- Benchmark Results ---");
        Console.WriteLine($"Direct Allocation: {swA.ElapsedMilliseconds}ms, Gen 0 Collections: {gcCountAEnd - gcCountAStart}");
        Console.WriteLine($"ArrayPool Rented: {swB.ElapsedMilliseconds}ms, Gen 0 Collections: {gcCountBEnd - gcCountBStart}");
    }
}
