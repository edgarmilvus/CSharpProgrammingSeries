
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalAIChat
{
    // Real-world context: A field service technician needs an offline assistant
    // to diagnose equipment issues. The app runs entirely on a laptop without
    // internet connectivity, using local ONNX models for safety and privacy.
    class Program
    {
        // Configuration constants for the local environment
        const string ModelUrl = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/resolve/main/cpu_and_gpu/cpu-int4-rtn-block-32-acc-level-4.onnx";
        const string ModelFileName = "phi-3-mini-4k-instruct.onnx";
        const string ModelDirectory = "./models";
        const string ModelPath = ModelDirectory + "/" + ModelFileName;

        // HTTP client for downloading models (only used during setup)
        static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Local AI Diagnostic Assistant ===");
            Console.WriteLine("Running entirely offline with ONNX Runtime\n");

            // Step 1: Ensure the local environment is set up
            await SetupLocalEnvironment();

            // Step 2: Initialize the ONNX Runtime session
            // This loads the model into memory for inference
            Console.WriteLine("Initializing Inference Engine...");
            using var session = new InferenceSession(ModelPath);

            // Step 3: Main interaction loop
            while (true)
            {
                Console.WriteLine("\n[Technician] Enter a diagnostic query (or 'exit' to quit):");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
                {
                    break;
                }

                // Step 4: Preprocess input for the model
                // In a real scenario, we would tokenize the input string
                // For this example, we simulate the tokenization process
                int[] inputTokens = Tokenize(input);

                // Step 5: Create input tensors for the model
                // ONNX Runtime requires specific tensor shapes and data types
                using var inputTensors = CreateInputTensors(inputTokens);

                // Step 6: Run inference
                Console.WriteLine("Processing query locally...");
                var results = session.Run(inputTensors);

                // Step 7: Post-process the output
                string response = DecodeOutput(results);
                
                Console.WriteLine($"\n[Assistant] {response}");
            }

            Console.WriteLine("\nSession terminated. Local data remains on disk.");
        }

        // ---------------------------------------------------------
        // Method: SetupLocalEnvironment
        // Purpose: Ensures the ONNX model exists locally before attempting to load it
        // ---------------------------------------------------------
        static async Task SetupLocalEnvironment()
        {
            // Check if the model directory exists
            if (!Directory.Exists(ModelDirectory))
            {
                Console.WriteLine($"Creating model directory: {ModelDirectory}");
                Directory.CreateDirectory(ModelDirectory);
            }

            // Check if the model file already exists
            if (File.Exists(ModelPath))
            {
                Console.WriteLine($"Model found at: {ModelPath}");
                return;
            }

            // If model is missing, download it
            Console.WriteLine("Model not found. Downloading from Hugging Face...");
            Console.WriteLine("This may take several minutes depending on internet speed.");

            try
            {
                // Download the model stream
                using var response = await httpClient.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // Write to local disk
                using var fileStream = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fileStream);

                Console.WriteLine("Model downloaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading model: {ex.Message}");
                Console.WriteLine("Please ensure you have internet connectivity for the initial setup.");
                Environment.Exit(1);
            }
        }

        // ---------------------------------------------------------
        // Method: Tokenize
        // Purpose: Converts a string input into a sequence of integers (tokens)
        // Note: In a production environment, this would use a BPE tokenizer.
        // ---------------------------------------------------------
        static int[] Tokenize(string input)
        {
            // Simple simulation: Convert characters to their ASCII values
            // This is a placeholder for the actual tokenization logic
            var tokens = new List<int>();
            
            // Add a start token (simulated as 1)
            tokens.Add(1);

            foreach (char c in input)
            {
                tokens.Add((int)c);
            }

            // Add an end token (simulated as 2)
            tokens.Add(2);

            return tokens.ToArray();
        }

        // ---------------------------------------------------------
        // Method: CreateInputTensors
        // Purpose: Prepares the input data in the format expected by the ONNX model
        // ---------------------------------------------------------
        static List<NamedOnnxValue> CreateInputTensors(int[] tokens)
        {
            // ONNX models typically expect a tensor of shape [batch_size, sequence_length]
            // For Phi-3, the input name is usually "input_ids"
            
            // Create a dense tensor with the token IDs
            var tensor = new DenseTensor<long>(new long[tokens.Length], [1, tokens.Length]);
            
            for (int i = 0; i < tokens.Length; i++)
            {
                tensor.SetValue(0, i, tokens[i]);
            }

            // Create the NamedOnnxValue list
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", tensor)
            };

            return inputs;
        }

        // ---------------------------------------------------------
        // Method: DecodeOutput
        // Purpose: Converts the model's numerical output back into human-readable text
        // ---------------------------------------------------------
        static string DecodeOutput(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            // The output tensor contains logits (scores) for the next token
            // In a real application, we would select the token with the highest score
            // and loop until an end-of-sequence token is generated.
            
            // For this example, we simulate a response based on the output shape
            var outputTensor = results[0].AsTensor<long>();
            
            // Extract the shape to verify data
            var shape = outputTensor.Dimensions;
            
            // Simulate a response based on the processing
            // In reality, we would map the token IDs back to text
            if (shape[0] > 0 && shape[1] > 0)
            {
                return "I have processed your query locally. (Simulation: Output tensor shape is " + 
                       $"{shape[0]}x{shape[1]}. In production, this would be decoded into text.)";
            }
            
            return "I could not generate a response.";
        }
    }
}
