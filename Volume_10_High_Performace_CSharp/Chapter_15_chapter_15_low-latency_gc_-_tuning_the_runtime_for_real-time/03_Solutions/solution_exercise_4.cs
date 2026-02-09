
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

public class AdvancedTokenStreamer
{
    // Shared buffer for the streamer
    private char[] _buffer = ArrayPool<char>.Shared.Rent(1024);

    public void ProcessTokenStream(ReadOnlySpan<int> tokenIds)
    {
        // 1. FIX: Clear the buffer before reuse to prevent data corruption
        // If the previous token was longer than the current one, 
        // stale characters might remain in the buffer.
        _buffer.AsSpan().Clear();

        // 2. OPTIMIZE: Validate using SIMD
        if (!ValidateTokenIdsSimd(tokenIds))
        {
            Console.WriteLine("Validation failed for batch.");
            return;
        }

        // Simulate processing
        int position = 0;
        foreach (var id in tokenIds)
        {
            string token = $"ID:{id};";
            if (token.Length + position < _buffer.Length)
            {
                token.AsSpan().CopyTo(_buffer.AsSpan(position));
                position += token.Length;
            }
        }
    }

    private bool ValidateTokenIdsSimd(ReadOnlySpan<int> ids)
    {
        // Vector width depends on architecture (e.g., 256-bit AVX2 = 8 ints, 128-bit SSE = 4 ints)
        int simdLength = Vector<int>.Count;
        int i = 0;
        
        // Define constants as Vectors for comparison
        var minVal = new Vector<int>(1);       // > 0
        var maxVal = new Vector<int>(50000);   // < 50000

        // Process in vector-sized chunks
        for (; i <= ids.Length - simdLength; i += simdLength)
        {
            var vectorIds = new Vector<int>(ids.Slice(i, simdLength));
            
            // Check if ids > 0 (vectorIds > minVal)
            // Check if ids < 50000 (vectorIds < maxVal)
            // Vector comparison returns a mask of -1 (true) or 0 (false)
            if (Vector.GreaterThanAny(vectorIds, minVal) && 
                Vector.LessThanAny(vectorIds, maxVal))
            {
                // All elements in this vector are valid
                continue;
            }
            
            // If we are here, at least one element in the vector failed
            // We need to fall back to scalar check for this specific chunk
            for (int j = 0; j < simdLength; j++)
            {
                if (ids[i + j] <= 0 || ids[i + j] >= 50000) return false;
            }
        }

        // Process remaining elements that didn't fit in a vector
        for (; i < ids.Length; i++)
        {
            if (ids[i] <= 0 || ids[i] >= 50000) return false;
        }

        return true;
    }

    // Helper for performance comparison
    public void RunBenchmark()
    {
        var random = new Random(42);
        int[] ids = new int[100_000];
        for(int i=0; i<ids.Length; i++) ids[i] = random.Next(1, 49999);

        // Warmup
        ValidateTokenIdsSimd(ids);
        ValidateTokenIdsScalar(ids);

        var sw = Stopwatch.StartNew();
        for(int k=0; k<1000; k++) ValidateTokenIdsSimd(ids);
        sw.Stop();
        Console.WriteLine($"SIMD Time: {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        for(int k=0; k<1000; k++) ValidateTokenIdsScalar(ids);
        sw.Stop();
        Console.WriteLine($"Scalar Time: {sw.ElapsedMilliseconds} ms");
    }

    private bool ValidateTokenIdsScalar(ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            if (id <= 0 || id >= 50000) return false;
        }
        return true;
    }
}
