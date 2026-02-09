
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalLLMBackgroundProcessor
{
    // Real-world Context: A customer support chatbot running locally on a user's machine.
    // The UI must remain responsive to accept new user inputs while the LLM generates a response
    // in the background. We simulate the LLM inference (token generation) using a heavy loop
    // to demonstrate the non-blocking behavior.

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup: Initialize the cancellation token source to handle user interruptions.
            // 2. Setup: Create a progress reporter to update the UI from the background thread.
            var cts = new CancellationTokenSource();
            var progress = new Progress<string>(token =>
            {
                // This lambda runs on the main thread (SynchronizationContext captured).
                // It simulates updating a UI console line without freezing the main loop.
                Console.Write(token + " ");
            });

            Console.WriteLine("Local LLM Chatbot Started. Type 'exit' to quit.");
            Console.WriteLine("--------------------------------------------------");

            // 3. Main Loop: Continuously accept user input until 'exit' is commanded.
            while (true)
            {
                Console.Write("\n[User]: ");
                string input = Console.ReadLine();

                if (input?.ToLower() == "exit")
                {
                    cts.Cancel();
                    break;
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                // 4. Execution: Offload the heavy LLM inference to a background thread.
                // We use Task.Run to prevent the main thread from blocking.
                try
                {
                    Console.Write("[Bot]: ");
                    await Task.Run(() => ProcessQuery(input, progress, cts.Token), cts.Token);
                    Console.WriteLine(); // New line after response is complete.
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("\n[System]: Inference cancelled by user.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Error]: {ex.Message}");
                }
            }

            Console.WriteLine("Application shutting down...");
        }

        // Simulates a local LLM inference engine (like Llama or Phi running via ONNX).
        // It mimics token generation with delays and checks for cancellation.
        static void ProcessQuery(string query, IProgress<string> progress, CancellationToken token)
        {
            // 5. Model Warm-up: Simulate loading model weights or preparing the session.
            // In a real app, this might load an ONNX model file.
            Thread.Sleep(100); 
            token.ThrowIfCancellationRequested();

            // 6. Inference Pipeline: Generate tokens one by one.
            // We use a standard loop instead of LINQ to adhere to the chapter's constraints.
            string[] mockTokens = GenerateResponseTokens(query);
            
            for (int i = 0; i < mockTokens.Length; i++)
            {
                // 7. Safety Check: Verify if the user requested cancellation before processing the next token.
                token.ThrowIfCancellationRequested();

                // 8. Streaming Update: Report the token to the UI thread via IProgress<T>.
                // This ensures the UI updates immediately without waiting for the whole response.
                progress.Report(mockTokens[i]);

                // 9. Latency Simulation: Mimics the time taken for matrix multiplication in a real LLM.
                // Without this, the loop would finish instantly, hiding the concurrency benefits.
                Thread.Sleep(150); 
            }
        }

        // Helper method to generate a mock response based on input.
        // In a real scenario, this would be handled by the ONNX Runtime.
        static string[] GenerateResponseTokens(string input)
        {
            if (input.Contains("hello"))
            {
                return new string[] { "Hello", " there", "!", " How", " can", " I", " assist", " you", " today?" };
            }
            else if (input.Contains("time"))
            {
                return new string[] { "The", " current", " time", " is", " ", DateTime.Now.ToString("HH:mm:ss"), "." };
            }
            else
            {
                return new string[] { "I", " processed", " your", " request", " about", " '", input, "'.", " (Simulated)" };
            }
        }
    }
}
