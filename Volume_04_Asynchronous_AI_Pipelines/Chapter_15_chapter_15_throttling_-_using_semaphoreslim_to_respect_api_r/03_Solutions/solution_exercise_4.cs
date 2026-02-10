
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class StreamingPipeline
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10);

    public async IAsyncEnumerable<string> GetStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Acquire semaphore slot before starting the stream request
        await _semaphore.WaitAsync(ct);

        try
        {
            // Simulate opening a connection to LLM API
            // In a real scenario, this would be an HttpClient.GetStreamAsync or similar
            Console.WriteLine($"Starting stream for: {prompt}");
            
            // Simulate streaming tokens
            for (int i = 0; i < 10; i++)
            {
                // Check cancellation token frequently
                ct.ThrowIfCancellationRequested();

                await Task.Delay(100, ct); // Simulate network delay between tokens
                
                yield return $"Token {i} for {prompt}";
            }
        }
        finally
        {
            // Ensure semaphore is released when the stream ends (either naturally or via cancellation)
            _semaphore.Release();
            Console.WriteLine($"Finished stream for: {prompt}");
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        var pipeline = new StreamingPipeline();
        var cts = new CancellationTokenSource();

        // Simulate a UI scenario: Start 15 streams (exceeding the limit of 10)
        var tasks = new List<Task>();
        
        for (int i = 0; i < 15; i++)
        {
            int id = i;
            tasks.Add(Task.Run(async () => 
            {
                try 
                {
                    await foreach (var token in pipeline.GetStreamAsync($"Stream {id}", cts.Token))
                    {
                        Console.WriteLine($"Received: {token}");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Stream {id} cancelled.");
                }
            }));
        }

        // Let them run for a bit, then cancel
        await Task.Delay(500);
        Console.WriteLine("--- Cancelling all streams ---");
        cts.Cancel();

        await Task.WhenAll(tasks);
    }
}
