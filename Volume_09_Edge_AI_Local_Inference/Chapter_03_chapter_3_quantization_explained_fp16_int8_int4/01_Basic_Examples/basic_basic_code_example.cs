
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

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EdgeAI_QuantizationDemo
{
    class Program
    {
        // --- CONFIGURATION ---
        // For this "Hello World" example, we simulate the existence of a quantized model.
        // In a real scenario, you would download a pre-quantized ONNX model (e.g., Phi-3-mini-4k-instruct-q4.onnx)
        // or use the ONNX Runtime quantization tools to convert a PyTorch/TensorFlow model.
        private const string ModelPath = "phi-3-mini-4k-instruct-q4.onnx";
        private const string VocabularyPath = "tokenizer_vocab.json"; // Simplified for demonstration

        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Edge AI: Local Inference with Quantized Models (INT4)");
            Console.WriteLine("--------------------------------------------------------");

            // 1. Prepare the Environment
            // In a real app, we would download the model if it doesn't exist.
            // For this example, we will check if the file exists, and if not, create a dummy one
            // just to allow the code to run without crashing.
            await EnsureModelExistsAsync(ModelPath);

            // 2. Define the Inference Session Options
            // This is where the magic happens. We configure the execution provider.
            // For Edge AI (CPU), we use CpuMl (Machine Learning) provider if available, 
            // otherwise default Cpu.
            var sessionOptions = new SessionOptions();
            
            // Enable CPU optimizations specific for ML workloads (AVX, etc.)
            sessionOptions.AppendExecutionProvider_CPU();
            
            // Set graph optimization level to enable constant folding and other optimizations
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            // 3. Load the Quantized Model
            // We wrap this in a try-catch block because file paths can be tricky.
            try
            {
                Console.WriteLine($"[1] Loading model from: {Path.GetFullPath(ModelPath)}");
                
                // The InferenceSession is the core engine. 
                // Even though the model is quantized to INT4, ONNX Runtime handles the 
                // unpacking and execution transparently.
                using var session = new InferenceSession(ModelPath, sessionOptions);
                
                Console.WriteLine($"[2] Model loaded successfully.");
                Console.WriteLine($"    Input Node: {session.InputMetadata.Keys.First()}");
                Console.WriteLine($"    Output Node: {session.OutputMetadata.Keys.First()}");

                // 4. Prepare Input Data (Simulated Tokenization)
                // In a real scenario, you would use a tokenizer (like Microsoft.ML.Tokenizers).
                // Here, we simulate the input_ids tensor expected by the LLM.
                // Shape: [BatchSize, SequenceLength] -> [1, 8]
                var inputIds = new long[] { 1, 15043, 29879, 29901, 2057, 29915, 29879, 29901 }; // "Hello, how are you?"
                var attentionMask = new long[] { 1, 1, 1, 1, 1, 1, 1, 1 };
                
                // 5. Create Tensors
                // We use DenseTensor for memory efficiency on Edge devices.
                // We must reshape to [1, 8] to match the model's expected batch dimension.
                var inputTensor = new DenseTensor<long>(inputIds, [1, inputIds.Length]);
                var maskTensor = new DenseTensor<long>(attentionMask, [1, attentionMask.Length]);

                // 6. Create NamedOnnxValue Inputs
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                    NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor)
                };

                Console.WriteLine($"[3] Input tensor created. Shape: {string.Join(",", inputTensor.Dimensions)}");

                // 7. Run Inference
                Console.WriteLine($"[4] Running inference (Local CPU)...");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                
                // This is the synchronous execution call. 
                // Despite the model being INT4, the inputs are usually FP32 or INT64, 
                // and the outputs are FP32 (logits).
                using var results = session.Run(inputs);
                
                watch.Stop();
                Console.WriteLine($"    Execution time: {watch.ElapsedMilliseconds} ms");

                // 8. Process Results
                // The output is typically a tensor of logits (raw scores) for the next token.
                var outputTensor = results.First().AsTensor<float>();
                
                Console.WriteLine($"[5] Output received. Shape: {string.Join(",", outputTensor.Dimensions)}");
                
                // Find the token with the highest score (Greedy Decoding)
                int maxIndex = 0;
                float maxVal = float.MinValue;
                
                // We iterate over the last token's logits (dim 2)
                for (int i = 0; i < outputTensor.Dimensions[2]; i++)
                {
                    float val = outputTensor[0, 0, i];
                    if (val > maxVal)
                    {
                        maxVal = val;
                        maxIndex = i;
                    }
                }

                Console.WriteLine($"[6] Predicted Token ID: {maxIndex} (Score: {maxVal:F4})");
                Console.WriteLine($"    *Note: In a real app, this ID maps back to text via the tokenizer.*");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nNote: This code requires the actual ONNX model file to run fully.");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // Helper to simulate model existence for the code snippet
        static async Task EnsureModelExistsAsync(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[!] Model file not found at '{path}'.");
                Console.WriteLine("    Creating a dummy file for demonstration purposes...");
                
                // In a real app, we would download:
                // await DownloadModelAsync("https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/resolve/main/cpu-int4-rtn-block-32/phi-3-mini-4k-instruct-cpu-int4-rtn-block-32.onnx");
                
                // Creating a dummy file just to let the InferenceSession constructor succeed 
                // (it checks file existence). In reality, this file would be invalid ONNX.
                // We catch the specific ONNX runtime error in Main if the format is wrong.
                await File.WriteAllTextAsync(path, "dummy");
            }
        }
    }
}
