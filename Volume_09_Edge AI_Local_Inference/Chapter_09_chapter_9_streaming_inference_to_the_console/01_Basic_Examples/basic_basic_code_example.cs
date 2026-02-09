
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeAIStreamingDemo
{
    // ---------------------------------------------------------
    // 1. MOCK INFRASTRUCTURE (Simulating ONNX Runtime)
    // ---------------------------------------------------------
    
    // Represents a simplified ONNX Runtime InferenceSession.
    // In a real scenario, this wraps the Microsoft.ML.OnnxRuntime.InferenceSession.
    public class MockInferenceSession : IDisposable
    {
        private readonly string[] _mockVocabulary;

        public MockInferenceSession()
        {
            // A tiny vocabulary for demonstration purposes.
            _mockVocabulary = new[] { "Hello", " ", "World", "!", "How", "are", "you", "?", "\n" };
        }

        // Simulates the complex 'Run' method of ONNX Runtime.
        // It takes an input token ID and returns the next predicted token ID.
        // We simulate a delay to represent model computation time.
        public async Task<int> RunAsync(int inputTokenId, CancellationToken cancellationToken)
        {
            // Simulate GPU/CPU inference latency (e.g., 100ms - 300ms per token)
            await Task.Delay(new Random().Next(100, 300), cancellationToken);

            // Simple logic to generate a sequence: "Hello World!" -> "How are you?"
            // This is purely for the demo to produce readable output.
            int nextTokenId = inputTokenId switch
            {
                0 => 1, // "Hello" -> " "
                1 => 2, // " " -> "World"
                2 => 3, // "World" -> "!"
                3 => 4, // "!" -> "How"
                4 => 5, // "How" -> " "
                5 => 6, // " " -> "are"
                6 => 7, // "are" -> " "
                7 => 8, // " " -> "you"
                8 => 3, // "you" -> "!"
                _ => -1 // End of sequence
            };

            return nextTokenId;
        }

        // Helper to convert ID back to text for display.
        public string TokenIdToString(int tokenId)
        {
            if (tokenId < 0 || tokenId >= _mockVocabulary.Length) return "";
            return _mockVocabulary[tokenId];
        }

        public void Dispose() { /* Cleanup native resources */ }
    }

    // ---------------------------------------------------------
    // 2. CORE LOGIC: Streaming Pipeline
    // ---------------------------------------------------------

    public class StreamingGenerator
    {
        private readonly MockInferenceSession _session;

        public StreamingGenerator(MockInferenceSession session)
        {
            _session = session;
        }

        /// <summary>
        /// Generates text asynchronously and yields tokens as they are produced.
        /// </summary>
        /// <param name="promptTokenId">The starting token ID (e.g., BOS token).</param>
        /// <param name="cancellationToken">Cancellation token to stop generation.</param>
        /// <returns>An async stream of strings (tokens).</returns>
        public async IAsyncEnumerable<string> GenerateStreamAsync(
            int promptTokenId, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int currentTokenId = promptTokenId;

            // The Inference Loop
            while (true)
            {
                // 1. Run inference asynchronously (non-blocking)
                int nextTokenId = await _session.RunAsync(currentTokenId, cancellationToken);

                // 2. Check for end-of-sequence (EOS) token
                if (nextTokenId == -1) 
                    break;

                // 3. Decode the token ID to text
                string tokenText = _session.TokenIdToString(nextTokenId);

                // 4. Yield the token immediately to the consumer
                yield return tokenText;

                // 5. Update state for the next iteration
                currentTokenId = nextTokenId;
            }
        }
    }

    // ---------------------------------------------------------
    // 3. CONSUMER: Console Application
    // ---------------------------------------------------------

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Local LLM Stream...");
            
            // Initialize the mock session (In real code, load .onnx file here)
            using var inferenceSession = new MockInferenceSession();
            var generator = new StreamingGenerator(inferenceSession);

            // Define a cancellation token source (e.g., handle Ctrl+C)
            using var cts = new CancellationTokenSource();

            try
            {
                Console.WriteLine("\nGenerated Output: ");
                Console.WriteLine("-------------------");

                // Start the stream
                // We start with Token ID 0 ("Hello")
                await foreach (var token in generator.GenerateStreamAsync(0, cts.Token))
                {
                    // Write token to console immediately without waiting for the next line
                    Console.Write(token);
                    
                    // Optional: Flush standard output to ensure text appears immediately
                    // (Usually handled by Console.Write, but critical in some redirections)
                    Console.Out.Flush();
                }

                Console.WriteLine("\n-------------------");
                Console.WriteLine("Stream finished.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n[Stream cancelled by user]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error: {ex.Message}]");
            }
        }
    }
}
