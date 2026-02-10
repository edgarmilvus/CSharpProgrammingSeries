
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStreamSimulation
{
    // Real-world context: Simulating an LLM (Large Language Model) that generates text token-by-token.
    // Instead of waiting for the entire response (which could be slow), we stream tokens to the UI
    // for a responsive user experience.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Starting LLM Stream Simulation ---\n");

            // 1. Initialize the LLM Service
            var llmService = new LLMService();

            // 2. Define a prompt
            string prompt = "Explain the concept of asynchronous streams in C#.";

            // 3. Consume the stream using IAsyncEnumerable
            // We use 'await foreach' to iterate over the asynchronous sequence.
            // This allows the UI to update as soon as each token arrives.
            try
            {
                int tokenCount = 0;
                await foreach (var token in llmService.GenerateResponseAsync(prompt))
                {
                    Console.Write(token + " ");
                    tokenCount++;

                    // Simulate UI rendering delay or console update time
                    await Task.Delay(50); 
                }

                Console.WriteLine($"\n\n--- Stream Finished. Total tokens: {tokenCount} ---");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n\n--- Stream was cancelled by the user. ---");
            }
        }
    }

    /// <summary>
    /// Represents a service that communicates with an LLM.
    /// </summary>
    public class LLMService
    {
        // A predefined vocabulary for simulation purposes.
        private readonly string[] _vocabulary = {
            "Asynchronous", "streams", "allow", "processing", "of", "data", 
            "as", "it", "arrives,", "reducing", "memory", "usage", "and", 
            "improving", "responsiveness.", "This", "is", "crucial", "for", 
            "AI", "pipelines", "handling", "large", "language", "models."
        };

        private readonly Random _random = new Random();

        /// <summary>
        /// Generates a response token by token using IAsyncEnumerable.
        /// </summary>
        /// <param name="prompt">The user input.</param>
        /// <returns>An asynchronous stream of strings (tokens).</returns>
        public IAsyncEnumerable<string> GenerateResponseAsync(string prompt)
        {
            // We pass the cancellation token to allow the caller to stop the stream.
            // In a real app, this might be linked to a UI "Stop" button.
            return GenerateTokensAsync(CancellationToken.None);
        }

        // Implementation of the async iterator
        private async IAsyncEnumerable<string> GenerateTokensAsync(CancellationToken ct)
        {
            // Simulate processing time before the first token
            await Task.Delay(200, ct);

            int tokensToGenerate = 20; // Simulate a fixed length response

            for (int i = 0; i < tokensToGenerate; i++)
            {
                // Check for cancellation request
                ct.ThrowIfCancellationRequested();

                // Simulate network latency for each token (variable delay)
                int delay = _random.Next(50, 150);
                await Task.Delay(delay, ct);

                // Select a random word from the vocabulary
                string token = _vocabulary[_random.Next(_vocabulary.Length)];

                // CRITICAL: Yield return suspends execution here and sends the token to the consumer.
                // The method resumes here when the consumer requests the next item.
                yield return token;
            }
        }
    }
}
