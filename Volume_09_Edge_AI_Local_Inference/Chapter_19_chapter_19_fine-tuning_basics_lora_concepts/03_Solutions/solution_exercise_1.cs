
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoraMergeExercise
{
    // Simulates the structure of a LoRA configuration (usually found in adapter_config.json)
    public class LoraConfig
    {
        public int Rank { get; set; } = 64;
        public float Alpha { get; set; } = 16.0f;
        public string TargetModule { get; set; } = "q_proj";
    }

    public class LoraWeightLoader
    {
        private readonly string _baseModelPath;
        private readonly string _loraConfigPath;

        public LoraWeightLoader(string baseModelPath, string loraConfigPath)
        {
            _baseModelPath = baseModelPath;
            _loraConfigPath = loraConfigPath;
        }

        // Simulates loading Safetensors. In a real app, use a library like HuggingFace.Safetensors
        public Dictionary<string, float[]> LoadMockLoraWeights()
        {
            Console.WriteLine("Simulating load of LoRA safetensors...");
            
            // Mocking weights for 'model.layers.0.self_attn.q_proj'
            // A shape: [64, 4096] (Rank r, In_features)
            // B shape: [4096, 64] (Out_features, Rank r)
            
            var weights = new Dictionary<string, float[]>();
            
            // Initialize with dummy data for demonstration
            weights["lora_A.weight"] = new float[64 * 4096]; 
            weights["lora_B.weight"] = new float[4096 * 64];
            
            // Fill with some values to verify math later
            Array.Fill(weights["lora_A.weight"], 0.1f);
            Array.Fill(weights["lora_B.weight"], 0.1f);

            return weights;
        }

        public void MergeAndSave(string outputPath)
        {
            // 1. Load Base Model Initializers
            // Note: In a real scenario, we use OnnxModel or GraphSurgeon to manipulate the graph.
            // Here we simulate extracting the base weight for 'q_proj'.
            float[] baseWeights = new float[4096 * 4096]; // Assuming 4096 out/in features for Phi-3
            Array.Fill(baseWeights, 1.0f); // Mock base weight = 1.0

            // 2. Load LoRA Weights
            var loraWeights = LoadMockLoraWeights();
            var loraA = loraWeights["lora_A.weight"];
            var loraB = loraWeights["lora_B.weight"];

            // 3. Validate Shapes
            int r = 64;
            int inFeatures = 4096;
            int outFeatures = 4096;

            if (loraA.Length != r * inFeatures || loraB.Length != outFeatures * r)
            {
                throw new InvalidOperationException("LoRA weight dimensions do not match expected rank and feature sizes.");
            }

            // 4. Merge Logic: W_merged = W_base + (B * A) * (alpha / r)
            float scale = 16.0f / 64.0f; // Alpha / Rank
            
            // Calculate B * A (Matrix Multiplication)
            // Result shape: [outFeatures, inFeatures]
            float[] loraUpdate = new float[outFeatures * inFeatures];

            // Manual Matrix Multiplication (Optimization note: use BLAS or Tensor in production)
            for (int i = 0; i < outFeatures; i++)
            {
                for (int j = 0; j < inFeatures; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < r; k++)
                    {
                        // B[i, k] * A[k, j]
                        sum += loraB[i * r + k] * loraA[k * inFeatures + j];
                    }
                    loraUpdate[i * inFeatures + j] = sum * scale;
                }
            }

            // Add to base weights
            float[] mergedWeights = new float[baseWeights.Length];
            for (int i = 0; i < baseWeights.Length; i++)
            {
                mergedWeights[i] = baseWeights[i] + loraUpdate[i];
            }

            Console.WriteLine($"Weights merged. Calculated update for {mergedWeights.Length} parameters.");

            // 5. Serialization
            // In a real app, we would load the ONNX model using OnnxModel, 
            // replace the initializer 'q_proj.weight' with mergedWeights, and save.
            // For this exercise, we simulate the save operation.
            File.WriteAllText(outputPath, $"Simulated ONNX file with merged weights of size {mergedWeights.Length}");
            Console.WriteLine($"Saved merged model to: {outputPath}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var loader = new LoraWeightLoader("phi3-base.onnx", "adapter_config.json");
                loader.MergeAndSave("phi3-lora-merged.onnx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
