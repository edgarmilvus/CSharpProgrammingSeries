
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStateMachinesExercises
{
    public class Exercise5
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("=== Exercise 5: Async Streaming (IAsyncEnumerable) ===\n");

            // Create a cancellation token source to demonstrate graceful shutdown
            var cts = new CancellationTokenSource();
            
            // Simulate user cancelling after 300ms
            var _ = Task.Delay(300).ContinueWith(_ => 
            {
                Console.WriteLine("\n[User] Cancelling stream...");
                cts.Cancel();
            });

            try
            {
                await StreamLLMResponseAsync("Explain quantum physics", cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n[System] Stream operation was cancelled.");
            }
        }

        // IAsyncEnumerable allows yielding values asynchronously
        private static async IAsyncEnumerable<string> StreamLLMResponseAsync(string prompt, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            Console.WriteLine($"[LLM] Generating response for: '{prompt}'");
            
            var tokens = new[] { "Quantum", " mechanics", " describes", " nature", " at", " the", " smallest", " scales." };
            var random = new Random();

            foreach (var token in tokens)
            {
                // Check cancellation before async work
                ct.ThrowIfCancellationRequested();

                // Simulate network latency
                int delay = random.Next(50, 150);
                Console.WriteLine($"[LLM] Fetching next token (delay: {delay}ms)...");

                // The 'yield return' is the suspension point.
                // The state machine pauses here, yields the token, and waits for the consumer to request the next one.
                await Task.Delay(delay, ct);
                
                yield return token;
            }
        }
    }
}
