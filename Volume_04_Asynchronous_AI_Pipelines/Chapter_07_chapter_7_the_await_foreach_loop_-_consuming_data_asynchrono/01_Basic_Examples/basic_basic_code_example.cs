
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
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    // Entry point of the application
    public static async Task Main(string[] args)
    {
        // 1. Create a CancellationTokenSource to handle graceful cancellation
        using var cts = new CancellationTokenSource();
        
        // Simulate a user pressing Ctrl+C after 3 seconds to cancel the operation
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            Console.WriteLine("\n[Simulating User Cancellation...]");
            cts.Cancel();
        });

        try
        {
            Console.WriteLine("Starting asynchronous stream consumption...");

            // 2. Consume the async stream using 'await foreach'
            // This loop awaits each item as it becomes available.
            await foreach (var token in GetStreamingResponseAsync(cts.Token))
            {
                Console.Write(token); // Process the token (e.g., print to console)
            }

            Console.WriteLine("\n\nStream consumption finished successfully.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n\nOperation was cancelled by the user.");
        }
    }

    /// <summary>
    /// Simulates an asynchronous stream of data (e.g., an LLM response).
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of strings.</returns>
    public static async IAsyncEnumerable<string> GetStreamingResponseAsync(
        [System.Runtime.CompilerServices.IteratorCancellation] CancellationToken cancellationToken)
    {
        // Simulated data chunks representing tokens from an AI
        string[] tokens = { "Hello", ", ", "World", "!", " This", " is", " a", " stream." };

        foreach (var token in tokens)
        {
            // 3. Register for cancellation before the async operation
            cancellationToken.ThrowIfCancellationRequested();

            // 4. Simulate network latency (e.g., waiting for the next token from an API)
            await Task.Delay(500, cancellationToken);

            // 5. Yield the token back to the caller
            yield return token;
        }
    }
}
