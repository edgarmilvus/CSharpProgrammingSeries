
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
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

// Re-using definitions from Exercise 1 for context
namespace Chapter16.Exercise4
{
    public class RateLimitExceededException : Exception 
    { 
        public int RetryAfterSeconds { get; set; } 
        public RateLimitExceededException(int retryAfter) { RetryAfterSeconds = retryAfter; }
    }

    public static class ResilientApiClient
    {
        public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Await the delegate passed in
                    return await operation();
                }
                catch (RateLimitExceededException rlEx)
                {
                    // STRATEGY 1: Specific Exception Logic
                    Console.WriteLine($"Rate limit hit. Strategy: Wait specific {rlEx.RetryAfterSeconds}s.");
                    
                    if (attempt == maxRetries) throw; // Re-throw if last attempt

                    await Task.Delay(TimeSpan.FromSeconds(rlEx.RetryAfterSeconds));
                }
                catch (HttpRequestException netEx)
                {
                    // STRATEGY 2: Exponential Backoff Logic
                    int delaySeconds = (int)Math.Pow(2, attempt);
                    Console.WriteLine($"Network error. Strategy: Exponential backoff {delaySeconds}s.");
                    
                    if (attempt == maxRetries) throw; // Re-throw if last attempt

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            // Should be unreachable due to throws inside loop, but required for compilation
            throw new InvalidOperationException("Max retries reached.");
        }
    }
}
