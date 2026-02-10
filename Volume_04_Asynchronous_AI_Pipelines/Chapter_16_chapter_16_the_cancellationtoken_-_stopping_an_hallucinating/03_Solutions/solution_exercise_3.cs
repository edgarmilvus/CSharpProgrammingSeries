
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class PoisonPillExercise
{
    private static readonly Random _rng = new Random();

    public async IAsyncEnumerable<string> StreamWithSafetyAsync(CancellationTokenSource cts)
    {
        var token = cts.Token;
        for (int i = 0; i < 50; i++)
        {
            // Simulate generation delay
            await Task.Delay(50, token);
            
            // Randomly generate a safe token or a hallucination
            string generatedToken = _rng.Next(0, 10) == 0 ? "NONSENSE" : $"SafeToken_{i}";
            
            // Check if generatedToken is "NONSENSE"
            if (generatedToken == "NONSENSE")
            {
                // Trigger cancellation internally
                cts.Cancel();
            }
            
            // Check token. This will throw immediately if Cancel() was just called above
            // or if external cancellation occurred.
            token.ThrowIfCancellationRequested();
            
            yield return generatedToken;
        }
    }

    public async Task RunExercise()
    {
        using var cts = new CancellationTokenSource();
        string fallbackResponse = "I apologize, but I encountered an internal error.";

        try
        {
            await foreach (var token in StreamWithSafetyAsync(cts))
            {
                Console.WriteLine(token);
            }
        }
        catch (OperationCanceledException)
        {
            // In a real scenario, we might inspect cts.IsCancellationRequested 
            // to distinguish between timeout and poison pill if multiple sources existed.
            // Since we only have one source here, we assume the poison pill triggered it.
            Console.WriteLine($"Caught cancellation. Returning fallback: {fallbackResponse}");
        }
    }
}
