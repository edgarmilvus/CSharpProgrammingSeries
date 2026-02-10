
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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAIPipeline
{
    // Real-world context:
    // We are building a "Real-time Log Analysis Dashboard" for a server farm.
    // Multiple servers generate log streams. We need to consume these streams asynchronously,
    // filter for critical errors in real-time, and buffer the output to prevent UI freezing,
    // simulating an LLM token streaming scenario.

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Starting Real-time Log Analysis Pipeline ---");
            Console.WriteLine("Press 'q' to stop the pipeline gracefully.\n");

            // 1. Setup Cancellation Token Source for graceful shutdown
            using var cts = new CancellationTokenSource();

            // 2. Create a background task to detect 'q' key press
            _ = Task.Run(() =>
            {
                while (Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    Console.WriteLine("\n[User Input] Q detected. Requesting cancellation...");
                    cts.Cancel();
                    break;
                }
            });

            // 3. Instantiate the Async Pipeline
            var logProcessor = new LogStreamProcessor();

            try
            {
                // 4. The Core Concept: 'await foreach'
                // We iterate over the asynchronous stream without blocking the main thread.
                // The loop pauses execution when data isn't available yet, yielding control back to the event loop.
                await foreach (var processedLog in logProcessor.GetAnalyzedLogsAsync(cts.Token))
                {
                    // Visualizing the flow of data (simulated LLM token streaming)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"[Processed Output]: ");
                    Console.ResetColor();
                    Console.WriteLine(processedLog);
                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[Pipeline Status] Operation was cancelled by the user.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[Error] An unexpected error occurred: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("\n--- Pipeline Shutdown Complete ---");
            }
        }
    }

    /// <summary>
    /// Simulates a raw data source (e.g., a network stream or file stream).
    /// In a real LLM scenario, this would be the HTTP Response stream.
    /// </summary>
    public class RawLogGenerator
    {
        private readonly string[] _logLevels = { "INFO", "WARN", "ERROR", "DEBUG" };
        private readonly Random _random = new Random();

        // Generates a log entry with a slight delay to simulate network latency
        public async Task<string> ReadNextLogLineAsync(CancellationToken ct)
        {
            // Simulate I/O delay (e.g., waiting for network packet)
            await Task.Delay(_random.Next(100, 300), ct);

            var level = _logLevels[_random.Next(_logLevels.Length)];
            var message = $"Server-{_random.Next(1, 5)}: Detected {level} status code {_random.Next(400, 600)}";
            return message;
        }
    }

    /// <summary>
    /// The core engine that implements IAsyncEnumerable.
    /// This encapsulates the logic for fetching, filtering, and formatting data asynchronously.
    /// </summary>
    public class LogStreamProcessor
    {
        private readonly RawLogGenerator _generator = new RawLogGenerator();

        // Implements the Async Stream pattern
        public async IAsyncEnumerable<string> GetAnalyzedLogsAsync(CancellationToken ct)
        {
            // In a real scenario, this loop might read until a stream closes or a specific token is received.
            // Here, we loop indefinitely until cancellation is requested.
            while (!ct.IsCancellationRequested)
            {
                // 1. Await the raw data generation
                // This yields control until the task completes.
                string rawLog = await _generator.ReadNextLogLineAsync(ct);

                // 2. Apply Business Logic (Filtering)
                // We only care about ERROR logs in this dashboard.
                if (rawLog.Contains("ERROR"))
                {
                    // 3. Formatting the output (simulating LLM response formatting)
                    string formattedLog = FormatForDisplay(rawLog);

                    // 4. Yielding the result
                    // This is the key mechanism of IAsyncEnumerable.
                    // It returns the value to the consumer and pauses here until the consumer requests the next item.
                    yield return formattedLog;
                }
            }
        }

        // Helper method to demonstrate modular logic without advanced features
        private string FormatForDisplay(string rawLog)
        {
            // Simple string concatenation (avoiding String Interpolation for strict "basic blocks" adherence)
            return DateTime.Now.ToString("HH:mm:ss") + " | CRITICAL: " + rawLog;
        }
    }
}
