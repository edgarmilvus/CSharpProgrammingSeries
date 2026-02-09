
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public enum StreamingMode { Standard, Fast }

public class StreamingLLMClient
{
    private static readonly Random _random = new();
    private readonly string _responseText = "This is a simulated streaming response from an asynchronous AI pipeline.";

    public async IAsyncEnumerable<string> GetResponseStreamAsync(string prompt, StreamingMode mode, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Simulate initial processing time
        await Task.Delay(200, cancellationToken);

        var tokens = _responseText.Split(' ');

        foreach (var token in tokens)
        {
            // Calculate delay based on mode
            int baseMin = 50;
            int baseMax = 150;
            
            int delayMs = mode == StreamingMode.Fast 
                ? _random.Next(baseMin / 2, baseMax / 2) 
                : _random.Next(baseMin, baseMax);

            try
            {
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                yield break;
            }

            yield return token + " ";
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var client = new StreamingLLMClient();
        
        // Interactive Challenge: Select Mode
        Console.WriteLine("Select Streaming Mode: [1] Standard (Slower)  [2] Fast");
        var mode = Console.ReadKey().Key == ConsoleKey.D2 ? StreamingMode.Fast : StreamingMode.Standard;
        Console.WriteLine($"\nSelected Mode: {mode}\n");

        var cts = new CancellationTokenSource();

        // Task to detect Ctrl+C or 'q' key to cancel
        _ = Task.Run(() =>
        {
            if (Console.ReadKey(true).Key == ConsoleKey.Q) 
                cts.Cancel();
        });

        var responseBuilder = new StringBuilder();
        
        Console.WriteLine("Generating response...");
        Console.CursorVisible = false; // Hides cursor for cleaner effect

        try
        {
            await foreach (var token in client.GetResponseStreamAsync("User Prompt", mode, cts.Token))
            {
                responseBuilder.Append(token);
                
                // Typewriter effect: Clear line and rewrite
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1)); // Clear line
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(responseBuilder.ToString());
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n\n[Stream Cancelled by User]");
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }
}
