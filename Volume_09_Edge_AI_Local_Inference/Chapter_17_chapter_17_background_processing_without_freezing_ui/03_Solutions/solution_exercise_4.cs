
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
using System.Threading;
using System.Threading.Tasks;

public class AdvancedInferenceEngine
{
    public async IAsyncEnumerable<string> GenerateTokensAsync(string prompt, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        // Simulate fast model inference (produces tokens faster than 10/sec)
        for (int i = 0; i < 20; i++)
        {
            ct.ThrowIfCancellationRequested();

            // --- Throttling Logic ---
            // In a real scenario, we would track the timestamp of the last emitted token.
            // Here we simulate a strict delay between yields.
            // 10 tokens/sec = 100ms per token.
            await Task.Delay(100, ct);
            
            yield return $"Token_{i}";
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var engine = new AdvancedInferenceEngine();
        var cts = new CancellationTokenSource();
        
        Console.WriteLine("Starting throttled stream...");

        // Consumer loop
        await foreach (var token in engine.GenerateTokensAsync("Prompt", cts.Token))
        {
            // Visualize the smooth output
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} > {token}");
        }
        
        Console.WriteLine("Stream finished.");
    }
}
