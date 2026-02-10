
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class AdvancedStreamingExample
{
    private readonly MemoryStream _outputStream = new MemoryStream();

    public async Task StreamToOutputAsync(IAsyncEnumerable<string> tokenStream, CancellationTokenSource cts)
    {
        var token = cts.Token;
        try
        {
            await foreach (var tokenItem in tokenStream)
            {
                // Simulate hallucination check every 5 items
                if (tokenItem.EndsWith("_4") || tokenItem.EndsWith("_9")) // Simple logic to hit every 5th token
                {
                    var score = new Random().Next(0, 100);
                    if (score > 80)
                    {
                        throw new HallucinationDetectedException($"Hallucination detected! Score: {score}");
                    }
                }

                // Write to stream
                var bytes = System.Text.Encoding.UTF8.GetBytes(tokenItem + " ");
                await _outputStream.WriteAsync(bytes, 0, bytes.Length, token);
                
                // Check cancellation
                token.ThrowIfCancellationRequested();
            }
        }
        catch (HallucinationDetectedException ex)
        {
            Console.WriteLine($"Detected hallucination: {ex.Message}");
            // Trigger cancellation to notify other parts of the system
            cts.Cancel();
            // Re-throw to be caught by outer handler
            throw;
        }
        catch (OperationCanceledException)
        {
            // Attempt graceful flush of buffered data
            // We use CancellationToken.None here to ensure flush completes even if cancellation is active
            await _outputStream.FlushAsync(CancellationToken.None);
            Console.WriteLine("Stream flushed gracefully after cancellation.");
            throw;
        }
        finally
        {
            // Always dispose resources
            _outputStream.Dispose();
            Console.WriteLine("Resource cleanup complete.");
        }
    }
}

public class HallucinationDetectedException : Exception
{
    public HallucinationDetectedException(string message) : base(message) { }
}

// Interactive Challenge Implementation
public class InteractiveChallengeRunner
{
    public async Task Run()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var example = new AdvancedStreamingExample();

        // Create a token stream that yields items slowly to allow score checking
        async IAsyncEnumerable<string> TokenStream()
        {
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(100); // Slow enough to potentially hit timeout or hallucination
                yield return $"Token_{i}";
            }
        }

        try
        {
            await example.StreamToOutputAsync(TokenStream(), cts);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Main execution caught cancellation.");
        }
        catch (HallucinationDetectedException)
        {
            Console.WriteLine("Main execution caught hallucination exception.");
        }
    }
}
