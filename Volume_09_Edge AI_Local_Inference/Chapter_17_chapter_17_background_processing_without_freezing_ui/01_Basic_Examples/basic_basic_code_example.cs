
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LlmBackgroundInferenceDemo
{
    // Simulates a heavy LLM inference engine (like ONNX Runtime or a native wrapper)
    public class MockLlmEngine : IDisposable
    {
        private bool _disposed;

        // Simulates model warm-up time (e.g., loading weights into GPU memory)
        public async Task WarmUpAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[Engine] Warming up model...");
            // Simulate non-blocking CPU work
            await Task.Delay(500, cancellationToken); 
            Console.WriteLine("[Engine] Model ready.");
        }

        // Simulates the inference loop. 
        // In a real scenario, this calls the ONNX Runtime session and iterates over the tokenizer.
        public async IAsyncEnumerable<string> GenerateAsync(
            string prompt, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Console.WriteLine($"[Engine] Processing prompt: '{prompt}'");
            
            // Simulate processing latency per token
            var tokens = new[] { "The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog." };
            
            foreach (var token in tokens)
            {
                // Check cancellation before yielding
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate the time it takes to compute a single token (forward pass)
                await Task.Delay(100, cancellationToken); 
                
                yield return token;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Console.WriteLine("[Engine] Disposing resources (GPU memory released).");
                _disposed = true;
            }
        }
    }

    // Handles the orchestration of background tasks and UI updates
    public class InferenceOrchestrator
    {
        private readonly MockLlmEngine _engine;
        private CancellationTokenSource? _cts;

        public InferenceOrchestrator(MockLlmEngine engine)
        {
            _engine = engine;
        }

        // Starts the inference on a background thread
        public async Task<string> RunInferenceAsync(string prompt, IProgress<string> progress)
        {
            // 1. Safety: Cancel any previous running inference
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // 2. Warm-up (Offloaded to background to prevent UI freeze)
            // We use Task.Run to ensure the warm-up CPU work happens off the UI thread.
            await Task.Run(async () => await _engine.WarmUpAsync(token), token);

            // 3. Inference Loop
            // We capture the result in a StringBuilder-like structure (string.Join is inefficient for 
            // long streams but fine for this demo).
            var resultBuilder = new List<string>();

            // The core background processing block
            try
            {
                // Get the async stream of tokens
                var tokenStream = _engine.GenerateAsync(prompt, token);

                // Iterate over the stream asynchronously
                await foreach (var tokenChunk in tokenStream)
                {
                    resultBuilder.Add(tokenChunk);
                    
                    // Report progress to the UI thread safely
                    progress.Report(tokenChunk);
                }

                return string.Join("", resultBuilder);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n[Orchestrator] Inference was cancelled by user.");
                return string.Join("", resultBuilder) + " [Cancelled]";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Orchestrator] Error: {ex.Message}");
                throw;
            }
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }

    // Represents the UI Layer (Console or WPF/MAUI)
    public class UserInterface
    {
        private readonly InferenceOrchestrator _orchestrator;

        public UserInterface(InferenceOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public async Task SimulateUserInteraction()
        {
            Console.WriteLine("=== UI Thread: User clicks 'Generate' ===");
            var sw = Stopwatch.StartNew();

            // IProgress<T> implementation handles marshalling back to the UI thread automatically
            var progress = new Progress<string>(token =>
            {
                // In a real UI (WPF/WinUI), this callback automatically runs on the UI thread.
                // We simulate that here by checking thread ID.
                Console.Write(token); 
            });

            try
            {
                // Start the long-running task. 
                // IMPORTANT: We do NOT await inside the UI event handler in a way that blocks.
                // Here, we are in an async method, so 'await' yields control.
                var result = await _orchestrator.RunInferenceAsync("Tell me a story", progress);

                sw.Stop();
                Console.WriteLine($"\n\n=== UI Thread: Generation Complete in {sw.ElapsedMilliseconds}ms ===");
                Console.WriteLine($"Final Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UI Error: {ex.Message}");
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup
            using var engine = new MockLlmEngine();
            var orchestrator = new InferenceOrchestrator(engine);
            var ui = new UserInterface(orchestrator);

            // Run
            await ui.SimulateUserInteraction();

            // Simulate a cancellation scenario
            Console.WriteLine("\n\n=== Testing Cancellation ===");
            var cancelTask = Task.Run(async () =>
            {
                await Task.Delay(350); // Cancel halfway through generation
                Console.WriteLine("\n[System] User pressed Stop!");
                orchestrator.Cancel();
            });

            // Attempt to run again while cancelling
            try
            {
                // Note: This demonstrates concurrency handling
                await ui.SimulateUserInteraction(); 
            }
            catch (OperationCanceledException)
            {
                // Handled inside orchestrator
            }
        }
    }
}
