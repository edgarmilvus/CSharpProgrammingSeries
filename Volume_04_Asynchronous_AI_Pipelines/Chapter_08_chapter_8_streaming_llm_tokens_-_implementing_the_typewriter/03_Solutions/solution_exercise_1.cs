
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class LlmStreamService
{
    // Simulates a network call that yields tokens one by one with delays
    public async IAsyncEnumerable<string> GetStoryStreamAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        string[] storyTokens = { "Once", " upon", " a", " time", ",", " in", " a", " land", " far", " away", "..." };

        foreach (var token in storyTokens)
        {
            // Check for cancellation before yielding
            ct.ThrowIfCancellationRequested();

            // Simulate network latency
            await Task.Delay(200, ct); 
            
            yield return token;
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var service = new LlmStreamService();
        var cts = new CancellationTokenSource();
        
        Console.WriteLine("Starting stream generation...");

        // Start the heartbeat task (simulates a responsive UI loop)
        var heartbeatTask = RunHeartbeatAsync(cts.Token);

        try
        {
            // Process the stream incrementally
            await foreach (var token in service.GetStoryStreamAsync(cts.Token))
            {
                Console.Write(token);
            }
            
            // Signal completion
            cts.Cancel(); 
            Console.WriteLine("\n\nGeneration complete.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n\nGeneration was cancelled.");
        }
        finally
        {
            // Ensure the heartbeat task completes cleanly
            await heartbeatTask;
        }
    }

    private static async Task RunHeartbeatAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Log a heartbeat every 500ms
            await Task.Delay(500, ct);
            
            // Only log if the main thread hasn't finished/cancelled yet
            if (!ct.IsCancellationRequested)
            {
                Console.WriteLine($"[Heartbeat: {DateTime.Now:HH:mm:ss}] UI Thread Active");
            }
        }
    }
}
