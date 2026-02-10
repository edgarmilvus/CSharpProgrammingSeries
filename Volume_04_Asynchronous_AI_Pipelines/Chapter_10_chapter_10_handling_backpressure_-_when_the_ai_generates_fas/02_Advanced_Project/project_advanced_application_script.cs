
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

namespace BackpressureHandlingApp
{
    // Real-world context: A live sports commentary system where an AI generates
    // play-by-play updates much faster than the UI (console) can render them.
    // This causes the console buffer to flood, leading to lag and poor user experience.
    // We solve this using a Producer-Consumer pattern with a bounded buffer (Queue)
    // to apply backpressure, throttling the AI generation when the UI is saturated.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- AI Sports Commentary System (Backpressure Demo) ---");
            Console.WriteLine("Press 'Enter' to start the AI feed. Press 'Q' to quit.");
            Console.WriteLine("Notice how the AI generation slows down when the buffer is full.");
            
            // The buffer size represents the UI's capacity (e.g., lines of text it can hold).
            // If we set this too high, memory usage spikes. Too low, and the AI is throttled unnecessarily.
            // This decouples the AI generation (Producer) from the Console rendering (Consumer).
            int maxBufferSize = 5; 
            
            // Shared buffer (Queue) protected by a lock for thread safety.
            // This is our "Backpressure Buffer".
            Queue<string> commentaryBuffer = new Queue<string>();
            object bufferLock = new object();
            
            // Cancellation token to gracefully stop the system.
            CancellationTokenSource cts = new CancellationTokenSource();

            // Start the Consumer (UI/Console) on the main thread.
            // In a real UI app (WPF/WinForms), this would be the UI thread rendering the data.
            Task consumerTask = Task.Run(() => ConsumeCommentary(commentaryBuffer, bufferLock, cts.Token, maxBufferSize));

            // Start the Producer (AI) on a background thread.
            // In a real scenario, this could be an HTTP stream from an LLM.
            Task producerTask = Task.Run(() => ProduceCommentary(commentaryBuffer, bufferLock, cts.Token, maxBufferSize));

            // Input handling loop to allow user to stop the system.
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q)
                    {
                        Console.WriteLine("\n[SYSTEM] Shutting down...");
                        cts.Cancel();
                        break;
                    }
                    if (key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("[SYSTEM] Feed active. Processing commentary...");
                    }
                }
                // Small sleep to prevent CPU spinning in the main loop.
                Thread.Sleep(100);
            }

            // Wait for tasks to finish cleaning up.
            Task.WaitAll(new[] { producerTask, consumerTask }, 5000);
            Console.WriteLine("System stopped.");
        }

        // PRODUCER: Simulates the AI generating data rapidly.
        // This method represents the "Source" of data (e.g., an LLM streaming tokens).
        static void ProduceCommentary(Queue<string> buffer, object lockObj, CancellationToken token, int capacity)
        {
            int playCount = 1;
            while (!token.IsCancellationRequested)
            {
                // Simulate AI processing time (variable latency).
                Thread.Sleep(300); 

                // Generate data payload.
                string commentary = $"[AI] Play #{playCount}: Long pass intercepted by defense! The crowd goes wild...";
                playCount++;

                // CRITICAL: Apply Backpressure.
                // We check the buffer size before adding. If full, we wait (block).
                // This prevents memory overflow and signals the AI to slow down (cooperative throttling).
                lock (lockObj)
                {
                    // If buffer is full, wait for the consumer to signal (Monitor.Wait).
                    // This is the core "Backpressure" mechanism: the producer waits for the consumer.
                    while (buffer.Count >= capacity)
                    {
                        // Console color indicates throttling state.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[AI] BUFFER FULL. Pausing generation...");
                        Console.ResetColor();
                        
                        // Release the lock and wait until Pulse is received from Consumer.
                        Monitor.Wait(lockObj);
                    }

                    // Add data to buffer.
                    buffer.Enqueue(commentary);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[AI] Generated: {playCount - 1}");
                    Console.ResetColor();

                    // Signal the consumer that data is available.
                    Monitor.Pulse(lockObj);
                }
            }
        }

        // CONSUMER: Simulates the UI rendering data.
        // This method represents the "Sink" (e.g., a Virtualized List in a UI).
        static void ConsumeCommentary(Queue<string> buffer, object lockObj, CancellationToken token, int capacity)
        {
            while (!token.IsCancellationRequested)
            {
                string itemToRender = null;

                lock (lockObj)
                {
                    // If buffer is empty, wait for the producer to signal.
                    while (buffer.Count == 0)
                    {
                        // Release lock and wait.
                        Monitor.Wait(lockObj);
                        
                        // Check cancellation again after waking up.
                        if (token.IsCancellationRequested) return;
                    }

                    // Dequeue (consume) the data.
                    itemToRender = buffer.Dequeue();

                    // If we were previously full, signal the producer that space is available.
                    // This wakes up the waiting Producer thread.
                    if (buffer.Count == capacity - 1)
                    {
                        Monitor.PulseAll(lockObj);
                    }
                }

                // Simulate UI Rendering Latency.
                // In a real UI, this represents the time to draw the element on screen.
                // If this is slow, the buffer drains, and the AI is throttled automatically.
                Thread.Sleep(500); 

                // Render the data (UI Logic).
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[UI] Rendering: {itemToRender}");
                Console.ResetColor();
            }
        }
    }
}
