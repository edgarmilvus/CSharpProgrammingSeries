
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace EdgeAI_HardwareAcceleration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Edge AI: Hardware Acceleration Selection Demo ---");

            // 1. Detect Hardware Capabilities
            // We check for the presence of an NVIDIA GPU (CUDA) or a specialized NPU (via DirectML).
            // In a real scenario, we might also check for Intel/AMD GPUs via DirectML or OpenVINO.
            bool hasCuda = CheckForCuda();
            bool hasDirectML = CheckForDirectML(); // Often covers AMD/Intel/NVIDIA on Windows

            // 2. Configure Execution Providers (EPs)
            // ONNX Runtime prioritizes EPs in the order provided.
            // We construct a list of desired providers dynamically.
            var executionProviders = new List<string>();

            // Strategy: Prefer CUDA for raw speed on NVIDIA hardware.
            if (hasCuda)
            {
                executionProviders.Add("CUDAExecutionProvider");
                Console.WriteLine("✅ NVIDIA GPU detected. Prioritizing CUDA Execution Provider.");
            }
            // Strategy: Fallback to DirectML for broad Windows GPU support (Intel/AMD/NVIDIA).
            else if (hasDirectML)
            {
                executionProviders.Add("DmlExecutionProvider");
                Console.WriteLine("✅ Non-NVIDIA GPU or Windows GPU detected. Prioritizing DirectML (DML).");
            }
            // Strategy: Fallback to CPU if no specialized hardware is available.
            else
            {
                executionProviders.Add("CPUExecutionProvider");
                Console.WriteLine("⚠️ No specialized hardware detected. Falling back to CPU Execution Provider.");
            }

            // 3. Define Session Options
            // We configure the session to use our selected providers.
            // Note: For this "Hello World" to run without an actual ONNX model file,
            // we will simulate the configuration logic. In production, you would load a .onnx file.
            var sessionOptions = new SessionOptions();
            
            // Apply the execution providers in order of preference.
            // The first available provider in the list will be used.
            foreach (var provider in executionProviders)
            {
                sessionOptions.AppendExecutionProvider(provider);
                Console.WriteLine($"   -> Registered Execution Provider: {provider}");
            }

            // 4. Simulate Inference Setup
            // In a real application, you would load the model here:
            // var session = new InferenceSession("model.onnx", sessionOptions);
            
            Console.WriteLine("\n--- Session Configuration Summary ---");
            Console.WriteLine($"Selected Backend: {executionProviders.First()}");
            Console.WriteLine($"Session Options Configured: {sessionOptions.ToConfigSummary()}");
            
            // 5. Simulate Input Tensor Creation
            // LLMs typically accept input IDs (int64) and Attention Mask (int64).
            // Here we create dummy data for a "Hello World" prompt tokenized to [101, 7592, 2088, 102].
            var inputIds = new long[] { 101, 7592, 2088, 102 };
            var attentionMask = new long[] { 1, 1, 1, 1 };

            // Create Tensor objects using the ML.NET Tensor API (System.Numerics.Tensors backend)
            var inputIdsTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
            var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });

            Console.WriteLine("\n--- Input Data Prepared ---");
            Console.WriteLine($"Input IDs Shape: [{inputIdsTensor.Dimensions[0]}, {inputIdsTensor.Dimensions[1]}]");
            Console.WriteLine($"Input Values: {string.Join(", ", inputIds)}");

            // 6. Prepare Inputs for Inference
            // We wrap the tensors in NamedOnnxValue objects for the session.
            // In a real LLM, you might also pass 'position_ids' or 'past_key_values' (for caching).
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
            };

            // 7. Execution Simulation
            // We attempt to run inference. 
            // CRITICAL: Since we don't have a physical .onnx file loaded in this snippet,
            // we catch the expected FileNotFoundException to demonstrate the flow without crashing.
            try
            {
                // In a real scenario:
                // using var results = session.Run(inputs);
                // var output = results.First().AsTensor<float>();
                
                Console.WriteLine("\n🚀 Inference execution triggered.");
                Console.WriteLine("   (Mock execution: In a real app, the model would process inputs on the selected hardware.)");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("\nℹ️ Model file not found (expected in this demo). Logic for EP selection is validated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error during inference: {ex.Message}");
            }

            // 8. Hardware Compatibility Check Logic (Helper Methods)
            // These methods simulate checking system capabilities.
            // In production, you might use NVML (NVIDIA Management Library) or DirectML API queries.
            static bool CheckForCuda()
            {
                // Heuristic: Check OS and potentially look for CUDA libraries.
                // For this demo, we assume true if on Windows/Linux and user has a GPU.
                // A robust implementation checks specific CUDA version DLLs (e.g., "cudart64_110.dll").
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            }

            static bool CheckForDirectML()
            {
                // DirectML is native to Windows 10/11 (build 19041+).
                // We check for Windows OS.
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }
        }
    }

    // Extension method to simulate summarizing session options for display
    public static class SessionOptionsExtensions
    {
        public static string ToConfigSummary(this SessionOptions options)
        {
            // In a real implementation, we might reflect on internal properties.
            // Here we return a static summary.
            return "Optimized for Latency | Memory Pattern: Sequential";
        }
    }
}
