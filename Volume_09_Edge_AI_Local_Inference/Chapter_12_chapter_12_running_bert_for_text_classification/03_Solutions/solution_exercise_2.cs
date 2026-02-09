
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BertEdgeExercises
{
    public enum ExecutionProviderType
    {
        CPU,
        CUDA,
        TensorRT,
        OpenVINO
    }

    public class InferenceConfig
    {
        public ExecutionProviderType Provider { get; set; } = ExecutionProviderType.CPU;
        public int GpuDeviceId { get; set; } = 0;
    }

    public class OptimizedInferenceManager : IDisposable
    {
        private InferenceSession _session;
        private readonly InferenceConfig _config;

        public OptimizedInferenceManager(string modelPath, InferenceConfig config)
        {
            _config = config;
            var sessionOptions = CreateSessionOptions();
            InitializeSession(modelPath, sessionOptions);
        }

        private SessionOptions CreateSessionOptions()
        {
            var options = new SessionOptions();
            
            // Deep Dive: Graph Optimization
            // ORT_ENABLE_ALL is crucial for edge deployment because it enables 
            // memory layout optimizations and node fusions (e.g., fusing Conv+ReLU) 
            // specific to the hardware backend. ORT_ENABLE_BASIC only performs 
            // trivial optimizations (like constant folding), which is insufficient 
            // for squeezing max performance out of limited edge hardware.
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            try
            {
                switch (_config.Provider)
                {
                    case ExecutionProviderType.CUDA:
                        options.AppendExecutionProvider_CUDA(_config.GpuDeviceId);
                        Console.WriteLine($"Initialized session with CUDA EP (Device: {_config.GpuDeviceId})");
                        break;
                    case ExecutionProviderType.TensorRT:
                        options.AppendExecutionProvider_TensorRT(_config.GpuDeviceId);
                        Console.WriteLine($"Initialized session with TensorRT EP");
                        break;
                    case ExecutionProviderType.OpenVINO:
                        options.AppendExecutionProvider_OpenVINO("");
                        Console.WriteLine($"Initialized session with OpenVINO EP");
                        break;
                    case ExecutionProviderType.CPU:
                    default:
                        // CPU is always available, but we explicitly append it for clarity
                        options.AppendExecutionProvider_CPU();
                        Console.WriteLine("Initialized session with CPU EP");
                        break;
                }
            }
            catch (OnnxRuntimeException ex)
            {
                // Edge Case Handling: Fallback
                Console.WriteLine($"Warning: Failed to initialize requested provider {_config.Provider}. Error: {ex.Message}");
                Console.WriteLine("Falling back to CPU provider.");
                options.ClearExecutionProviders();
                options.AppendExecutionProvider_CPU();
            }

            return options;
        }

        private void InitializeSession(string modelPath, SessionOptions options)
        {
            _session = new InferenceSession(modelPath, options);
        }

        public void BenchmarkInference(BertInput input, int iterations)
        {
            // Prepare input tensors (simplified for example)
            var inputTensors = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(input.InputIds, new[] { input.InputIds.Length / input.SequenceLength, input.SequenceLength })),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(input.AttentionMask, new[] { input.AttentionMask.Length / input.SequenceLength, input.SequenceLength }))
            };

            var stopwatch = new Stopwatch();
            double totalTimeMs = 0;

            Console.WriteLine($"Starting benchmark for {iterations} iterations...");

            for (int i = 0; i < iterations; i++)
            {
                stopwatch.Restart();
                using (var results = _session.Run(inputTensors))
                {
                    // Consume results to ensure full execution
                    foreach (var result in results) { /* Do nothing */ }
                }
                stopwatch.Stop();
                totalTimeMs += stopwatch.ElapsedMilliseconds;
            }

            double avgLatency = totalTimeMs / iterations;
            double throughput = (1000.0 / avgLatency) * (input.InputIds.Length / input.SequenceLength); // Inferences per second

            Console.WriteLine($"Average Latency: {avgLatency:F2} ms");
            Console.WriteLine($"Throughput: {throughput:F2} inferences/sec");
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
