
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

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Chapter16.Exercise3
{
    public static class RetryHandler
    {
        public static async Task RunWithBackoff(Action action, int maxRetries)
        {
            List<Exception> exceptions = new List<Exception>();

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"Attempt {attempt}...");
                    action(); // Execute the provided action
                    Console.WriteLine("Success!");
                    return; // Break loop on success
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {ex.Message}");
                    exceptions.Add(ex);

                    // Check if we should retry
                    if (attempt < maxRetries)
                    {
                        // Calculate exponential delay: 2 ^ attemptCount
                        // Note: 2^1=2, 2^2=4, 2^3=8. 
                        // To start at 1s (2^0), we use (attempt - 1) or 2^attempt / 2.
                        // Let's stick to the prompt's formula: 2 ^ attemptCount
                        int delaySeconds = (int)Math.Pow(2, attempt);
                        
                        Console.WriteLine($"Waiting {delaySeconds} seconds before next retry...");
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
            }

            // If we exit the loop without returning, all attempts failed
            throw new AggregateException("Operation failed after all retries.", exceptions);
        }
    }
}
