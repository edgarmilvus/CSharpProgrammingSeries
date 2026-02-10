
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;

public static class HybridProcessor
{
    // Threshold for stack allocation (e.g., 512 bytes)
    private const int StackThreshold = 512;

    public static void ProcessTokenHybrid(ReadOnlySpan<byte> token, Action<Span<byte>> processor)
    {
        // Check if we can safely use the stack
        if (token.Length <= StackThreshold)
        {
            // Stack Allocation Path
            Span<byte> buffer = stackalloc byte[token.Length];
            token.CopyTo(buffer);
            processor(buffer);
        }
        else
        {
            // Heap Path (ArrayPool)
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(token.Length);
            try
            {
                // Create a span over the rented array that matches the token length
                Span<byte> buffer = rentedArray.AsSpan(0, token.Length);
                token.CopyTo(buffer);
                processor(buffer);
            }
            finally
            {
                // CRITICAL: Always return the array to the pool
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
    }

    // Simulation method
    public static (int StackAllocs, int PoolRentals) BenchmarkSimulation()
    {
        int stackCount = 0;
        int poolCount = 0;
        Random rnd = new Random(42);

        // We can't easily hook into the allocation logic inside ProcessTokenHybrid 
        // without modifying it to return stats, so we will simulate the logic here 
        // to demonstrate the decision flow for 10,000 tokens.

        for (int i = 0; i < 10000; i++)
        {
            // Generate varying lengths: 10 to 2000 bytes
            int length = rnd.Next(10, 2001);

            if (length <= StackThreshold)
            {
                stackCount++;
                // Simulate stackalloc usage (conceptual)
                Span<byte> buffer = stackalloc byte[length];
            }
            else
            {
                poolCount++;
                // Simulate ArrayPool usage (conceptual)
                byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        return (stackCount, poolCount);
    }
}
