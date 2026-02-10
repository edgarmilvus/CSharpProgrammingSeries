
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionProfilingDemo
{
    class Program
    {
        // Main entry point: Simulates a real-time AI token processing pipeline
        // and provides hooks for monitoring via dotnet-counters and dotnet-trace.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting AI Token Processing Pipeline Simulation...");
            Console.WriteLine("Use 'dotnet-counters monitor -p <PID>' to watch CPU/Memory.");
            Console.WriteLine("Use 'dotnet-trace collect -p <PID> --providers Microsoft-Windows-DotNETRuntime' for GC/JIT analysis.");
            Console.WriteLine("Press Ctrl+C to stop.\n");

            // 1. Setup: Initialize the pipeline components.
            var tokenizer = new Tokenizer();
            var processor = new TokenProcessor();
            var logger = new MetricsLogger();

            // 2. Simulation Loop: Mimics incoming requests.
            // We use a cancellation token to handle graceful shutdown.
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            // 3. Run the pipeline continuously until stopped.
            try
            {
                await RunPipeline(tokenizer, processor, logger, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nShutdown requested. Flushing logs...");
            }

            // 4. Final Report: Summarize performance metrics collected.
            logger.PrintSummary();
            Console.WriteLine("Simulation ended.");
        }

        // Orchestrates the flow of data through the pipeline.
        static async Task RunPipeline(Tokenizer tokenizer, TokenProcessor processor, MetricsLogger logger, CancellationToken token)
        {
            int iteration = 0;
            while (!token.IsCancellationRequested)
            {
                // Simulate a burst of incoming text data (e.g., from a web request)
                string inputText = $"AI Request {iteration++}: The quick brown fox jumps over the lazy dog. " +
                                   "Optimization requires careful profiling and memory management. " +
                                   "Span<T> and SIMD are crucial for high-performance C#.";

                // 1. Tokenization Phase: Convert raw text into integer tokens.
                // This is often CPU-bound and memory-intensive if not optimized.
                int[] tokens = tokenizer.ConvertToTokens(inputText);

                // 2. Processing Phase: Apply logic (e.g., sentiment analysis simulation).
                // This simulates heavy CPU work, potentially triggering GC pressure.
                bool result = processor.AnalyzeTokens(tokens);

                // 3. Logging Phase: Record metrics for external observation.
                logger.RecordIteration(iteration, tokens.Length, result);

                // Simulate some delay to mimic real-world request pacing.
                // Using Task.Delay is non-blocking, allowing the runtime to process other work.
                await Task.Delay(500, token);
            }
        }
    }

    // Simulates converting text into tokens (integers).
    // Focuses on memory allocation patterns visible to dotnet-counters.
    public class Tokenizer
    {
        private readonly Random _random = new Random();

        public int[] ConvertToTokens(string input)
        {
            // ALLOCATION: Allocates a new integer array for every request.
            // In a high-throughput system, this creates pressure on the Gen0/Gen1 Heap.
            // Profiling with dotnet-trace will show frequent Gen0 collections here.
            int[] tokens = new int[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                // Simulate a hash-like integer representation of the character.
                tokens[i] = input[i] * _random.Next(1, 10);
            }

            return tokens;
        }
    }

    // Simulates an AI model inference step.
    // Focuses on CPU usage and potential JIT compilation overhead.
    public class TokenProcessor
    {
        // Pre-allocate a buffer to simulate a lookup table (avoiding allocations in the hot path).
        private readonly double[] _weights = new double[256];

        public TokenProcessor()
        {
            // Initialize weights to simulate model parameters.
            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = i * 0.01;
            }
        }

        public bool AnalyzeTokens(int[] tokens)
        {
            double sum = 0.0;

            // CPU-INTENSIVE LOOP: Simulates vectorizable math operations.
            // In a real scenario, this is where SIMD (System.Numerics) would be used.
            // Profiling with dotnet-trace (CPU samples) will show high time spent here.
            for (int i = 0; i < tokens.Length; i++)
            {
                int token = tokens[i];
                
                // Bounds check to simulate safe access (though we know the range here).
                if (token >= 0 && token < _weights.Length)
                {
                    sum += _weights[token];
                }
                else
                {
                    // Fallback logic for out-of-bounds tokens.
                    sum += 0.001;
                }

                // Artificially extend processing time to make it visible in traces.
                // This simulates complex matrix multiplications or activation functions.
                for (int j = 0; j < 100; j++) 
                {
                    sum /= 1.000001; 
                }
            }

            return sum > 0.5; // Arbitrary decision boundary.
        }
    }

    // Collects and aggregates metrics for analysis.
    // This class itself should be lightweight to avoid becoming the bottleneck.
    public class MetricsLogger
    {
        private long _totalTokensProcessed = 0;
        private long _totalRequests = 0;
        private long _positiveResults = 0;
        private Stopwatch _stopwatch = new Stopwatch();

        public void RecordIteration(int iteration, int tokenCount, bool result)
        {
            _totalRequests++;
            _totalTokensProcessed += tokenCount;
            if (result) _positiveResults++;

            // Log to console periodically to show liveness.
            if (iteration % 10 == 0)
            {
                // ALLOCATION: String formatting allocates memory on the heap.
                // While acceptable for logging, high-frequency logging can cause GC pressure.
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Req: {iteration}, Tokens: {tokenCount}, Mem: {GC.GetTotalMemory(false) / 1024}KB");
            }
        }

        public void PrintSummary()
        {
            Console.WriteLine("\n--- Final Metrics Summary ---");
            Console.WriteLine($"Total Requests: {_totalRequests}");
            Console.WriteLine($"Total Tokens: {_totalTokensProcessed}");
            Console.WriteLine($"Positive Results: {_positiveResults}");
            
            // Calculate average tokens per request.
            double avg = _totalRequests > 0 ? (double)_totalTokensProcessed / _totalRequests : 0;
            Console.WriteLine($"Avg Tokens/Req: {avg:F2}");
        }
    }
}
