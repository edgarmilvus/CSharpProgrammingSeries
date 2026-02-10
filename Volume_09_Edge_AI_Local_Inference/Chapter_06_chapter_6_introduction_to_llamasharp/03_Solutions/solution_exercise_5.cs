
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LlamaSharpExercises
{
    // Bridge for ONNX Runtime
    public class OnnxModelBridge : IDisposable
    {
        private InferenceSession? _session;
        private readonly string _modelPath;
        
        // Mock Tokenizer for demonstration
        private readonly Dictionary<string, int> _vocab = new Dictionary<string, int> 
        { 
            { "The", 1 }, { "meaning", 2 }, { "of", 3 }, { "life", 4 }, { "is", 5 } 
        };

        public OnnxModelBridge(string modelPath)
        {
            _modelPath = modelPath;
        }

        public void Load()
        {
            Console.WriteLine($"[ONNX] Loading model from: {_modelPath}");
            
            // Configure Session Options
            var options = new SessionOptions();
            
            // Attempt to use GPU if available (DirectML for Windows, CUDA for Linux)
            // For this exercise, we default to CPU for stability, but show the logic.
            try 
            {
                // options.AppendExecutionProvider_DML(0); // Windows GPU
                // options.AppendExecutionProvider_CUDA(0); // Linux GPU
            }
            catch 
            {
                Console.WriteLine("[ONNX] GPU provider not found, falling back to CPU.");
            }

            // Load the model
            _session = new InferenceSession(_modelPath, options);
            Console.WriteLine("[ONNX] Model loaded successfully.");
        }

        public int[] Tokenize(string text)
        {
            // Simple mock tokenizer - real implementation requires BPE/WordPiece logic
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                       .Select(word => _vocab.ContainsKey(word) ? _vocab[word] : 0)
                       .ToArray();
        }

        public float[] Infer(int[] inputIds)
        {
            if (_session == null) throw new InvalidOperationException("Model not loaded.");

            // Create Input Tensor (Shape: [1, SequenceLength])
            var tensor = new DenseTensor<long>(inputIds.Select(x => (long)x).ToArray(), new[] { 1, inputIds.Length });
            
            // Create NamedOnnxValue
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", tensor)
            };

            // Run Inference
            using var results = _session.Run(inputs);
            
            // Extract Logits (Output tensor usually shape [1, SeqLen, VocabSize])
            // For simplicity, we take the last token's logits
            var outputTensor = results.First().AsTensor<float>();
            
            // Flatten to 1D array for our sampler
            int vocabSize = outputTensor.Dimensions.Last();
            float[] logits = new float[vocabSize];
            
            // Get the logits for the last position in the sequence
            int lastTokenIndex = inputIds.Length - 1;
            for (int i = 0; i < vocabSize; i++)
            {
                // Accessing tensor data: [batch, seq, vocab]
                logits[i] = outputTensor[0, lastTokenIndex, i];
            }

            return logits;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }

    // Hybrid System Manager
    public class HybridInferenceSystem
    {
        private LlamaCppLib.LlamaModel? _llamaModel;
        private OnnxModelBridge? _onnxModel;
        private bool _llamaLoaded = false;
        private bool _onnxLoaded = false;

        public void Run()
        {
            Console.WriteLine("=== Hybrid Inference System ===");
            Console.WriteLine("Commands: /use llama, /use onnx, /load llama <path>, /load onnx <path>, exit");

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;

                var parts = input.Split(' ');
                var command = parts[0].ToLower();

                try
                {
                    if (command == "exit") break;

                    // Memory Check Simulation
                    long memoryUsage = GC.GetTotalMemory(false);
                    if (memoryUsage > 1_000_000_000) // 1GB threshold simulation
                    {
                        Console.WriteLine("[Warning] High memory usage detected. Unloading inactive models...");
                        UnloadInactiveModels(parts.Length > 2 ? parts[2] : "");
                    }

                    if (command == "/load")
                    {
                        if (parts.Length < 3) { Console.WriteLine("Usage: /load <type> <path>"); continue; }
                        string type = parts[1].ToLower();
                        string path = parts[2];

                        if (type == "llama")
                        {
                            // Load GGUF via LlamaSharp
                            var param = new LlamaCppLib.LlamaModelParams { Threads = 4, GpuLayers = 0 };
                            _llamaModel = new LlamaCppLib.LlamaModel(path, param);
                            _llamaLoaded = true;
                            Console.WriteLine("[System] LlamaSharp model loaded.");
                        }
                        else if (type == "onnx")
                        {
                            // Load ONNX
                            _onnxModel = new OnnxModelBridge(path);
                            _onnxModel.Load();
                            _onnxLoaded = true;
                            Console.WriteLine("[System] ONNX model loaded.");
                        }
                    }
                    else if (command == "/use")
                    {
                        if (parts.Length < 2) { Console.WriteLine("Usage: /use <type>"); continue; }
                        string type = parts[1].ToLower();

                        if (type == "llama" && _llamaLoaded)
                        {
                            Console.WriteLine("[System] Using LlamaSharp for next query.");
                            // Simulate inference
                            Console.WriteLine("Output: The meaning of life is 42."); 
                        }
                        else if (type == "onnx" && _onnxLoaded)
                        {
                            Console.WriteLine("[System] Using ONNX for next query.");
                            // Simulate inference
                            Console.WriteLine("Output: The meaning of life is complex.");
                        }
                        else
                        {
                            Console.WriteLine("[Error] Model not loaded or invalid type.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] {ex.Message}");
                }
            }
        }

        private void UnloadInactiveModels(string activeModel)
        {
            // Logic to dispose models not currently in use
            if (activeModel != "llama" && _llamaLoaded)
            {
                _llamaModel?.Dispose();
                _llamaLoaded = false;
                Console.WriteLine("[Memory] Unloaded LlamaSharp model.");
            }
            
            if (activeModel != "onnx" && _onnxLoaded)
            {
                _onnxModel?.Dispose();
                _onnxLoaded = false;
                Console.WriteLine("[Memory] Unloaded ONNX model.");
            }
        }
    }
}
