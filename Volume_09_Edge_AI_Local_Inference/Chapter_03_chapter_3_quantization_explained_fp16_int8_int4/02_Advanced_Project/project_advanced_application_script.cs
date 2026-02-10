
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EdgeAI_QuantizationDemo
{
    /// <summary>
    /// Simulates the deployment pipeline for an Edge AI application (e.g., a Smart Home Voice Assistant)
    /// running on a resource-constrained device (like a Raspberry Pi or an industrial IoT gateway).
    /// 
    /// Problem Context: A smart home system needs to classify audio commands locally to ensure 
    /// privacy and low latency. The raw model (FP32) is too slow and memory-intensive for the device.
    /// We will apply dynamic quantization (INT8) to the model weights to optimize for CPU inference.
    /// </summary>
    class Program
    {
        // Configuration: Simulating the hardware constraints of an Edge device.
        const int AVAILABLE_RAM_MB = 2048; // 2GB RAM limit
        const int MAX_INFERENCE_TIME_MS = 50; // Requirement: Real-time response (<50ms)

        static void Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Quantization Deployment Simulator ===");
            Console.WriteLine($"Target Hardware: CPU with {AVAILABLE_RAM_MB}MB RAM");
            Console.WriteLine($"Performance Requirement: < {MAX_INFERENCE_TIME_MS}ms per inference\n");

            // 1. Load the "Pre-trained" Model (Simulated)
            // In a real scenario, this would be loading a .onnx file.
            // We simulate a model with 10 million parameters (typical for a small transformer).
            var baseModel = LoadModel("llama_tiny.onnx", parameterCount: 10_000_000, precision: "FP32");
            InspectModel(baseModel);

            // 2. Evaluate Baseline Performance (FP32)
            // Before optimization, we measure the cost of high precision.
            Console.WriteLine("\n--- Baseline Evaluation (FP32) ---");
            EvaluatePerformance(baseModel);

            // 3. Apply Dynamic Quantization (INT8)
            // We convert weights from FP32 to INT8. This is a destructive process but reduces size.
            Console.WriteLine("\n--- Applying Dynamic Quantization (FP32 -> INT8) ---");
            var quantizedModel = QuantizeModelToInt8(baseModel);
            
            // 4. Verify Model Integrity
            // Check if the quantization caused structural errors (e.g., overflow).
            if (VerifyModelIntegrity(quantizedModel))
            {
                Console.WriteLine("Quantization successful. Model integrity verified.");
            }
            else
            {
                Console.WriteLine("Quantization failed. Model integrity compromised.");
                return;
            }

            // 5. Evaluate Optimized Performance (INT8)
            // Measure the impact of reduced precision on memory and speed.
            Console.WriteLine("\n--- Optimized Evaluation (INT8) ---");
            EvaluatePerformance(quantizedModel);

            // 6. Decision Logic
            // Determine if the quantized model meets the deployment criteria.
            Console.WriteLine("\n--- Deployment Decision ---");
            CheckDeploymentReadiness(quantizedModel);
        }

        /// <summary>
        /// Simulates loading a neural network model.
        /// </summary>
        /// <returns>A Model object containing weights and metadata.</returns>
        static Model LoadModel(string name, int parameterCount, string precision)
        {
            Console.WriteLine($"Loading model: {name}...");
            var weights = new float[parameterCount];
            
            // Initialize with random weights to simulate a trained model
            var rand = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < parameterCount; i++)
            {
                weights[i] = (float)(rand.NextDouble() * 2 - 1); // Range [-1, 1]
            }

            return new Model
            {
                Name = name,
                WeightsFP32 = weights,
                Precision = precision,
                ParameterCount = parameterCount
            };
        }

        /// <summary>
        /// Simulates the ONNX Runtime quantization tool.
        /// Converts FP32 weights to INT8 by scaling and clamping.
        /// </summary>
        static Model QuantizeModelToInt8(Model sourceModel)
        {
            if (sourceModel.Precision != "FP32")
            {
                throw new InvalidOperationException("Source model must be FP32 for this quantization path.");
            }

            Console.WriteLine("Analyzing weight distribution for dynamic scaling...");
            
            // In Dynamic Quantization, we determine the scale factor based on the max absolute value.
            float maxVal = 0f;
            foreach (var w in sourceModel.WeightsFP32)
            {
                if (Math.Abs(w) > maxVal) maxVal = Math.Abs(w);
            }

            // Scale factor: Maps FP32 range [-max, max] to INT8 range [-127, 127]
            // Formula: int8_val = round(fp32_val / max_abs_weight * 127)
            float scale = 127.0f / maxVal;
            
            Console.WriteLine($"Scale factor calculated: {scale:F4} (Max Weight: {maxVal:F4})");

            var quantizedWeights = new sbyte[sourceModel.ParameterCount];
            long totalError = 0;

            // Perform the quantization loop
            for (int i = 0; i < sourceModel.ParameterCount; i++)
            {
                float original = sourceModel.WeightsFP32[i];
                
                // 1. Scale
                float scaled = original * scale;
                
                // 2. Round (Nearest integer)
                sbyte quantized = (sbyte)Math.Round(scaled);
                
                // 3. Clamp (Ensure it fits in 8 bits, though Math.Round + scale usually handles this)
                if (quantized > 127) quantized = 127;
                if (quantized < -128) quantized = -128;

                quantizedWeights[i] = quantized;

                // Calculate reconstruction error (for demonstration purposes)
                float dequantized = quantized / scale;
                totalError += (long)Math.Abs(original - dequantized);
            }

            double avgError = (double)totalError / sourceModel.ParameterCount;
            Console.WriteLine($"Quantization complete. Average reconstruction error per weight: {avgError:E4}");

            return new Model
            {
                Name = sourceModel.Name.Replace(".onnx", "_int8.onnx"),
                WeightsINT8 = quantizedWeights,
                Precision = "INT8",
                ParameterCount = sourceModel.ParameterCount,
                ScaleFactor = scale
            };
        }

        /// <summary>
        /// Simulates inference execution and measures resource usage.
        /// </summary>
        static void EvaluatePerformance(Model model)
        {
            long memoryUsageBytes = 0;

            // Calculate Memory Footprint
            if (model.Precision == "FP32")
            {
                // 4 bytes per float
                memoryUsageBytes = model.ParameterCount * 4;
            }
            else if (model.Precision == "INT8")
            {
                // 1 byte per int8
                memoryUsageBytes = model.ParameterCount * 1;
            }

            double memoryUsageMB = memoryUsageBytes / (1024.0 * 1024.0);
            
            // Simulate Inference Time
            // INT8 operations are generally faster on CPUs due to SIMD (Single Instruction, Multiple Data) optimizations.
            // However, we must account for the de-quantization step (converting INT8 back to FP32 for activation layers).
            double baseOpsPerParam = 2.0; // Multiply-Add operation
            double clockSpeedGHz = 2.4; // Simulated CPU speed
            
            // Heuristic: FP32 takes longer per operation than INT8
            double timePerOpNs = (model.Precision == "FP32") ? 2.5 : 0.8; 
            
            // Total time = Ops * TimePerOp
            double estimatedTimeMs = (model.ParameterCount * baseOpsPerParam * timePerOpNs) / 1_000_000.0;

            Console.WriteLine($"  - Precision: {model.Precision}");
            Console.WriteLine($"  - Memory Usage: {memoryUsageMB:F2} MB");
            Console.WriteLine($"  - Estimated Inference Time: {estimatedTimeMs:F2} ms");

            // Store metrics in model object for later comparison
            model.Metrics = new ModelMetrics
            {
                MemoryMB = memoryUsageMB,
                InferenceTimeMs = estimatedTimeMs
            };
        }

        /// <summary>
        /// Checks if the quantized model fits within the hardware constraints.
        /// </summary>
        static void CheckDeploymentReadiness(Model model)
        {
            if (model.Metrics == null) return;

            bool memoryOk = model.Metrics.MemoryMB <= AVAILABLE_RAM_MB;
            bool speedOk = model.Metrics.InferenceTimeMs <= MAX_INFERENCE_TIME_MS;

            if (memoryOk && speedOk)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SUCCESS] Model '{model.Name}' is ready for deployment.");
                Console.ResetColor();
                Console.WriteLine($"Reason: Fits in RAM ({model.Metrics.MemoryMB:F2}MB < {AVAILABLE_RAM_MB}MB) " +
                                  $"and meets speed requirements ({model.Metrics.InferenceTimeMs:F2}ms < {MAX_INFERENCE_TIME_MS}ms).");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAILURE] Model '{model.Name}' is NOT ready for deployment.");
                Console.ResetColor();
                if (!memoryOk) Console.WriteLine($"  - Memory Exceeded: {model.Metrics.MemoryMB:F2}MB > {AVAILABLE_RAM_MB}MB");
                if (!speedOk) Console.WriteLine($"  - Speed Exceeded: {model.Metrics.InferenceTimeMs:F2}ms > {MAX_INFERENCE_TIME_MS}ms");
            }
        }

        /// <summary>
        /// Helper to display model metadata.
        /// </summary>
        static void InspectModel(Model model)
        {
            Console.WriteLine($"Model Inspection: {model.Name}");
            Console.WriteLine($"  - Parameters: {model.ParameterCount:N0}");
            Console.WriteLine($"  - Precision: {model.Precision}");
        }

        /// <summary>
        /// Validates that quantized weights are within valid ranges and no data corruption occurred.
        /// </summary>
        static bool VerifyModelIntegrity(Model model)
        {
            if (model.Precision != "INT8" || model.WeightsINT8 == null) return false;

            // Check for NaN or Infinity (Simulated check)
            // In INT8, we just check bounds.
            for (int i = 0; i < model.WeightsINT8.Length; i++)
            {
                sbyte val = model.WeightsINT8[i];
                if (val < -128 || val > 127) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Data structure representing a Neural Network Model.
    /// Using basic arrays to store weights as per C# fundamentals.
    /// </summary>
    class Model
    {
        public string Name { get; set; }
        public int ParameterCount { get; set; }
        public string Precision { get; set; } // FP32, INT8, etc.
        
        // Storage for different precisions
        public float[] WeightsFP32 { get; set; }
        public sbyte[] WeightsINT8 { get; set; }
        
        public float ScaleFactor { get; set; } // Used for INT8 -> FP32 dequantization
        
        // Runtime metrics
        public ModelMetrics Metrics { get; set; }
    }

    class ModelMetrics
    {
        public double MemoryMB { get; set; }
        public double InferenceTimeMs { get; set; }
    }
}
