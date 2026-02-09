
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Simulating an external AI provider (e.g., OpenAI, Azure AI)
public static class ExternalAiProvider
{
    private static readonly Random _random = new();
    private static int _requestCount = 0;

    // Simulates an API call that is rate-limited to 2 concurrent requests.
    // If more than 2 requests arrive simultaneously, the provider "throttles" (rejects) them.
    public static async Task<string> GetCompletionAsync(string prompt, CancellationToken ct)
    {
        var currentCount = Interlocked.Increment(ref _requestCount);
        Console.WriteLine($"[API] Request '{prompt}' received. Active requests: {currentCount}");

        // Simulate network latency
        await Task.Delay(TimeSpan.FromMilliseconds(200), ct);

        // Simulate rate limiting logic
        if (currentCount > 2)
        {
            Interlocked.Decrement(ref _requestCount);
            Console.WriteLine($"[API] REJECTED '{prompt}' (Too many concurrent requests: {currentCount})");
            throw new HttpRequestException("Rate limit exceeded. Status: 429 Too Many Requests.");
        }

        // Simulate processing
        await Task.Delay(TimeSpan.FromMilliseconds(300), ct);
        
        Interlocked.Decrement(ref _requestCount);
        Console.WriteLine($"[API] COMPLETED '{prompt}'");
        return $"Response to: {prompt}";
    }
}

public class ThrottledAiClient
{
    // SemaphoreSlim(2, 2) creates a semaphore with an initial count of 2 and a maximum count of 2.
    // This enforces that only 2 threads can enter the protected section concurrently.
    private readonly SemaphoreSlim _throttler = new(2, 2);

    public async Task<string> ProcessRequestAsync(string prompt, CancellationToken ct)
    {
        // We wrap the semaphore wait/release in a using statement to ensure 
        // the semaphore is always released, even if an exception occurs.
        await _throttler.WaitAsync(ct);
        
        try
        {
            // Only 2 of these calls can be active at any given moment across all instances
            // of this client (if shared) or per instance (if instantiated separately).
            return await ExternalAiProvider.GetCompletionAsync(prompt, ct);
        }
        finally
        {
            // Release the semaphore slot so another waiting request can proceed.
            _throttler.Release();
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("--- Starting Throttled Batch Processing ---");
        
        var client = new ThrottledAiClient();
        var prompts = Enumerable.Range(1, 5).Select(i => $"Prompt {i}").ToList();
        var tasks = new List<Task<string>>();

        // We use Task.WhenAll to process the batch concurrently.
        // Without SemaphoreSlim, this would spawn 5 simultaneous API calls,
        // likely overwhelming the provider.
        foreach (var prompt in prompts)
        {
            // We do not await immediately. We start the task and store it.
            tasks.Add(client.ProcessRequestAsync(prompt, CancellationToken.None));
        }

        try
        {
            var results = await Task.WhenAll(tasks);
            Console.WriteLine($"\n--- Batch Completed. {results.Length} results received. ---");
            foreach (var result in results)
            {
                Console.WriteLine($"Result: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- Batch Failed: {ex.Message} ---");
        }
    }
}
