
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record struct ProcessingResult(string Token, bool IsBlocked, int TokenCount);

public class LlmStreamProcessor
{
    private readonly HashSet<string> _blocklist = new() { "bad", "forbidden", "error" };

    // 1. Mock LLM Streamer
    public async IAsyncEnumerable<string> GetLlmTokenStreamAsync(string prompt, CancellationToken token)
    {
        var random = new Random();
        var tokens = new[] { "The", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog", "bad", "news", "error", "found" };
        
        // Yield 10-15 random tokens
        int count = random.Next(10, 16);
        
        for (int i = 0; i < count; i++)
        {
            // Check cancellation before yielding
            token.ThrowIfCancellationRequested();
            
            await Task.Delay(random.Next(20, 50), token);
            yield return tokens[random.Next(tokens.Length)];
        }
    }

    // 2. Processing Logic
    public async Task<ProcessingResult> ProcessTokenAsync(string token, CancellationToken token)
    {
        // CPU-bound check (fast)
        bool isBlocked = _blocklist.Contains(token.ToLower());

        // I/O-bound check (mock tokenizer service)
        // We simulate this with a delay
        await Task.Delay(10, token);

        return new ProcessingResult(token, isBlocked, token.Length);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var processor = new LlmStreamProcessor();
        
        Console.Write("Enter a prompt to start the stream: ");
        string prompt = Console.ReadLine() ?? "Test Prompt";

        using var cts = new CancellationTokenSource();
        
        // Interactive cancellation listener
        _ = Task.Run(() =>
        {
            if (Console.ReadKey(true).Key == ConsoleKey.C)
            {
                Console.WriteLine("\n[Cancellation Requested]");
                cts.Cancel();
            }
        });

        Console.WriteLine("Processing stream... (Press 'C' to cancel)");

        try
        {
            await ProcessStreamWithConcurrencyLimit(processor, prompt, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Processing cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task ProcessStreamWithConcurrencyLimit(LlmStreamProcessor processor, string prompt, CancellationToken token)
    {
        // 3. & 4. Dynamic Fan-Out/Fan-In with Ordered Aggregation
        // We use a SemaphoreSlim to limit concurrency to 4
        var semaphore = new SemaphoreSlim(4);
        
        // We use a List of Tasks to track active processing
        var activeTasks = new List<Task<ProcessingResult>>();
        
        // We use a Queue to track the order of arrival for aggregation
        // Since we need to print in order, we will use a simpler approach:
        // Process in batches or use a specific ordering mechanism.
        // Given the constraint "Ordered Aggregation" with dynamic arrival,
        // the most robust way without blocking the stream consumption is to 
        // track the index and sort the results at the end, OR
        // use a mechanism that ensures completion order matches input order.
        
        // However, the strict requirement is "exact order... cannot sort at the end".
        // This implies we must wait for Task #1 before printing Task #1, even if Task #2 finishes first.
        // This is complex with `WhenAny` because we need to re-assemble the order.
        
        // Strategy: 
        // 1. Consume the stream and wrap processing in a Task.
        // 2. Store these tasks in a collection.
        // 3. As tasks complete, we cannot print immediately if order matters.
        // 4. Instead, we will maintain a "Next Expected Index" and a buffer for out-of-order results.
        
        var resultBuffer = new SortedDictionary<int, ProcessingResult>();
        int streamIndex = 0;
        int nextExpectedIndex = 0;

        // We need to consume the stream AND process concurrently.
        // We can't just `await foreach` if we want to start tasks and move on immediately.
        // We need to pull tokens, start tasks, and manage the results.
        
        // Let's create a helper to pull tokens and start tasks
        var streamingTask = Task.Run(async () =>
        {
            await foreach (var tokenStr in processor.GetLlmTokenStreamAsync(prompt, token))
            {
                // Wait for a slot to open up (Concurrency Limit)
                await semaphore.WaitAsync(token);
                
                int currentIndex = streamIndex++;
                
                // Start the processing task
                var processingTask = processor.ProcessTokenAsync(tokenStr, token);
                
                // Add to active list
                activeTasks.Add(processingTask.ContinueWith(t =>
                {
                    // When task finishes, release semaphore
                    semaphore.Release();
                    
                    // Store result in buffer with its index
                    lock (resultBuffer)
                    {
                        resultBuffer[currentIndex] = t.Result;
                    }
                    return t.Result;
                }, token));
            }
        }, token);

        // 5. Aggregation Loop
        // We need to print results as they become available in order.
        // We check the buffer for the next expected index.
        while (!streamingTask.IsCompleted || activeTasks.Any(t => !t.IsCompleted) || resultBuffer.Any())
        {
            token.ThrowIfCancellationRequested();

            ProcessingResult? resultToPrint = null;
            
            lock (resultBuffer)
            {
                if (resultBuffer.TryGetValue(nextExpectedIndex, out var result))
                {
                    resultToPrint = result;
                    resultBuffer.Remove(nextExpectedIndex);
                    nextExpectedIndex++;
                }
            }

            if (resultToPrint.HasValue)
            {
                var r = resultToPrint.Value;
                Console.WriteLine($"[Order: {nextExpectedIndex}] Token: '{r.Token}' | Blocked: {r.IsBlocked} | Count: {r.TokenCount}");
            }
            else
            {
                // If we haven't received the next item yet, wait a bit
                await Task.Delay(50);
            }
        }

        // Clean up remaining tasks
        await Task.WhenAll(activeTasks);
    }
}
