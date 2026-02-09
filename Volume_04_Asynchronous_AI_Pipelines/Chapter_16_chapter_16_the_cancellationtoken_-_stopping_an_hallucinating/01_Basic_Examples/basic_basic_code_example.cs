
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class HallucinationDetector
{
    // A list of known hallucination markers (e.g., "poison pills").
    private static readonly HashSet<string> _hallucinationMarkers = new()
    {
        "[UNDEFINED]",
        "ERROR: MEMORY CORRUPTION",
        "NULL_REFERENCE"
    };

    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- AI Hallucination Cancellation Demo ---");

        // 1. Create a CancellationTokenSource. This acts as the controller for cancellation.
        using var cts = new CancellationTokenSource();

        // 2. Simulate a user pressing a "Stop Generation" button after 1.5 seconds.
        // In a real UI app, this would be triggered by a button click event.
        var userCancellationTask = Task.Run(async () =>
        {
            await Task.Delay(1500);
            Console.WriteLine("\n[USER ACTION]: Detected potential hallucination! Triggering cancellation...\n");
            cts.Cancel(); // Signal cancellation
        });

        try
        {
            // 3. Pass the Token to the processing method.
            await GenerateResponseAsync("Explain quantum physics", cts.Token);
        }
        catch (OperationCanceledException)
        {
            // 4. Catch the specific exception thrown when the token is canceled.
            Console.WriteLine("\n[SYSTEM]: Operation was successfully canceled. Returning safe fallback response.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR]: An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            // Ensure the user cancellation task completes before exiting.
            await userCancellationTask;
        }
    }

    /// <summary>
    /// Simulates an AI model generating a response token by token.
    /// </summary>
    private static async Task GenerateResponseAsync(string prompt, CancellationToken token)
    {
        Console.WriteLine($"[AI]: Generating response for: \"{prompt}\"...");

        // Simulate a stream of tokens from an LLM.
        var responseTokens = new[]
        {
            "Quantum",
            " physics",
            " is",
            " the",
            " study",
            " of",
            " the",
            " smallest",
            " particles",
            " [UNDEFINED]", // <--- Hallucination marker detected here
            " in",
            " the",
            " universe."
        };

        foreach (var tokenPart in responseTokens)
        {
            // 5. CRITICAL: Check the token before processing.
            // This throws OperationCanceledException if cancellation was requested.
            token.ThrowIfCancellationRequested();

            // Simulate network latency or processing time.
            await Task.Delay(200);

            // 6. Check for internal hallucination markers (Poison Pill detection).
            if (_hallucinationMarkers.Contains(tokenPart))
            {
                Console.WriteLine($"[AI INTERNAL]: Hallucination marker '{tokenPart}' detected. Requesting cancellation...");
                // In a real scenario, the AI service might cancel itself here.
                // For this example, we will let the external token handle it, 
                // but we can also manually trigger cancellation:
                // token.ThrowIfCancellationRequested(); 
                // Or simply throw to stop immediately:
                throw new OperationCanceledException("Internal hallucination detection triggered.", token);
            }

            // 7. Output the token if no cancellation occurred.
            Console.Write(tokenPart);
        }
    }
}
