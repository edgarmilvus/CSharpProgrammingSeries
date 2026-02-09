
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

// This example demonstrates running a local ONNX model (Phi-3 Mini) for text generation.
// Prerequisites:
// 1. Install NuGet package: Microsoft.ML.OnnxRuntime
// 2. Download a Phi-3 Mini ONNX model (e.g., from Hugging Face) and place it in a folder named "models".
//    Ensure you have the 'tokenizer.json' in the same folder for proper token decoding.
public class LocalLlmInference
{
    public static void Main()
    {
        Console.WriteLine("Initializing Local AI Assistant...");

        // Path to the ONNX model file.
        // Note: In a real app, use Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "phi-3-mini.onnx")
        string modelPath = "models/phi-3-mini.onnx";

        if (!File.Exists(modelPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Model file not found at: {modelPath}");
            Console.WriteLine("Please download a Phi-3 Mini ONNX model to proceed.");
            Console.ResetColor();
            return;
        }

        // 1. Initialize the Inference Session
        // We use 'using' to ensure resources are disposed of correctly.
        using var session = new InferenceSession(modelPath);

        // 2. Prepare the Input
        // For this example, we will manually tokenize a simple prompt.
        // In a production app, you would use the Microsoft.ML.OnnxRuntime.Extensions NuGet package
        // to load 'tokenizer.json' and handle tokenization automatically.
        // "What is 2 + 2?" (Prompt token IDs for Phi-3 Mini - simplified for example)
        // Note: Real tokenization requires a tokenizer library. Here we simulate the input tensor.
        
        // We need to construct the 'input_ids' tensor. 
        // Shape: [batch_size, sequence_length]
        // For Phi-3, the input shape is usually [1, sequence_length].
        
        // Let's create a dummy input for demonstration. 
        // In a real scenario, you'd tokenize "What is 2 + 2?" into integers.
        // Example token IDs for "What is 2 + 2?" (approximate for Phi-3):
        // <s> (1), What (1867), is (318), 2 (17), + (337), 2 (17), ? (30)
        long[] inputIds = [1, 1867, 318, 17, 337, 17, 30]; 
        
        // Attention mask (usually all 1s for valid tokens)
        long[] attentionMask = [1, 1, 1, 1, 1, 1, 1];
        
        // Position IDs (usually 0 to sequence_length-1)
        long[] positionIds = [0, 1, 2, 3, 4, 5, 6];

        // Convert arrays to Tensors
        var inputIdsTensor = new DenseTensor<long>(inputIds, [1, inputIds.Length]);
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, [1, attentionMask.Length]);
        var positionIdsTensor = new DenseTensor<long>(positionIds, [1, positionIds.Length]);

        // 3. Create NamedOnnxValue inputs
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
            NamedOnnxValue.CreateFromTensor("position_ids", positionIdsTensor)
        };

        // 4. Run Inference
        Console.WriteLine("Running inference...");
        
        // We use 'Run' to execute the model. 
        // The output name "logits" is specific to the model architecture.
        using var results = session.Run(inputs);

        // 5. Process the Output
        // The model outputs 'logits' (raw scores for the next token).
        // We need to find the token with the highest score (Greedy Search).
        var logitsTensor = results.First().AsTensor<float>();
        
        // Shape is [batch_size, sequence_length, vocab_size]
        // We look at the last token position (the prediction for the next token).
        int vocabSize = logitsTensor.Dimensions[2];
        int lastTokenIndex = inputIds.Length - 1; // Index of the last input token
        
        // Extract logits for the last token
        float[] lastTokenLogits = new float[vocabSize];
        for (int i = 0; i < vocabSize; i++)
        {
            // Accessing tensor data: [batch=0, sequence_position=lastTokenIndex, vocab_index=i]
            lastTokenLogits[i] = logitsTensor[0, lastTokenIndex, i];
        }

        // Find the index of the maximum value (ArgMax)
        int predictedTokenId = Array.IndexOf(lastTokenLogits, lastTokenLogits.Max());

        // 6. Decode the Output (Simulated)
        // In a real app, you would feed this 'predictedTokenId' back into the model 
        // repeatedly (autoregressive generation) until you hit a stop token.
        // Here, we just print the single predicted token ID.
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nInput Prompt (Token IDs): {string.Join(", ", inputIds)}");
        Console.WriteLine($"Predicted Next Token ID: {predictedTokenId}");
        
        // Note: Without a tokenizer, we can't easily convert ID back to text here.
        // But typically, ID 1867 might be "What", 17 might be "2", etc.
        Console.WriteLine("Inference complete.");
        Console.ResetColor();
    }
}
