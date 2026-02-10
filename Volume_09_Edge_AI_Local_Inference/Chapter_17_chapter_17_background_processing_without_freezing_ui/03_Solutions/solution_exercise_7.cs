
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

// Source File: solution_exercise_7.cs
// Description: Solution for Exercise 7
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

public class InferenceFailedException : Exception
{
    public InferenceFailedException(string message, Exception inner) : base(message, inner) { }
}

public class ResilientInferenceService
{
    public async Task<string> InferWithRetryAsync(string prompt, IProgress<string> errorProgress, CancellationToken ct)
    {
        int maxRetries = 3;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Simulate inference that randomly fails
                await Task.Delay(500, ct);
                if (new Random().Next(0, 2) == 0) // 50% chance of failure
                {
                    throw new InvalidOperationException("Simulated connection failure");
                }
                
                return "Success: Model output generated.";
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                if (attempt == maxRetries)
                {
                    errorProgress.Report($"Final attempt {attempt} failed. Aborting.");
                    throw new InferenceFailedException("Max retries reached", ex);
                }

                // Calculate Exponential Backoff: 500ms, 1000ms, 2000ms
                int delay = 500 * (int)Math.Pow(2, attempt - 1);
                
                errorProgress.Report($"Attempt {attempt} failed: {ex.Message}. Retrying in {delay}ms...");

                try
                {
                    // Wait before retrying, but allow cancellation during the wait
                    await Task.Delay(delay, ct);
                }
                catch (OperationCanceledException)
                {
                    // If cancelled during the backoff, break the loop immediately
                    errorProgress.Report("Retry cancelled by user.");
                    throw;
                }
            }
        }

        return "Unreachable code";
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var service = new ResilientInferenceService();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Cancel after 5s
        
        var errorProgress = new Progress<string>(msg => Console.WriteLine($"[ERROR LOG]: {msg}"));

        try
        {
            Console.WriteLine("Starting resilient inference...");
            string result = await service.InferWithRetryAsync("Prompt", errorProgress, cts.Token);
            Console.WriteLine(result);
        }
        catch (InferenceFailedException ex)
        {
            Console.WriteLine($"Service failed permanently: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
        }
    }
}
