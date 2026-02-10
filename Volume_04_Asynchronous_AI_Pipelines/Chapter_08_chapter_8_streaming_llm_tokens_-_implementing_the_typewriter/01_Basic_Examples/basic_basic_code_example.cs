
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingLlmTypewriter
{
    // Represents a single token or chunk of text from an LLM response.
    public record TokenChunk(string Text, bool IsComplete);

    // Simulates an LLM API endpoint that streams tokens.
    // In a real scenario, this would be an HttpClient call to an external service.
    public class MockLlmApi
    {
        private static readonly Random _rng = new();

        // Simulates a streaming response using an async iterator.
        // This mimics the behavior of Server-Sent Events (SSE) or HTTP streaming.
        public async IAsyncEnumerable<TokenChunk> GetStreamingResponseAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // 1. Define the response content.
            // We are simulating a "Hello World" response from an LLM.
            string[] tokens = ["Hello", " ", "World", "!", " This", " is", " a", " streaming", " response", "."];

            // 2. Iterate through the tokens and yield them one by one.
            foreach (string token in tokens)
            {
                // Check for cancellation before processing.
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate network latency (random delay between 50ms and 150ms).
                int delay = _rng.Next(50, 150);
                await Task.Delay(delay, cancellationToken);

                // Yield the token chunk.
                // IsComplete is false for intermediate tokens, true for the last one.
                bool isComplete = token == tokens.Last();
                yield return new TokenChunk(token, isComplete);
            }
        }
    }

    // Handles the rendering of tokens to the console.
    // This class simulates the UI layer (e.g., a text block in a GUI).
    public class TypewriterRenderer
    {
        // Renders the stream of tokens to the console with a typewriter effect.
        public async Task RenderStreamAsync(IAsyncEnumerable<TokenChunk> stream, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("\n--- Start Typewriter Output ---\n");

            // 1. Asynchronously iterate over the stream.
            // This is the core mechanism that enables non-blocking consumption of data.
            await foreach (var chunk in stream.WithCancellation(cancellationToken))
            {
                // 2. Write the token to the console immediately.
                // In a UI application (WPF, MAUI, Blazor), this would update a TextBlock.
                Console.Write(chunk.Text);

                // 3. Flush the output buffer to ensure immediate display.
                // Crucial for console apps to see real-time updates.
                Console.Out.Flush();
            }

            Console.WriteLine("\n\n--- End Typewriter Output ---\n");
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup dependencies.
            var api = new MockLlmApi();
            var renderer = new TypewriterRenderer();

            // Create a cancellation token source to handle graceful shutdown.
            using var cts = new CancellationTokenSource();

            // Handle Ctrl+C to cancel the stream gracefully.
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate process termination.
                cts.Cancel();    // Signal cancellation to the async operations.
                Console.WriteLine("\nCancellation requested...");
            };

            try
            {
                // 1. Get the stream from the API.
                // Note: No data is fetched yet; this is just setting up the async iterator.
                var tokenStream = api.GetStreamingResponseAsync("Say Hello World", cts.Token);

                // 2. Render the stream.
                await renderer.RenderStreamAsync(tokenStream, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
