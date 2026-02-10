
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace QuantizationConfigurator
{
    // 1. Configuration Record
    public record QuantizationConfig(
        string ExecutionProvider = "CPUExecutionProvider", 
        string QuantizationMode = "None", // Dynamic, Static, None
        string OptimizationLevel = "All"  // Basic, Extended, All
    );

    public class SessionFactory
    {
        // 2. Session Factory Method
        public static InferenceSession CreateSession(string modelPath, QuantizationConfig config)
        {
            var options = new SessionOptions();

            // Apply Execution Provider
            if (config.ExecutionProvider == "CPUExecutionProvider")
            {
                options.AppendExecutionProvider_CPU();
            }
            // Note: GPU providers would be added here if needed

            // Set Graph Optimization Level
            switch (config.OptimizationLevel)
            {
                case "Basic":
                    options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_BASIC;
                    break;
                case "Extended":
                    options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
                    break;
                case "All":
                    options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                    break;
            }

            // Apply Quantization Mode (Dynamic vs Static)
            // Note: In ONNX Runtime, dynamic quantization is often applied by loading a quantized model.
            // However, if we want to dynamically quantize a float model at runtime, we use session options.
            // For this exercise, we simulate the configuration logic.
            if (config.QuantizationMode == "Dynamic")
            {
                // This key enables dynamic quantization for operators like MatMul
                // Note: This requires the model to be compatible (usually a Float model).
                options.SetSessionGraphOptimizationLevel(GraphOptimizationLevel.ORT_ENABLE_ALL);
                // In newer ONNX Runtime versions, dynamic quantization of a float model 
                // is often done via a separate tool, but session options can influence it.
                // Here we assume the model passed is the target format or we configure for it.
            }
            
            try
            {
                return new InferenceSession(modelPath, options);
            }
            catch (OnnxRuntimeException ex)
            {
                // 5. Edge Case Handling
                Console.WriteLine($"Error loading model {modelPath} with config {config}: {ex.Message}");
                throw;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var inputTensor = new float[1 * 128]; // Dummy input
            var inputName = "input";

            // 3. Benchmarking Loop Setup
            var configs = new[]
            {
                new QuantizationConfig(QuantizationMode: "None", OptimizationLevel: "Basic"),
                new QuantizationConfig(QuantizationMode: "Dynamic", OptimizationLevel: "All")
            };

            foreach (var config in configs)
            {
                Console.WriteLine($"\nTesting Configuration: {config}");
                RunBenchmark("model_fp32.onnx", config, inputName, inputTensor);
            }
        }

        static void RunBenchmark(string modelPath, QuantizationConfig config, string inputName, float[] inputData)
        {
            var latencies = new List<double>();
            
            try 
            {
                // 4. Create Session
                using var session = SessionFactory.CreateSession(modelPath, config);
                
                var inputs = new List<NamedOnnxValue> 
                { 
                    NamedOnnxValue.CreateFromTensor(inputName, 
                        new DenseTensor<float>(inputData, new[] { 1, 128 })) 
                };

                // Warm up
                for (int i = 0; i < 5; i++) session.Run(inputs);

                // Measure 100 runs
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++)
                {
                    var start = Stopwatch.GetTimestamp();
                    using var results = session.Run(inputs);
                    latencies.Add(Stopwatch.GetTimestamp() - start);
                }
                sw.Stop();

                // Calculate Stats
                double avgLatencyMs = latencies.Average() / Stopwatch.Frequency * 1000;
                double variance = latencies.Select(l => Math.Pow((l / Stopwatch.Frequency * 1000) - avgLatencyMs, 2)).Average();
                double stdDev = Math.Sqrt(variance);

                Console.WriteLine($"Avg Latency: {avgLatencyMs:F2}ms | Std Dev: {stdDev:F2}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Benchmark failed: {ex.Message}");
            }
        }
    }
}
