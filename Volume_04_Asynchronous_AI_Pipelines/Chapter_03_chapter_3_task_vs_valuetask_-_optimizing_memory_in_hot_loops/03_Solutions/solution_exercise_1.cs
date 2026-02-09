
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TokenDecoder
{
    // Using ConcurrentDictionary for thread safety, though the exercise implies a single-threaded benchmark loop.
    private readonly ConcurrentDictionary<string, Token> _tokenCache = new();

    // Refactored to return ValueTask<Token>
    public ValueTask<Token> GetTokenIdAsync(string tokenString)
    {
        // Synchronous Hot Path
        if (_tokenCache.TryGetValue(tokenString, out var token))
        {
            // Returns directly. No heap allocation for the Task wrapper.
            return new ValueTask<Token>(token);
        }

        // Asynchronous Cold Path
        // We cannot return the async keyword here because that would box the ValueTask immediately.
        // Instead, we return a Task-wrapped ValueTask or use an async local function.
        return new ValueTask<Token>(FetchAndCacheTokenAsync(tokenString));
    }

    private async Task<Token> FetchAndCacheTokenAsync(string tokenString)
    {
        // Simulate I/O
        await Task.Delay(10);
        var newToken = new Token(tokenString);
        _tokenCache[tokenString] = newToken;
        return newToken;
    }
}

public record Token(string Value);

public class BenchmarkRunner
{
    public static async Task Run()
    {
        var decoder = new TokenDecoder();
        const int iterations = 10000;
        const double cacheHitRatio = 0.9;
        int hits = (int)(iterations * cacheHitRatio);
        int misses = iterations - hits;

        // Pre-populate some cache entries
        for (int i = 0; i < hits; i++)
        {
            decoder.GetTokenIdAsync($"token_{i}"); // Just to warm up if we wanted, but we want to measure the loop
        }

        Console.WriteLine("Starting Benchmark...");
        
        // Measure Memory Before
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long memoryBefore = GC.GetTotalMemory(true);

        // Run the loop
        for (int i = 0; i < iterations; i++)
        {
            string key = i < hits ? $"token_{i}" : $"miss_{i}";
            
            // In a real benchmark (BenchmarkDotNet), we would avoid awaiting here to measure allocation per call,
            // but for this exercise, we await to simulate real usage and measure total memory pressure.
            var token = await decoder.GetTokenIdAsync(key);
        }

        // Measure Memory After
        long memoryAfter = GC.GetTotalMemory(true);
        long allocatedBytes = memoryAfter - memoryBefore;

        Console.WriteLine($"Completed {iterations} iterations.");
        Console.WriteLine($"Estimated Allocation: {allocatedBytes} bytes");
        Console.WriteLine($"Average per call: {(double)allocatedBytes / iterations:F2} bytes");
        
        // Note: With ValueTask, synchronous hits allocate 0 bytes for the Task.
        // Only the 10% misses allocate Task objects (plus the Token objects themselves).
    }
}
