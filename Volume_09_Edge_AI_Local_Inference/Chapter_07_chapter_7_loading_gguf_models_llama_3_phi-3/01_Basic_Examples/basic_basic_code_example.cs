
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

// ==========================================
// Edge AI: Local Inference with GGUF Models
// ==========================================
// This "Hello World" example demonstrates how to load a quantized GGUF model
// (specifically Microsoft's Phi-3 Mini) and perform text generation entirely
// locally within a .NET console application using ONNX Runtime GenAI.
//
// Real-World Context:
// Imagine you are building an IoT device (e.g., a smart home controller or 
// an industrial sensor) that needs to summarize sensor logs or generate 
// responses without sending data to the cloud. This code runs entirely 
// on the edge device's CPU, ensuring privacy, low latency, and offline capability.
//
// Prerequisites:
// 1. .NET 8.0 SDK or later.
// 2. NuGet Package: Microsoft.ML.OnnxRuntimeGenAI (v0.2.0 or later).
// 3. A downloaded GGUF model file (e.g., "Phi-3-mini-4k-instruct-q4.gguf").
//
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace EdgeAILocalInference
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Local GGUF Inference ===");
            
            // 1. Define the model path. 
            // In a real app, this might come from a config file or command line args.
            // We expect the user to place the GGUF file in the execution directory.
            string modelPath = "phi-3-mini-4k-instruct-q4.gguf";
            
            if (!File.Exists(modelPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Model file not found at '{modelPath}'.");
                Console.ResetColor();
                Console.WriteLine("Please download a Phi-3 GGUF model and place it in the output directory.");
                return;
            }

            try
            {
                // 2. Initialize the Model.
                // This loads the GGUF weights into memory and prepares the tokenizer.
                // ONNX Runtime GenAI handles the specific GGUF format parsing internally.
                using var model = new Model(modelPath);

                // 3. Initialize the Tokenizer.
                // The tokenizer converts text strings into numerical tokens that the model understands.
                using var tokenizer = new Tokenizer(model);

                // 4. Define the User Prompt.
                // We use a standard instruction format compatible with Phi-3.
                string prompt = "Write a haiku about coding in C# on the edge.";

                // 5. Tokenize the Input.
                // The tokenizer encodes the prompt into a sequence of token IDs.
                var tokenizerStream = tokenizer.CreateStream();
                var tokens = tokenizer.Encode(prompt);

                Console.WriteLine($"\nUser Prompt: {prompt}");
                Console.WriteLine("Generating response...\n");
                Console.ForegroundColor = ConsoleColor.Green;

                // 6. Configure Generation Parameters.
                // These settings control the randomness and length of the output.
                // 'max_length' limits the total tokens (input + output).
                // 'do_sample' enables stochastic sampling (creativity).
                var generatorParams = new GeneratorParams(model);
                generatorParams.SetSearchOption("max_length", 200);
                generatorParams.SetSearchOption("do_sample", true); 
                generatorParams.SetInputSequences(tokens);

                // 7. Initialize the Generator.
                // The generator manages the state during the decoding process.
                using var generator = new Generator(model, generatorParams);

                // 8. Run Inference Loop.
                // We generate tokens one by one to allow for streaming output.
                while (!generator.IsDone())
                {
                    // Compute the next token ID based on the current sequence.
                    generator.ComputeLogits();
                    
                    // Select the next token based on the configured search strategy (e.g., sampling).
                    generator.GenerateNextToken();

                    // Get the ID of the newly generated token.
                    // Note: In newer versions, we might get the sequence directly, 
                    // but iterating by index is the standard low-level approach.
                    ulong[] nextTokenIds = generator.GetSequence(0);
                    ulong nextTokenId = nextTokenIds[^1]; // Get the last token in the sequence

                    // Decode the token ID back to a string.
                    string nextToken = tokenizerStream.Decode(nextTokenId);

                    // Print the token immediately to simulate streaming.
                    Console.Write(nextToken);
                }

                Console.ResetColor();
                Console.WriteLine("\n\n=== Generation Complete ===");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
