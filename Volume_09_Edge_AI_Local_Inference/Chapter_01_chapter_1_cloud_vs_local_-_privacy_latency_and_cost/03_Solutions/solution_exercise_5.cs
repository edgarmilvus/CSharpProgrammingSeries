
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OptimizedInference
{
    // 1. Singleton Pattern for Session Management
    public sealed class InferenceSessionProvider
    {
        private static InferenceSession? _session;
        private static readonly object _lock = new object();

        public static InferenceSession GetSession(string modelPath)
        {
            if (_session == null)
            {
                lock (_lock)
                {
                    if (_session == null)
                    {
                        _session = new InferenceSession(modelPath);
                    }
                }
            }
            return _session;
        }
    }

    public class OptimizedEngine
    {
        private readonly InferenceSession _session;

        public OptimizedEngine(string modelPath)
        {
            _session = InferenceSessionProvider.GetSession(modelPath);
        }

        // 2. Async & Batching
        public async Task<List<string>> RunBatchInferenceAsync(List<string> prompts)
        {
            // Return immediately if empty
            if (prompts == null || !prompts.Any()) return new List<string>();

            // Simulate CPU-bound work (tokenization) off the main thread
            // In a real scenario, tokenizer calls can be heavy.
            var inputTensors = await Task.Run(() =>
            {
                var batchInputIds = new List<List<int>>();
                foreach (var prompt in prompts)
                {
                    var tokens = new List<int> { 1 };
                    tokens.AddRange(prompt.Select(c => (int)c));
                    tokens.Add(2);
                    batchInputIds.Add(tokens);
                }

                // Padding logic (simplified): Pad to max length in batch
                int maxLen = batchInputIds.Max(t => t.Count);
                var paddedBatch = new List<int>();
                foreach (var seq in batchInputIds)
                {
                    // Pad with 0s
                    paddedBatch.AddRange(seq.Concat(Enumerable.Repeat(0, maxLen - seq.Count)));
                }

                // Shape: (BatchSize, SequenceLength)
                var shape = new long[] { batchInputIds.Count, maxLen };
                return new DenseTensor<int>(paddedBatch.ToArray(), shape);
            });

            // Prepare inputs
            var inputName = _session.InputMetadata.Keys.First();
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensors)
            };

            // Run Async (Note: ONNX Runtime RunAsync internally manages thread pool)
            using var results = await Task.Run(() => _session.Run(inputs));

            // Process outputs
            var outputs = new List<string>();
            var outputTensor = results.First().AsTensor<int>();
            
            // Extract batch results (Simplified extraction)
            for (int i = 0; i < outputTensor.Dimensions[0]; i++)
            {
                // Simulate detokenization
                outputs.Add($"Processed Batch Item {i + 1}");
            }

            return outputs;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            string modelPath = "phi-3-mini-4k-instruct.onnx";
            if (!System.IO.File.Exists(modelPath))
            {
                Console.WriteLine("Model file missing. Please provide a valid ONNX file.");
                return;
            }

            Console.WriteLine("Enter prompts (comma-separated):");
            string input = Console.ReadLine() ?? "Hello, how are you?, What is AI?, Explain gravity.";
            var prompts = input.Split(',').Select(p => p.Trim()).ToList();

            var engine = new OptimizedEngine(modelPath);

            // Warmup
            await engine.RunBatchInferenceAsync(new List<string> { "Warmup" });

            // Benchmark
            var sw = Stopwatch.StartNew();
            var results = await engine.RunBatchInferenceAsync(prompts);
            sw.Stop();

            Console.WriteLine($"\nProcessed {prompts.Count} prompts in {sw.ElapsedMilliseconds}ms.");
            Console.WriteLine($"Average latency per prompt: {(double)sw.ElapsedMilliseconds / prompts.Count:F2}ms");
            
            Console.WriteLine("\nResults:");
            foreach (var res in results)
            {
                Console.WriteLine($"- {res}");
            }
        }
    }
}
