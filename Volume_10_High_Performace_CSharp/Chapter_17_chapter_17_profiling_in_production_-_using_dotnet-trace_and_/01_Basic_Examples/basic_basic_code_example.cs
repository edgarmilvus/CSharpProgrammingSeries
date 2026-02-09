
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
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TokenProcessingProfiler
{
    // 1. Define a custom Meter for AI-specific metrics.
    // This adheres to the OpenTelemetry standard and allows dotnet-counters to track these specifically.
    public static class AiMetrics
    {
        public static readonly Meter Meter = new("TokenProcessing.AI", "1.0.0");
        
        // Counter: Represents a monotonically increasing value (total tokens processed).
        public static readonly Counter<long> TotalTokensProcessed = 
            Meter.CreateCounter<long>("ai.tokens.total", "tokens", "Total number of tokens processed");
            
        // Histogram: Used for measuring the latency of tokenization operations.
        public static readonly Histogram<double> TokenizationLatency = 
            Meter.CreateHistogram<double>("ai.tokenization.latency", "ms", "Time taken to tokenize a prompt");
    }

    class Program
    {
        // 2. Simulate a realistic AI workload.
        // In a real scenario, this might involve calling an LLM or running a local model.
        // Here, we simulate the latency and CPU load to generate profiling data.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting AI Token Processing Simulation...");
            Console.WriteLine("Run the following commands in a separate terminal to observe metrics:");
            Console.WriteLine("  > dotnet-counters monitor --process-id <PID> TokenProcessing.AI");
            Console.WriteLine("  > dotnet-trace collect --process-id <PID> --providers Microsoft-Windows-DotNETRuntime");
            Console.WriteLine("\nPress any key to start processing...");
            Console.ReadKey();

            var cts = new CancellationTokenSource();
            var processingTask = StartProcessingPipeline(cts.Token);

            Console.WriteLine("\nProcessing running. Press 'q' to quit.");
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                // Keep running
            }

            cts.Cancel();
            await processingTask;
            Console.WriteLine("\nSimulation stopped.");
        }

        static async Task StartProcessingPipeline(CancellationToken token)
        {
            // 3. Create a background task to simulate continuous incoming requests.
            var pipelineTasks = new List<Task>();

            // We spawn multiple workers to simulate concurrent API requests.
            for (int i = 0; i < 4; i++)
            {
                pipelineTasks.Add(Task.Run(async () => 
                {
                    while (!token.IsCancellationRequested)
                    {
                        await ProcessTokenBatch();
                    }
                }, token));
            }

            await Task.WhenAll(pipelineTasks);
        }

        static async Task ProcessTokenBatch()
        {
            // 4. Start a high-resolution timer to measure latency.
            // Stopwatch is crucial for precise timing in profiling.
            var sw = Stopwatch.StartNew();

            // 5. Simulate the "Tokenization" phase.
            // In a real app, this involves string manipulation or model inference.
            // We add a random delay to mimic network/processing variance.
            var randomDelay = Random.Shared.Next(50, 200);
            await Task.Delay(randomDelay);

            // 6. Simulate the "Inference" phase (CPU intensive).
            // We perform a dummy calculation to spike CPU usage, 
            // allowing dotnet-trace to capture JIT/GC activity.
            double result = 0;
            for (int i = 0; i < 10000; i++)
            {
                result += Math.Sqrt(i) * Math.Sin(i);
            }

            sw.Stop();

            // 7. Record metrics using the custom Meter.
            // This data is exposed to dotnet-counters and OpenTelemetry exporters.
            var tokenCount = Random.Shared.Next(50, 150);
            AiMetrics.TotalTokensProcessed.Add(tokenCount);
            AiMetrics.TokenizationLatency.Record(sw.ElapsedMilliseconds);

            // 8. Simulate occasional GC pressure.
            // Allocating objects forces the Garbage Collector to run.
            // Profiling this helps identify memory bottlenecks in token pipelines.
            if (Random.Shared.Next(0, 10) > 8) 
            {
                // Allocate a moderately large list to trigger Gen0/Gen1 collections
                var _ = new List<byte>(1024 * 100); 
            }
        }
    }
}
