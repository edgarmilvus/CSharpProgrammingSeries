
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalInferenceApp
{
    public class LocalInferenceEngine : IDisposable
    {
        private readonly InferenceSession _session;

        public LocalInferenceEngine(string modelPath)
        {
            // Load the ONNX model from disk.
            // In a real scenario, we might load execution providers (e.g., CUDA, CPU) here.
            var sessionOptions = new SessionOptions();
            sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
            
            _session = new InferenceSession(modelPath, sessionOptions);
            Console.WriteLine($"Model loaded: {modelPath}");
        }

        public string RunInference(string prompt)
        {
            // 1. Tokenize (Simulated)
            // In a real SLM, we would use a specific tokenizer (e.g., Tiktoken).
            // Here, we simulate by converting characters to their integer ASCII values.
            // We also add a BOS (Beginning of Sequence) token simulation (e.g., 1).
            var inputIds = new List<int> { 1 }; 
            inputIds.AddRange(prompt.Select(c => (int)c));
            
            // Add an EOS (End of Sequence) token simulation (e.g., 2).
            inputIds.Add(2);

            // 2. Create Input Tensor
            // ONNX models usually expect inputs in specific shapes (BatchSize, SequenceLength).
            // Shape: (1, sequence_length)
            var shape = new long[] { 1, inputIds.Count };
            
            // Create a dense tensor.
            var tensor = new DenseTensor<int>(inputIds.ToArray(), shape);

            // 3. Prepare Inputs
            // We map the tensor to the input name expected by the model.
            // Usually "input_ids" for transformer models.
            var inputName = _session.InputMetadata.Keys.First();
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, tensor)
            };

            // 4. Run Session
            // Using IDisposableCollection to manage memory of outputs.
            using var results = _session.Run(inputs);

            // 5. Extract Output
            // Assuming the model outputs logits or token IDs.
            // We will grab the first output tensor.
            var outputTensor = results.First().AsTensor<int>();
            
            // 6. Detokenize (Simulated)
            // Convert integers back to characters.
            var outputString = "";
            foreach (var token in outputTensor.ToArray())
            {
                // Skip special tokens (1 and 2) for cleaner output
                if (token > 2 && token < 127) 
                {
                    outputString += (char)token;
                }
                else if (token == 2) 
                {
                    break; // Stop at EOS
                }
            }

            return outputString;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // NOTE: Ensure 'phi-3-mini-4k-instruct.onnx' exists in the execution directory.
            // If not, this will throw a FileNotFoundException.
            string modelPath = "phi-3-mini-4k-instruct.onnx";
            
            if (!System.IO.File.Exists(modelPath))
            {
                Console.WriteLine($"Error: Model file '{modelPath}' not found.");
                Console.WriteLine("Please ensure the ONNX model is in the output directory.");
                return;
            }

            try
            {
                using var engine = new LocalInferenceEngine(modelPath);
                
                Console.WriteLine("Enter a prompt:");
                string prompt = Console.ReadLine() ?? "What is the capital of France?";

                string result = engine.RunInference(prompt);
                
                Console.WriteLine("\n--- Inference Result ---");
                Console.WriteLine($"Input: {prompt}");
                Console.WriteLine($"Output: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
