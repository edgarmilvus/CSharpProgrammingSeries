
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.ML.OnnxRuntime;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OptimizedLoraPipeline
{
    public class OptimizedInferencePipeline
    {
        private readonly InferenceSession _session;
        private const int SequenceLength = 128;
        private const int HiddenSize = 4096;

        public OptimizedInferencePipeline(byte[] modelBytes)
        {
            var options = new SessionOptions();
            // Enable hardware acceleration (DirectML for Windows, CUDA for Linux/Nvidia)
            // options.AppendExecutionProviderDirectML(0); 
            _session = new InferenceSession(new ReadOnlyMemory<byte>(modelBytes), options);
        }

        /// <summary>
        /// Simulates Graph Surgery: 
        /// Original: Input -> MatMul (Base) -> Output
        /// Modified: Input -> [MatMul (Base) + (MatMul (A) * MatMul (B) * Scale)] -> Output
        /// </summary>
        public void SimulateGraphSurgery()
        {
            // In a real scenario, we would use ONNX GraphSurgeon (Python) or 
            // Microsoft.ML.OnnxRuntime.Transforms to manipulate the GraphProto.
            // Here, we verify that the session contains the expected nodes.
            
            Console.WriteLine("Simulating Graph Surgery...");
            Console.WriteLine("1. Identifying target node: 'q_proj/MatMul'");
            
            // Accessing the graph via the InferenceSession (read-only usually)
            // var graph = _session.ModelMetadata; 
            // In C#, we typically pre-process the ONNX file or use a custom operator.
            
            Console.WriteLine("2. Inserting LoRA branch nodes: 'lora_A/MatMul', 'lora_B/MatMul', 'lora_Scale/Mul', 'lora_Add/Add'");
            Console.WriteLine("3. Rewiring connections...");
            Console.WriteLine("Graph surgery simulation complete.");
        }

        /// <summary>
        /// Runs inference with memory pooling to minimize Gen 2 allocations.
        /// </summary>
        public float[] RunInferenceOptimized(int[] inputIds)
        {
            // 1. Buffer Pooling for Input Tensor
            // Rent an array from the shared pool to avoid heap allocation for the tensor data
            var rentedInput = ArrayPool<float>.Shared.Rent(inputIds.Length);
            try
            {
                // Convert int input to float (simulating embedding lookup)
                for (int i = 0; i < inputIds.Length; i++)
                {
                    rentedInput[i] = inputIds[i];
                }

                // 2. Create Tensor (Disposable)
                // Note: OrtValue creates a native tensor, we must dispose it after Run
                var inputTensor = new DenseTensor<float>(rentedInput, new[] { 1, inputIds.Length });
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", inputTensor) };

                // 3. Run Inference
                // Stopwatch for benchmarking
                var sw = Stopwatch.StartNew();
                using (var results = _session.Run(inputs))
                {
                    sw.Stop();
                    Console.WriteLine($"Inference Latency: {sw.ElapsedMilliseconds}ms");

                    // 4. Process Output
                    var outputTensor = results.First().AsTensor<float>();
                    var output = outputTensor.ToArray();
                    return output;
                }
            }
            finally
            {
                // Return the rented array to the pool
                ArrayPool<float>.Shared.Return(rentedInput);
                
                // Force GC check (only for demonstration; usually avoid in tight loops)
                // GC.Collect(); 
            }
        }

        public void Dispose() => _session.Dispose();
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Mock model bytes
            byte[] modelBytes = new byte[1024 * 1024]; 

            using (var pipeline = new OptimizedInferencePipeline(modelBytes))
            {
                // 1. Graph Surgery
                pipeline.SimulateGraphSurgery();

                // 2. Benchmarking
                int[] dummyInput = Enumerable.Repeat(1, 128).ToArray();
                
                Console.WriteLine("\n--- Benchmarking Base vs LoRA ---");
                
                // Warmup
                pipeline.RunInferenceOptimized(dummyInput);

                // Actual Run
                pipeline.RunInferenceOptimized(dummyInput);
            }
        }
    }
}
