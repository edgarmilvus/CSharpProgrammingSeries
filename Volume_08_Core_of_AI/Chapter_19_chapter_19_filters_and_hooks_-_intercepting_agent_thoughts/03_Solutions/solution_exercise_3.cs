
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.SemanticKernel;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RetryFilter : IKernelFilter
{
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;

    public RetryFilter(int maxRetries, TimeSpan delay)
    {
        _maxRetries = maxRetries;
        _delay = delay;
    }

    public async Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        int currentRetry = 0;

        while (true)
        {
            try
            {
                // Attempt execution
                await next(context);
                return; // Success, exit method
            }
            catch (Exception ex) when (ex.Message.Contains("Timeout"))
            {
                currentRetry++;

                if (currentRetry > _maxRetries)
                {
                    // Max retries reached, re-throw the last exception
                    throw;
                }

                // Wait for the specified delay before retrying
                // Note: In a real scenario, exponential backoff is preferred
                await Task.Delay(_delay);

                // Loop continues to retry 'next(context)'
                // Critical: context is preserved automatically as we are in the same scope
            }
        }
    }
}

// --- Mock Function for Testing (Simulates transient failure) ---
public class TransientFunction : IKernelFunction
{
    private int _callCount = 0;

    public string Name => "TransientOperation";

    public async Task<KernelResult> InvokeAsync(KernelContext context, CancellationToken cancellationToken = default)
    {
        _callCount++;
        
        if (_callCount < 3)
        {
            // Simulate failure on first two calls
            throw new InvalidOperationException("Operation timed out: Timeout");
        }

        // Success on third call
        return KernelResult.FromValue("Success", context.Function);
    }
}
