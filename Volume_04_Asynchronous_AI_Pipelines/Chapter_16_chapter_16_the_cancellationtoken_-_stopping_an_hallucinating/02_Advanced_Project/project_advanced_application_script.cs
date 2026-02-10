
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

namespace HallucinationDetector
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Instantiate the cancellation token source.
            // This object manages the cancellation signal.
            var cts = new CancellationTokenSource();

            // 2. Create the AI Service instance.
            // We pass the token source to the service so it can listen for cancellation requests.
            var aiService = new AIService(cts.Token);

            // 3. Define a user query.
            // In a real scenario, this might come from a UI or API request.
            string userQuery = "Explain the concept of quantum entanglement.";

            Console.WriteLine($"User Query: {userQuery}");
            Console.WriteLine("AI is generating response...");
            Console.WriteLine("(Press 'X' to cancel generation manually if it gets stuck)");

            // 4. Start the generation task in the background.
            // We use Task.Run to offload the CPU-bound simulation to a separate thread.
            Task generationTask = Task.Run(() => aiService.GenerateResponseAsync(userQuery));

            // 5. Start a background task to monitor user input for manual cancellation.
            // This simulates a "Stop" button in a UI.
            Task monitorTask = Task.Run(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.KeyChar == 'X' || key.KeyChar == 'x')
                        {
                            Console.WriteLine("\n[SYSTEM] Manual cancellation requested by user.");
                            cts.Cancel(); // Trigger the cancellation token
                            break;
                        }
                    }
                    // Prevent tight looping
                    Thread.Sleep(50);
                }
            });

            // 6. Await the generation task with exception handling.
            try
            {
                await generationTask;
                Console.WriteLine("\n[SYSTEM] Generation completed successfully.");
            }
            catch (OperationCanceledException)
            {
                // 7. Handle the specific cancellation exception.
                // This block executes if the token is canceled before the task completes.
                Console.WriteLine("\n[SYSTEM] Operation was canceled. Cleaning up resources...");
                
                // In a real app, we might return a fallback message to the user here.
                Console.WriteLine("[SYSTEM] Fallback Response: 'I apologize, but I encountered an issue generating that response. Please try again.'");
            }
            catch (Exception ex)
            {
                // 8. Handle unexpected errors.
                Console.WriteLine($"\n[ERROR] An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                // 9. Ensure resources are disposed.
                cts.Dispose();
                Console.WriteLine("[SYSTEM] Resources released. Application shutting down.");
            }
        }
    }

    /// <summary>
    /// Simulates an AI model that generates text stream-by-stream.
    /// Contains logic to detect "hallucinations" and trigger cancellation.
    /// </summary>
    public class AIService
    {
        private readonly CancellationToken _cancellationToken;

        public AIService(CancellationToken token)
        {
            _cancellationToken = token;
        }

        public void GenerateResponseAsync(string prompt)
        {
            // Simulate a list of response chunks (tokens) that the AI might produce.
            // In a real scenario, these would come from an HTTP stream or SDK event.
            string[] responseChunks = new string[]
            {
                "Quantum entanglement is a physical phenomenon ",
                "where the quantum states of two or more objects ",
                "must be described with reference to each other, ",
                "even though the individual objects may be spatially separated. ",
                "This leads to correlations between observable physical properties. ",
                "For example, if one particle is measured to be spinning up, ",
                "the other might instantly be found to be spinning down. ",
                "Einstein famously referred to this as 'spooky action at a distance'. ",
                "However, the speed of light is not violated because no information is actually transmitted. ",
                "THE MOON IS MADE OF CHEESE AND GRAVITY IS A SOCIAL CONSTRUCT. ", // <--- HALLUCINATION TRIGGER
                "Therefore, entanglement allows for faster-than-light communication." // <--- LOGIC ERROR
            };

            try
            {
                foreach (var chunk in responseChunks)
                {
                    // 10. Critical Check: Poll the token before doing work.
                    // If cancellation was requested previously, this throws OperationCanceledException immediately.
                    _cancellationToken.ThrowIfCancellationRequested();

                    // Simulate processing time per token.
                    Thread.Sleep(200);

                    // 11. Write the chunk to the console (simulating a streaming UI).
                    Console.Write(chunk);

                    // 12. Hallucination Detection Logic (The "Poison Pill").
                    // We scan the current chunk for specific markers of nonsense.
                    if (chunk.Contains("CHEESE") || chunk.Contains("GRAVITY IS A SOCIAL CONSTRUCT"))
                    {
                        Console.WriteLine("\n[AI ALERT] Hallucination detected in stream!");
                        Console.WriteLine("[AI ALERT] Triggering cancellation token...");
                        
                        // 13. Trigger cancellation.
                        // This notifies all linked tokens (including the one in Main) that the operation should stop.
                        // Note: In a strict architecture, we might not have access to the source here.
                        // We assume the service has a reference to the source or a wrapper that allows cancellation.
                        // For this simulation, we will throw manually to simulate the interruption.
                        throw new OperationCanceledException("Hallucination detected. Generation stopped.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 14. Re-throw to propagate the cancellation up the call stack.
                // This ensures the Main method catches it in the try-catch block.
                throw;
            }
        }
    }
}
