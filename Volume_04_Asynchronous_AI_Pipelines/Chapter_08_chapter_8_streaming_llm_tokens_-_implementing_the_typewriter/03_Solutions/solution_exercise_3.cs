
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

public class InteractiveStreamingClient
{
    public async Task RunStreamingProcess()
    {
        // Create a CTS with a 3-second timeout
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        
        // Start the key listener task
        var keyListenerTask = ListenForCancelKeyAsync(cts);

        try
        {
            Console.WriteLine("Streaming started. Press 'X' + Enter to cancel, or wait for 3 seconds.");
            
            // Simulate a long-running stream
            for (int i = 0; i < 100; i++)
            {
                // Check cancellation token frequently
                cts.Token.ThrowIfCancellationRequested();

                // Simulate work
                await Task.Delay(100, cts.Token);
                Console.Write($".");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n[!] Generation Stopped by User/Timeout.");
            // Flush partial buffer logic would go here
            Console.WriteLine("Partial content flushed.");
        }
        finally
        {
            // Ensure the listener task is cleaned up
            await keyListenerTask;
        }
    }

    private async Task ListenForCancelKeyAsync(CancellationTokenSource cts)
    {
        // Run on a background thread to avoid blocking the main async loop
        await Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                // Non-blocking check for key availability
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.KeyChar == 'X' || key.KeyChar == 'x')
                    {
                        cts.Cancel();
                        break;
                    }
                }
                // Small delay to prevent high CPU usage in the loop
                Thread.Sleep(50);
            }
        });
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var client = new InteractiveStreamingClient();
        try
        {
            await client.RunStreamingProcess();
        }
        catch (OperationCanceledException)
        {
            // Handled inside the client, but caught here for safety
        }
        Console.WriteLine("Application finished.");
    }
}
