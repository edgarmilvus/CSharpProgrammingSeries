
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

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoraOnnxInference
{
    // Real-world context: Imagine you have a customer support chatbot 
    // that needs to understand specific product terminology (e.g., "Quantum Database v3").
    // Instead of retraining the entire 7B parameter model (costly and slow), 
    // we apply a LoRA adapter trained on a small dataset of product-specific Q&A.
    // This code demonstrates how to load a base ONNX model and a LoRA adapter 
    // to perform inference on a local edge device using C#.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: LoRA Adapter Inference with C# & ONNX ===");
            
            // 1. Setup the MLContext (the entry point for ML.NET operations)
            var mlContext = new MLContext(seed: 0);

            // 2. Define paths (In a real scenario, these would be downloaded/prepared)
            // Base Model: A standard Phi-2 or Llama 2 model exported to ONNX
            // Adapter: The LoRA weights (matrices A and B) exported to ONNX
            string baseModelPath = "phi-2-base.onnx";
            string loraAdapterPath = "phi-2-lora-adapter.onnx";

            // 3. Mock Data Loading (Simulating tokenized input for the example)
            // Real-world: Tokenize text -> convert to numeric tensors
            var inputData = new List<ModelInput>
            {
                new ModelInput { 
                    InputIds = new long[] { 1, 15496, 616, 4707, 13 }, // "Hello world example"
                    AttentionMask = new long[] { 1, 1, 1, 1, 1 } 
                }
            };
            var dataView = mlContext.Data.LoadFromEnumerable(inputData);

            // 4. Define the ONNX Transformer Pipeline
            // Note: ML.NET's OnnxTransformer primarily loads a single model file. 
            // For LoRA, we typically merge the adapter weights into the base model 
            // offline (using Python tools) or implement a custom operator. 
            // For this 'Hello World', we demonstrate the ONNX loading pattern 
            // assuming a merged model (base + lora) for simplicity in C#.
            
            // However, to strictly follow the LoRA concept without merging:
            // We would need to manually manipulate tensors (Add, MatMul) which 
            // ML.NET doesn't support out-of-the-box without custom C++ operators.
            // Below demonstrates the standard ONNX loading pipeline used in Edge AI.

            Console.WriteLine("Loading ONNX model pipeline...");

            var onnxModelPath = baseModelPath; // In practice: merge(base, lora) -> output.onnx
            
            // Define input/output column names matching the ONNX model signature
            var inputColumns = new[] { "input_ids", "attention_mask" };
            var outputColumns = new[] { "logits" };

            // Create the ONNX transformer
            var onnxTransformer = mlContext.Transforms.ApplyOnnxModel(
                modelFile: onnxModelPath,
                outputColumnNames: outputColumns,
                inputColumnNames: inputColumns
            );

            // 5. Fit the model (Load the ONNX graph into memory)
            var model = onnxTransformer.Fit(dataView);

            // 6. Create a prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);

            // 7. Run Inference
            var sampleInput = inputData.First();
            var prediction = predictionEngine.Predict(sampleInput);

            // 8. Process Output (Logits)
            // Logits are raw scores for the next token in the vocabulary.
            Console.WriteLine($"\nInference Complete. Output Shape: {prediction.Logits.GetLength(0)}x{prediction.Logits.GetLength(1)}");
            
            // Find the token with the highest logit (greedy decoding)
            int vocabSize = prediction.Logits.GetLength(1);
            int predictedTokenId = 0;
            float maxLogit = float.MinValue;

            // We look at the last token position (next token prediction)
            int lastTokenPosition = prediction.Logits.GetLength(0) - 1;
            
            for (int i = 0; i < vocabSize; i++)
            {
                float currentLogit = prediction.Logits[lastTokenPosition, i];
                if (currentLogit > maxLogit)
                {
                    maxLogit = currentLogit;
                    predictedTokenId = i;
                }
            }

            Console.WriteLine($"Predicted Next Token ID: {predictedTokenId}");
            Console.WriteLine($"(In a real app, map this ID back to text using a tokenizer)");
        }
    }

    // Data Schema Definitions
    public class ModelInput
    {
        // Variable length sequences are handled by padding in real scenarios
        // For this example, we define a fixed size for the tensor shape
        [VectorType(5)] 
        public long[] InputIds { get; set; }

        [VectorType(5)]
        public long[] AttentionMask { get; set; }
    }

    public class ModelOutput
    {
        // Output shape typically: [BatchSize, SequenceLength, VocabSize]
        // For Phi-2, VocabSize is ~51200
        [ColumnName("logits")]
        [VectorType(1, 5, 51200)] // Batch=1, SeqLen=5, Vocab=51200
        public float[,] Logits { get; set; }
    }
}
