
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
// Note: Assuming Microsoft.ML.Tokenizers is available for BPE
// If not, we mock the tokenizer interface for this solution.
using Microsoft.ML.Tokenizers; 

namespace SmartLlamaPipeline
{
    public class SmartLoader
    {
        // 1. System Check (Cross-platform approximation)
        public static long GetAvailablePhysicalMemory()
        {
            // In a real .NET 8 app, you might use System.Management or native calls
            // For this exercise, we simulate checking RAM.
            // Using GC.GetTotalMemory(false) is not accurate for physical RAM, 
            // but we will mock the threshold logic.
            
            // Real implementation would use:
            // using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            // For Linux: reading /proc/meminfo
            
            // Mocking 6GB available for demonstration
            return 6L * 1024 * 1024 * 1024; 
        }

        // 2. Dynamic Loader
        public static string SelectModelPath()
        {
            long availableRam = GetAvailablePhysicalMemory();
            Console.WriteLine($"Available RAM: {availableRam / (1024*1024*1024)} GB");

            if (availableRam < 4L * 1024 * 1024 * 1024)
                return "llama_int4.onnx";
            else if (availableRam < 8L * 1024 * 1024 * 1024)
                return "llama_int8.onnx";
            else
                return "llama_fp16.onnx";
        }
    }

    public class OptimizedInference
    {
        private InferenceSession? _session;
        private readonly Tokenizer _tokenizer;
        private List<int> _kvCacheInputIds = new List<int>();

        public OptimizedInference()
        {
            // 3. Tokenizer Integration (Mocking BPE setup)
            // In a real scenario, we load a tokenizer.json or use Tiktoken
            var vocab = new Dictionary<string, int> { { "<s>", 1 }, { "</s>", 2 }, { "user", 3 } };
            var merges = new List<string>();
            var bpe = new BpeTokenizer(vocab, merges);
            _tokenizer = new Tokenizer(bpe);
        }

        public void LoadModel()
        {
            string modelPath = SmartLoader.SelectModelPath();
            Console.WriteLine($"Loading model: {modelPath}");

            var options = new SessionOptions();
            options.AppendExecutionProvider_CPU();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            
            // Dispose previous session if switching
            _session?.Dispose();
            _session = new InferenceSession(modelPath, options);
        }

        // 4. KV Cache Optimization
        // Note: Standard ONNX Runtime C# API doesn't natively manage KV cache state objects 
        // without specific model outputs (like 'past_key_values'). 
        // This code simulates the logic of passing the cache.
        public void Generate(string prompt)
        {
            if (_session == null) throw new InvalidOperationException("Model not loaded.");

            // Tokenize
            var encoded = _tokenizer.Encode(prompt);
            var inputIds = encoded.Ids.ToArray();

            // Prepare Inputs
            // Standard input
            var inputTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
            var inputs = new List<NamedOnnxValue> 
            { 
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor) 
            };

            // If KV Cache exists from previous run, we would add it here.
            // For this exercise, we assume the model expects 'input_ids' and optionally 'past_key_values'
            // Since we don't have a real model file, we simulate the loop.
            
            var sw = Stopwatch.StartNew();
            
            // Cold Start (First token)
            using var results = _session.Run(inputs);
            sw.Stop();
            Console.WriteLine($"Cold Start (First Token): {sw.ElapsedMilliseconds}ms");

            // Warm Start (Subsequent tokens)
            // In a real loop, we update inputIds to be the last token + KV cache
            sw.Restart();
            using var results2 = _session.Run(inputs);
            sw.Stop();
            Console.WriteLine($"Warm Start (Next Token): {sw.ElapsedMilliseconds}ms");

            // 6. Disposal is handled by 'using' statements and the class Dispose pattern
        }

        public void Dispose() => _session?.Dispose();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var pipeline = new OptimizedInference();
            
            // 5. Performance Logging
            try
            {
                pipeline.LoadModel();
                pipeline.Generate("User: Hello, how are you?");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pipeline Error: {ex.Message}");
            }
        }
    }
    
    // Mock BPE Tokenizer for compilation
    public class BpeTokenizer : ITokenizer
    {
        public BpeTokenizer(Dictionary<string, int> vocab, List<string> merges) { }
        public TokenizerEncodeResult Encode(string text) => new TokenizerEncodeResult(new int[] { 1, 2, 3 }, null);
    }
}
