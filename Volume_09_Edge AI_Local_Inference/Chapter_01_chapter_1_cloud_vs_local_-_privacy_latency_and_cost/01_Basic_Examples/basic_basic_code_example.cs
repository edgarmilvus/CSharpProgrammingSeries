
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

// Requires the following NuGet packages:
// Microsoft.ML.OnnxRuntime (v1.17.1 or later)
// Microsoft.ML.OnnxRuntime.Managed (v1.17.1 or later)

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalPhiSentiment
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Define the Input Data
            // In a real app, this would come from a UI or database.
            // We are simulating a user's private feedback.
            string userFeedback = "The new update completely drains my battery! I hate it.";

            // 2. Setup Model Paths
            // NOTE: You must download a Phi-3 ONNX model (e.g., from HuggingFace)
            // and place it at this path. For this example, we assume the file exists.
            // We will use the 'quantized' version for better performance on CPU.
            string modelPath = @"phi-3-mini-4k-instruct-onnx/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4.onnx";

            Console.WriteLine("--- Local Edge AI Inference (ONNX Runtime) ---");
            Console.WriteLine($"Input: \"{userFeedback}\"");
            Console.WriteLine($"Model: {Path.GetFileName(modelPath)}");
            
            try 
            {
                // 3. Initialize the Inference Session
                // This loads the model from disk into memory.
                // We use 'using' to ensure resources are disposed of correctly.
                using var session = new InferenceSession(modelPath);

                // 4. Pre-process: Tokenize
                // LLMs don't understand strings; they understand numbers (tokens).
                // We need a tokenizer to convert "Hello" -> [287, 123].
                // Since we don't have a separate tokenizer file here, we simulate 
                // the tokenization for the specific prompt format Phi-3 expects.
                // Phi-3 Format: "<|user|>\n{prompt}<|end|>\n<|assistant|>"
                string prompt = $"<|user|>\n{userFeedback}<|end|>\n<|assistant|>";
                
                // In a real app, use the 'Microsoft.ML.OnnxRuntime.GenAI' tokenizer or 
                // Microsoft.ML.Tokenizers library. Here, we mock the token IDs for demonstration.
                // "The" -> 452, "new" -> 645, etc. (Simplified for the example logic)
                int[] inputTokenIds = MockTokenizerEncode(prompt); 

                // 5. Create Input Tensors
                // ONNX Runtime expects tensors. We create a DenseTensor for the input IDs.
                // Shape: [BatchSize (1), SequenceLength (number of tokens)]
                var inputTensor = new DenseTensor<long>(inputTokenIds.Select(x => (long)x).ToArray(), [1, inputTokenIds.Length]);

                // 6. Prepare Input Bindings
                // We map the tensor to the input name expected by the model.
                // Standard names for CausalLM models are usually 'input_ids'.
                var inputName = session.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                };

                // 7. Run Inference
                // This is the heavy lifting. The CPU (or GPU) calculates the probabilities.
                using var results = session.Run(inputs);

                // 8. Post-process: Extract Logits
                // The output is a massive array of raw scores (logits) for every word in the vocabulary.
                // We need to find the 'Next Token' probability to see what the model wants to say.
                var outputTensor = results.First().AsTensor<float>();
                
                // Find the ID of the token with the highest score (Greedy Search)
                int predictedTokenId = GetTopToken(outputTensor);

                // 9. Decode Output
                // Convert the predicted token ID back to text.
                // For this example, we will just loop to generate a few tokens to prove it works.
                Console.Write("\nModel Response: ");
                Console.ForegroundColor = ConsoleColor.Green;
                
                // Simple generation loop (generates up to 20 tokens)
                for (int i = 0; i < 20; i++)
                {
                    // Create input for the next step (shift window)
                    // In a real loop, we append the new token to the input_ids and run again.
                    // To keep this example short, we will just print what the first prediction implies.
                    // Usually, if the first token is a positive word, the sentiment is positive.
                    
                    string decodedToken = MockTokenizerDecode(predictedTokenId);
                    Console.Write(decodedToken + " ");

                    if (predictedTokenId == 32000) // <|end|> token
                        break;
                }
                Console.ResetColor();
                Console.WriteLine("\n\n--- Inference Complete ---");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nTroubleshooting: Ensure you have downloaded the ONNX model file and updated the 'modelPath' variable.");
            }
        }

        // --- Helper Methods (Simulating Tokenizer Logic for Self-Contained Example) ---

        static int[] MockTokenizerEncode(string text)
        {
            // Extremely simplified tokenizer simulation.
            // In reality, this uses a vocabulary file (tokenizer.json).
            // We map specific words to IDs to make the math work for the demo.
            var vocab = new Dictionary<string, int>
            {
                { "<|user|>", 32010 }, { "<|end|>", 32000 }, { "<|assistant|>", 32001 },
                { "The", 452 }, { "new", 645 }, { "update", 3369 }, { "drains", 18465 },
                { "battery", 16415 }, { "hate", 9675 }, { "it", 306 }, { "love", 1019 }
            };
            
            // Split text by space to approximate tokenization
            var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var tokens = new List<int>();
            foreach (var word in words)
            {
                if (vocab.TryGetValue(word, out int token))
                    tokens.Add(token);
                else
                    tokens.Add(500); // Unknown token fallback
            }
            return tokens.ToArray();
        }

        static string MockTokenizerDecode(int tokenId)
        {
            // Reverse lookup for demo output
            if (tokenId == 32000) return "";
            if (tokenId == 32010) return "\nUser: ";
            if (tokenId == 32001) return "\nAssistant: ";
            if (tokenId == 452) return "It";
            if (tokenId == 645) return "is";
            if (tokenId == 18465) return "terrible";
            if (tokenId == 9675) return "bad";
            if (tokenId == 1019) return "great";
            if (tokenId == 306) return ".";
            return "?";
        }

        static int GetTopToken(Tensor<float> tensor)
        {
            // The tensor shape is [1, SequenceLength, VocabularySize]
            // We are looking at the last token's logits (the next token prediction)
            // For this simplified example, we scan the last sequence position.
            
            // Find the index of the maximum value in the last dimension
            int vocabSize = tensor.Dimensions[^1]; // Last dimension size
            int lastTokenOffset = (tensor.Dimensions[^2] - 1) * vocabSize; // Start of last token
            
            float maxVal = float.MinValue;
            int maxIndex = -1;

            // Iterate over the vocabulary scores for the last token
            for (int i = 0; i < vocabSize; i++)
            {
                float val = tensor.GetValue(lastTokenOffset + i);
                if (val > maxVal)
                {
                    maxVal = val;
                    maxIndex = i;
                }
            }
            return maxIndex;
        }
    }
}
