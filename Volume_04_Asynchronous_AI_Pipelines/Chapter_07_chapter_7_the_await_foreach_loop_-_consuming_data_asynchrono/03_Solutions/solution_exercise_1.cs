
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

public class AsyncDataGenerator
{
    private static readonly Random _random = new();

    public async IAsyncEnumerable<int> GenerateReadingsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 1; i <= 10; i++)
        {
            // 1. Simulate failure: 10% chance to throw an exception
            if (_random.Next(1, 11) == 1)
            {
                throw new InvalidOperationException("Simulated sensor failure detected.");
            }

            // 2. Produce the reading
            int reading = _random.Next(0, 100);
            yield return reading;

            // 3. Introduce randomized delay (100ms - 500ms)
            int delayMs = _random.Next(100, 501);
            
            // 4. Respect cancellation token during delay
            try
            {
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Gracefully exit if cancellation is requested during the delay
                yield break;
            }
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var generator = new AsyncDataGenerator();
        var cts = new CancellationTokenSource();

        Console.WriteLine("Starting sensor reading stream...");

        try
        {
            // Using explicit try/catch around the await foreach
            await foreach (var reading in generator.GenerateReadingsAsync(cts.Token))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Reading: {reading}");
            }
        }
        catch (InvalidOperationException ex)
        {
            // Handle the simulated failure gracefully
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            
            // Signal cancellation to stop any pending delays in the generator
            cts.Cancel();
        }
        
        Console.WriteLine("Stream terminated.");
    }
}
