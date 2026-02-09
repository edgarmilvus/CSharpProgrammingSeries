
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class LlmResponseCache
{
    private readonly object _lock = new object();
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
    
    // Dictionary to track ongoing computations to prevent duplicate LLM calls
    private readonly Dictionary<string, Task<string>> _pendingRequests = new Dictionary<string, Task<string>>();

    public async Task<string> GetOrAddAsync(string prompt, Func<Task<string>> llmFactory)
    {
        // 1. Fast path: Check if already cached (read-only operation)
        lock (_lock)
        {
            if (_cache.TryGetValue(prompt, out var cachedResponse))
            {
                return cachedResponse;
            }
        }

        Task<string> pendingTask;
        bool isOriginator = false;

        lock (_lock)
        {
            // 2. Check if a request is already in progress
            if (_pendingRequests.TryGetValue(prompt, out pendingTask))
            {
                // Another thread is handling it; wait for that result
            }
            else
            {
                // 3. We are the originator: Create a TaskCompletionSource to represent the ongoing work
                // We use a TaskCompletionSource to wrap the async operation safely within the lock context
                var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                _pendingRequests[prompt] = tcs.Task;
                pendingTask = tcs.Task;
                isOriginator = true;
            }
        }

        // 4. If we are waiting for another thread, await their result
        if (!isOriginator)
        {
            return await pendingTask;
        }

        // 5. We are the originator: Perform the expensive operation OUTSIDE the lock
        string response = string.Empty;
        try
        {
            response = await llmFactory();
        }
        catch (Exception ex)
        {
            // Signal failure to waiting threads
            ((TaskCompletionSource<string>)_pendingRequests[prompt]).TrySetException(ex);
            throw;
        }

        // 6. Store result and cleanup
        lock (_lock)
        {
            _cache[prompt] = response;
            // Signal completion to waiting threads
            if (_pendingRequests.TryGetValue(prompt, out var tcsTask) && tcsTask is TaskCompletionSource<string> tcs)
            {
                tcs.TrySetResult(response);
            }
            _pendingRequests.Remove(prompt);
        }

        return response;
    }

    // Test Harness
    public static async Task RunTest()
    {
        var cache = new LlmResponseCache();
        int factoryCallCount = 0;
        var semaphore = new SemaphoreSlim(0, 1);

        // Simulate LLM call
        async Task<string> MockLlmCall()
        {
            Interlocked.Increment(ref factoryCallCount);
            await Task.Delay(100); // Simulate network latency
            return "Response";
        }

        // Scenario 1: 50 concurrent requests for the SAME prompt
        var sharedPrompt = "What is AI?";
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () => 
            {
                await semaphore.WaitAsync(); // Synchronize start
                return await cache.GetOrAddAsync(sharedPrompt, MockLlmCall);
            }));
        }

        // Release all threads at once to maximize concurrency
        semaphore.Release(50);
        await Task.WhenAll(tasks);

        Console.WriteLine($"Shared Prompt Calls: {factoryCallCount} (Expected: 1)");
        
        // Reset for Scenario 2
        factoryCallCount = 0;
        var uniqueTasks = new List<Task>();

        // Scenario 2: 50 concurrent requests for DIFFERENT prompts
        for (int i = 0; i < 50; i++)
        {
            int id = i; // Capture variable
            uniqueTasks.Add(Task.Run(async () => 
            {
                await semaphore.WaitAsync();
                return await cache.GetOrAddAsync($"Prompt_{id}", MockLlmCall);
            }));
        }

        semaphore.Release(50);
        await Task.WhenAll(uniqueTasks);

        Console.WriteLine($"Unique Prompt Calls: {factoryCallCount} (Expected: 50)");
    }
}
