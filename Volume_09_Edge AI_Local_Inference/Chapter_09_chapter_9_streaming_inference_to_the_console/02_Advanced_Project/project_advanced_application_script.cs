
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalInferenceStreamer
{
    // Real-World Context:
    // Imagine a scenario where a developer is building a "Code Commenter" tool.
    // Instead of waiting for the entire LLM response to generate (which might take seconds),
    // the developer wants to stream the comments directly into the console as they are generated.
    // This provides immediate feedback and mimics the experience of a real-time chatbot.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing Local LLM Inference Streamer...");
            Console.WriteLine("--------------------------------------------------");

            // 1. SETUP: Define the model path and tokenizer vocabulary.
            // In a production app, the vocabulary would be loaded from a JSON file.
            // Here, we simulate a small vocabulary for the "Phi-2" or similar small model context.
            string modelPath = "phi-2-quantized.onnx"; // Assumed file path
            var vocab = LoadMockVocabulary();

            // 2. INPUT: Define the user prompt.
            string prompt = "Write a C# function to calculate Fibonacci numbers: ";
            Console.WriteLine($"User Prompt: {prompt}");

            // 3. INITIALIZATION: Load the ONNX model and prepare the inference session.
            // We wrap this in a try-catch block to handle missing model files gracefully.
            InferenceSession session;
            try
            {
                // Note: In a real scenario, we would configure SessionOptions for hardware acceleration.
                var options = new SessionOptions();
                session = new InferenceSession(modelPath, options);
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[!] Model file '{modelPath}' not found.");
                Console.WriteLine("    Running in DEMO mode (simulated streaming output).");
                Console.ResetColor();
                await RunDemoSimulation(prompt);
                return;
            }

            // 4. TOKENIZATION: Convert text prompt to token IDs.
            // This is a simplified tokenizer logic for demonstration.
            List<int> inputIds = Tokenize(prompt, vocab);

            // 5. STREAMING INFERENCE LOOP
            Console.WriteLine("\nGenerating Response (Streaming):");
            Console.ForegroundColor = ConsoleColor.Cyan;

            // We generate up to 50 tokens or until the End-of-Sequence token is predicted.
            int maxTokens = 50;
            int eosTokenId = vocab["<|end|>"]; // Hypothetical End of Sequence token

            for (int i = 0; i < maxTokens; i++)
            {
                // A. Prepare Input Tensor
                // ONNX Runtime expects a Tensor<int> of shape [1, sequence_length]
                var inputTensor = new DenseTensor<int>(inputIds.ToArray(), [1, inputIds.Count]);
                var inputName = session.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };

                // B. Run Inference (Async)
                // This executes the model locally. In a heavy model, this is the blocking part.
                // For true async execution on CPU, we run it on a ThreadPool thread.
                List<NamedOnnxValue> outputs = await Task.Run(() =>
                {
                    return session.Run(inputs).ToList();
                });

                // C. Process Output Tensor
                // The output is usually a tensor of logits [1, vocab_size] or the last predicted token.
                // Here we assume the model outputs the next token ID directly for simplicity.
                var outputName = session.OutputMetadata.Keys.First();
                var outputTensor = outputs.First(x => x.Name == outputName).AsTensor<int>();
                
                // Extract the last predicted token ID
                int predictedTokenId = outputTensor.GetValue(outputTensor.Length - 1); // Simplified extraction

                // D. Decode Token to Text
                string decodedToken = Decode(predictedTokenId, vocab);

                // E. Stream to Console
                // We do not use Console.WriteLine to avoid new lines. 
                // We use Write and Flush to ensure characters appear immediately.
                Console.Write(decodedToken);
                Console.Out.Flush(); 

                // F. Check for Stop Condition
                if (predictedTokenId == eosTokenId)
                {
                    break;
                }

                // G. Update Input for Next Iteration (Autoregressive)
                // Append the new token to the input sequence for the next inference step.
                inputIds.Add(predictedTokenId);

                // H. Simulate Latency for Demo (Optional)
                // Real inference has latency; this prevents the console from flashing too fast.
                await Task.Delay(50); 
            }

            Console.ResetColor();
            Console.WriteLine("\n\nInference Complete.");
        }

        // --- Helper Methods ---

        /// <summary>
        /// Loads a mock vocabulary map for token ID lookup.
        /// </summary>
        static Dictionary<string, int> LoadMockVocabulary()
        {
            return new Dictionary<string, int>
            {
                { "def", 100 }, { " ", 101 }, { "fib", 102 }, { "(", 103 }, { "n", 104 },
                { ")", 105 }, { ":", 106 }, { "\n", 107 }, { "    ", 108 }, { "if", 109 },
                { "<|end|>", 200 }, { "x", 201 }, { "=", 202 }, { "0", 203 }, { "1", 204 },
                { "return", 205 }, { "else", 206 }, { "elif", 207 }, { ">", 208 }, { "<", 209 }
            };
        }

        /// <summary>
        /// Converts a string prompt into a list of integer Token IDs.
        /// </summary>
        static List<int> Tokenize(string text, Dictionary<string, int> vocab)
        {
            var tokens = new List<int>();
            // Simple whitespace splitting for demonstration
            var words = text.Split(' '); 
            
            foreach (var word in words)
            {
                if (vocab.ContainsKey(word))
                {
                    tokens.Add(vocab[word]);
                }
                else
                {
                    // Handle unknown tokens by mapping to a default ID (e.g., 0)
                    tokens.Add(0); 
                }
            }
            return tokens;
        }

        /// <summary>
        /// Converts a Token ID back to a readable string.
        /// </summary>
        static string Decode(int tokenId, Dictionary<string, int> vocab)
        {
            // Inverse lookup: Find key by value. 
            // Note: This is inefficient O(N) but fine for small vocab demos.
            // Production tokenizers use optimized lookup tables.
            foreach (var kvp in vocab)
            {
                if (kvp.Value == tokenId)
                {
                    return kvp.Key;
                }
            }
            return "?"; // Unknown token
        }

        /// <summary>
        /// Fallback simulation if the ONNX model file is missing.
        /// </summary>
        static async Task RunDemoSimulation(string prompt)
        {
            Console.WriteLine("\nGenerating Response (Simulated):");
            Console.ForegroundColor = ConsoleColor.Green;
            
            string[] simulatedTokens = ["def", " ", "fib", "(", "n", ")", ":", "\n", "    ", "if", " ", "n", " ", "<", " ", "2", ":", "\n", "        ", "return", " ", "n"];
            
            foreach (var token in simulatedTokens)
            {
                Console.Write(token);
                Console.Out.Flush();
                await Task.Delay(80); // Human-like typing speed
            }
            
            Console.ResetColor();
        }
    }
}
