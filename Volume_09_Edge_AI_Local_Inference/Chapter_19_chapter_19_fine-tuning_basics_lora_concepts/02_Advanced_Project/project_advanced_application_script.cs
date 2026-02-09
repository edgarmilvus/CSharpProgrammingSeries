
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
using System.IO;
using System.Text;

namespace EdgeAI_LoRA_Inference
{
    class Program
    {
        // Real-world context: A smart home hub needs to detect specific commands
        // (e.g., "dim lights", "play jazz") using a small, fine-tuned language model.
        // We simulate loading a base ONNX model and a LoRA adapter to adjust weights
        // for better task-specific performance without retraining the entire model.
        static void Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: LoRA Adapter Integration Demo ===\n");

            // 1. Initialize the Base Model (Simulated ONNX Weights)
            // In a real scenario, these floats would be loaded from a .onnx file.
            // We represent the model as a 2D array (Matrix) for simplicity.
            // Dimensions: [Hidden Size (4) x Hidden Size (4)]
            float[,] baseWeights = InitializeBaseWeights();
            Console.WriteLine("Base Model Weights Loaded.");
            PrintMatrix("Base Weights", baseWeights);

            // 2. Initialize LoRA Adapters (Simulated)
            // LoRA decomposes weight updates (DeltaW) into two low-rank matrices: A and B.
            // Rank (r) is typically small (e.g., 4, 8, 16).
            // Dimensions: A [4 x 2], B [2 x 4]
            int rank = 2;
            float[,] loraA = InitializeLoRA_A(rank);
            float[,] loraB = InitializeLoRA_B(rank);
            Console.WriteLine($"\nLoRA Adapters Initialized (Rank: {rank}).");
            PrintMatrix("LoRA A (Low Rank)", loraA);
            PrintMatrix("LoRA B (Low Rank)", loraB);

            // 3. Merge LoRA Weights into Base Model
            // This is the critical step for local inference efficiency.
            // Instead of running the adapter separately (which adds latency), we mathematically
            // merge the update (B * A) into the base weights.
            // Formula: W_merged = W_base + (B * A) * scaling_factor
            float scalingFactor = 1.0f; // Alpha / Rank
            float[,] mergedWeights = MergeWeights(baseWeights, loraA, loraB, scalingFactor);
            Console.WriteLine("\nWeights Merged successfully.");
            PrintMatrix("Merged Weights", mergedWeights);

            // 4. Simulate Inference
            // We run a forward pass with a dummy input vector.
            // Input: [1.0, 0.5, -0.5, 1.0]
            float[] inputVector = new float[] { 1.0f, 0.5f, -0.5f, 1.0f };
            Console.WriteLine("\nRunning Inference...");
            Console.WriteLine($"Input Vector: [{string.Join(", ", inputVector)}]");

            // In a real ONNX runtime, this would be Matrix * Vector multiplication + Activation.
            // We simulate the output of the fine-tuned layer.
            float[] output = MatrixMultiply(mergedWeights, inputVector);
            
            // Softmax simulation (simplified for readability)
            float[] probabilities = Softmax(output);
            
            Console.WriteLine("\nModel Output (Probabilities):");
            Console.WriteLine($"  Command 'Dim Lights': {probabilities[0]:P2}");
            Console.WriteLine($"  Command 'Play Jazz':  {probabilities[1]:P2}");
            Console.WriteLine($"  Command 'Unknown':    {probabilities[2]:P2}");
            
            // 5. Interpret Result
            int maxIndex = GetMaxIndex(probabilities);
            string[] commands = { "Dim Lights", "Play Jazz", "Unknown" };
            Console.WriteLine($"\nDetected Command: {commands[maxIndex]}");
        }

        // --- Helper Methods ---

        // Simulates loading pre-trained weights (e.g., from a Phi-3 or Llama ONNX file).
        // In a real app, this would be replaced by OnnxRuntime session.Run().
        static float[,] InitializeBaseWeights()
        {
            // 4x4 Matrix
            return new float[,]
            {
                { 0.1f, 0.2f, 0.3f, 0.4f },
                { 0.5f, 0.6f, 0.7f, 0.8f },
                { 0.9f, 1.0f, 1.1f, 1.2f },
                { 1.3f, 1.4f, 1.5f, 1.6f }
            };
        }

        // Simulates the LoRA Adapter A (Projection Down).
        // Randomly initialized during training, then saved.
        static float[,] InitializeLoRA_A(int rank)
        {
            // 4x2 Matrix
            return new float[,]
            {
                { 0.05f, 0.02f },
                { 0.01f, -0.03f },
                { 0.04f, 0.08f },
                { -0.02f, 0.01f }
            };
        }

        // Simulates the LoRA Adapter B (Projection Up).
        // Randomly initialized, then trained to minimize loss.
        static float[,] InitializeLoRA_B(int rank)
        {
            // 2x4 Matrix
            return new float[,]
            {
                { 0.1f, -0.1f, 0.2f, 0.0f },
                { 0.05f, 0.15f, -0.1f, 0.05f }
            };
        }

        // Performs Matrix Multiplication: (Base + (B * A)) * Scaling
        // This calculates the delta weights to add to the base model.
        static float[,] MergeWeights(float[,] baseWeights, float[,] loraA, float[,] loraB, float scale)
        {
            int rows = baseWeights.GetLength(0);
            int cols = baseWeights.GetLength(1);
            int rank = loraA.GetLength(1);

            float[,] result = new float[rows, cols];

            // Step 1: Calculate B * A (Matrix Multiplication)
            // Dimensions: [4x2] * [2x4] = [4x4]
            float[,] deltaW = new float[rows, cols];
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < rank; k++)
                    {
                        sum += loraB[i, k] * loraA[k, j];
                    }
                    deltaW[i, j] = sum * scale;
                }
            }

            // Step 2: Add Delta to Base
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = baseWeights[i, j] + deltaW[i, j];
                }
            }

            return result;
        }

        // Simulates the Forward Pass: Matrix * Vector
        static float[] MatrixMultiply(float[,] matrix, float[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            float[] result = new float[rows];

            for (int i = 0; i < rows; i++)
            {
                float sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                result[i] = sum;
            }
            return result;
        }

        // Converts raw logits to probabilities (Sum = 1.0)
        static float[] Softmax(float[] values)
        {
            float[] expValues = new float[values.Length];
            float sumExp = 0;
            
            // Find max value for numerical stability
            float maxVal = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > maxVal) maxVal = values[i];
            }

            for (int i = 0; i < values.Length; i++)
            {
                expValues[i] = (float)Math.Exp(values[i] - maxVal);
                sumExp += expValues[i];
            }

            for (int i = 0; i < values.Length; i++)
            {
                expValues[i] /= sumExp;
            }
            return expValues;
        }

        // Finds the index of the highest probability
        static int GetMaxIndex(float[] values)
        {
            int maxIndex = 0;
            float maxVal = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > maxVal)
                {
                    maxVal = values[i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        // Utility to print matrices for visualization
        static void PrintMatrix(string name, float[,] matrix)
        {
            Console.WriteLine($"{name}:");
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                Console.Write("  [ ");
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j]:F2} ");
                }
                Console.WriteLine("]");
            }
        }
    }
}
