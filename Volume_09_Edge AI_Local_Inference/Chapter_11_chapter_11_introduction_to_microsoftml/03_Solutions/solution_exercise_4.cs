
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OnnxBenchmarking
{
    class Program
    {
        static void Main(string[] args)
        {
            string modelPath = "TinyBERT_Sentiment.onnx";
            
            // Configuration
            int warmupIterations = 10;
            int benchmarkIterations = 100;
            
            // 1. Warm-up Phase
            Console.WriteLine("Warming up...");
            RunInference(modelPath, warmupIterations);
            
            // 2. Benchmarking Phase
            Console.WriteLine("Running benchmark...");
            var stopwatch = Stopwatch.StartNew();
            
            // Monitor Memory Before
            long memoryBefore = GC.GetTotalMemory(true);
            long processMemoryBefore = Process.GetCurrentProcess().WorkingSet64;

            RunInference(modelPath, benchmarkIterations);

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false); // Don't force GC here to see peak
            long processMemoryAfter = Process.GetCurrentProcess().WorkingSet64;

            // 3. Calculate Metrics
            double avgLatencyMs = (double)stopwatch.ElapsedMilliseconds / benchmarkIterations;
            double throughput = 1000.0 / avgLatencyMs; // Inferences per second

            double managedMemoryMB = (memoryAfter - memoryBefore) / (1024.0 * 1024.0);
            double processMemoryMB = (processMemoryAfter - processMemoryBefore) / (1024.0 * 1024.0);

            // 4. Output Report
            Console.WriteLine("\n--- Edge Inference Benchmark ---");
            Console.WriteLine($"Model: {modelPath}");
            Console.WriteLine($"Iterations: {benchmarkIterations}");
            Console.WriteLine($"Average Latency: {avgLatencyMs:F2} ms");
            Console.WriteLine($"Throughput: {throughput:F2} inferences/sec");
            Console.WriteLine($"Peak Managed Memory Delta: {managedMemoryMB:F2} MB");
            Console.WriteLine($"Peak Process Memory Delta: {processMemoryMB:F2} MB");
            Console.WriteLine("-------------------------------");

            // 5. Edge Case Analysis
            Console.WriteLine("\n--- Analysis ---");
            Console.WriteLine("Impact of ExecutionMode.ORT_PARALLEL vs ORT_SEQUENTIAL:");
            Console.WriteLine("On a single-core edge device (e.g., Raspberry Pi Zero):");
            Console.WriteLine("- ORT_SEQUENTIAL: Uses one thread. Predictable latency, low context switching.");
            Console.WriteLine("- ORT_PARALLEL: Attempts to use intra-op parallelism. On a single core, this adds");
            Console.WriteLine("  overhead (thread management) without speedup. It may actually increase latency");
            Console.WriteLine("  due to thread contention and context switching. It is generally recommended");
            Console.WriteLine("  to stick to SEQUENTIAL on single-core devices.");
        }

        static void RunInference(string modelPath, int iterations)
        {
            using var ortEnv = OrtEnv.Instance();
            using var sessionOptions = new SessionOptions();
            
            // Optional: Explicitly set execution mode (default is sequential)
            // sessionOptions.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL; 
            
            using var session = new InferenceSession(modelPath, sessionOptions);
            
            // Prepare dummy input once
            var inputTensor = GenerateDummyInput();
            var inputName = session.InputMetadata.Keys.First();
            using var inputOrtValue = OrtValue.CreateFromTensor(inputTensor);
            var inputs = new List<NamedOrtValue> { new NamedOrtValue(inputName, inputOrtValue) };

            for (int i = 0; i < iterations; i++)
            {
                // Run inference
                using var outputs = session.Run(inputs);
                // Dispose outputs immediately to keep scope clean
            }
        }

        static DenseTensor<float> GenerateDummyInput()
        {
            var data = new float[1 * 128];
            new Random().NextBytes(MemoryMarshal.AsBytes(data.AsSpan())); // Fast random fill
            return new DenseTensor<float>(data, new[] { 1, 128 });
        }
    }
}
