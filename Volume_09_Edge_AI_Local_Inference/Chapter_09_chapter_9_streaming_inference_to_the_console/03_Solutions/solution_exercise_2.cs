
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class CancellableStreamingEngine
{
    // Reusing the mock vocab from Exercise 1 for context
    private Dictionary<string, int> _vocab = new Dictionary<string, int> { { "The", 1 }, { " cat", 2 }, { " sat", 3 }, { " on", 4 }, { " the", 5 }, { " mat", 6 }, { " ", 7 }, { "", 0 } };
    private Dictionary<int, string> _reverseVocab => _vocab.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public async IAsyncEnumerable<string> GenerateTokensAsync(string prompt, InferenceSession session, [System.Runtime.CompilerServices.IteratorStateMachineAttribute(typeof(CancellableStreamingEngine))] CancellationToken token)
    {
        var inputTokens = new List<int> { 1 }; // Simulated start
        int maxTokens = 20;

        for (int i = 0; i < maxTokens; i++)
        {
            // 1. Check Cancellation
            token.ThrowIfCancellationRequested();

            // Simulate ONNX Run (Mocked for brevity, logic same as Ex 1)
            // In real code: using var outputs = await Task.Run(() => session.Run(...), token);
            await Task.Delay(50, token); // Simulate processing time
            
            // Mock Prediction
            int predictedId = (i % 2 == 0) ? 2 : 3; // Alternates " cat", " sat"
            
            if (predictedId == 0) yield break;

            string tokenString = _reverseVocab[predictedId];
            
            // 2. Yield
            yield return tokenString;
            
            inputTokens.Add(predictedId);
        }
    }

    public async Task StreamToConsoleAsync(IAsyncEnumerable<string> tokenStream, CancellationToken token)
    {
        try
        {
            // 3. Await Foreach
            await foreach (var tokenStr in tokenStream.WithCancellation(token))
            {
                // 4. Thread-safe Console Write
                // Console.Write is atomic for single calls, preventing interleaved characters
                // within a single line, though interleaving with other threads is still possible.
                Console.Write(tokenStr);
            }
            Console.WriteLine(); // New line after generation
        }
        catch (OperationCanceledException)
        {
            // 5. Handle Cancellation
            Console.WriteLine("\n[Generation Cancelled]");
        }
    }
}

// Example Main usage:
/*
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // Cancel after 2s
    var engine = new CancellableStreamingEngine();
    var session = new InferenceSession("model.onnx"); // Placeholder
    
    var stream = engine.GenerateTokensAsync("Prompt", session, cts.Token);
    await engine.StreamToConsoleAsync(stream, cts.Token);
*/
