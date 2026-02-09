
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
using System.Threading;
using System.Threading.Tasks;

public class LocalLLMInference
{
    // ManualResetEventSlim for the pause feature
    private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

    public async Task<string> GenerateAsync(string prompt, IProgress<string> progress, CancellationToken ct)
    {
        // Run the heavy simulation on a background thread
        return await Task.Run(async () =>
        {
            string result = "";
            // Simulate generating 10 tokens
            for (int i = 0; i < 10; i++)
            {
                // Check for cancellation
                ct.ThrowIfCancellationRequested();

                // Wait until unpaused (non-blocking wait if possible, but inside Task.Run blocking is acceptable)
                _pauseEvent.Wait(ct);

                // Simulate generation time per token
                await Task.Delay(200, ct);

                string token = $"Token_{i} ";
                result += token;
                
                // Report progress immediately
                progress.Report(token);
            }
            return result;
        }, ct);
    }

    public void TogglePause()
    {
        if (_pauseEvent.IsSet)
        {
            _pauseEvent.Reset();
            Console.WriteLine("\n[Inference Paused]");
        }
        else
        {
            _pauseEvent.Set();
            Console.WriteLine("\n[Inference Resumed]");
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        var inference = new LocalLLMInference();

        // Setup progress reporter (runs on Main thread context)
        var progress = new Progress<string>(token =>
        {
            Console.Write(token);
        });

        Console.WriteLine("Starting inference... Press 'P' to toggle pause, 'C' to cancel.");

        // Start the inference task
        Task<string> inferenceTask = inference.GenerateAsync("The capital of France is", progress, cts.Token);

        Console.WriteLine($"Inference started at {DateTime.Now:HH:mm:ss.fff}");

        // Main thread loop to prove responsiveness
        while (!inferenceTask.IsCompleted)
        {
            // Simulate UI tick
            Console.WriteLine($"\n[Main Thread Tick: {DateTime.Now:HH:mm:ss.fff}]");
            
            // Check for user input without blocking indefinitely
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'P' || key.KeyChar == 'p')
                {
                    inference.TogglePause();
                }
                else if (key.KeyChar == 'C' || key.KeyChar == 'c')
                {
                    cts.Cancel();
                    break;
                }
            }

            await Task.Delay(500); // Non-blocking wait
        }

        try
        {
            string finalResult = await inferenceTask;
            Console.WriteLine($"\nFinal Result: {finalResult}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nInference was cancelled by user.");
        }
    }
}
