
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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class ResilientStreamDemo
{
    // 3. Simulate a network operation that randomly throws an exception
    // 4. Respect CancellationToken
    public static async IAsyncEnumerable<string> StreamAiResponseAsync(
        string prompt, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 5. Resource Cleanup: Simulate opening a connection
        Console.WriteLine("Opening connection to AI Service...");
        
        try
        {
            // Simulate generating 10 chunks
            for (int i = 0; i < 10; i++)
            {
                // Check for cancellation before doing work
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate network delay
                await Task.Delay(100, cancellationToken);

                // Randomly throw an exception to simulate network failure
                if (i == 5 && new Random().Next(0, 2) == 0) // 50% chance at chunk 5
                {
                    throw new HttpRequestException("Network connection dropped.");
                }

                yield return $"Chunk {i}: Response text for '{prompt}'...";
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Iterator detected cancellation. Stopping gracefully.");
            // Don't rethrow here if we want graceful termination without bubbling up
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Iterator caught network error: {ex.Message}");
            throw; // Re-throw to let consumer know why it stopped
        }
        finally
        {
            // 5. Guaranteed cleanup
            Console.WriteLine("Closing connection and releasing resources...");
        }
    }

    // 6. Consumer implementation
    public static async Task ConsumeStreamAsync()
    {
        // Setup cancellation source with 500ms timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        
        try
        {
            Console.WriteLine("Starting consumption...");
            
            // 4. Pass the token to the iterator
            await foreach (var chunk in StreamAiResponseAsync("Explain quantum physics", cts.Token))
            {
                Console.WriteLine($"Received: {chunk}");
            }
        }
        catch (OperationCanceledException)
        {
            // This catches cancellation if it bubbles up from the iterator
            Console.WriteLine("[Consumer] The operation was cancelled.");
        }
        catch (HttpRequestException ex)
        {
            // 6. Catch specific exceptions
            Console.WriteLine($"[Consumer] Network Error: {ex.Message}");
        }
    }

    public static async Task RunDemo()
    {
        await ConsumeStreamAsync();
    }
}

// Entry point
// await ResilientStreamDemo.RunDemo();
