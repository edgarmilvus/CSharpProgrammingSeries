
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace EdgeAIHardwareAccelerator
{
    /// <summary>
    /// A real-world application for an Edge AI device (e.g., an industrial IoT gateway)
    /// that performs real-time anomaly detection on sensor data streams.
    /// 
    /// Problem: The device must process sensor data continuously. Depending on the 
    /// available hardware (Cloud GPU, Local NPU, or Standard CPU), it must dynamically
    /// select the fastest execution provider to maintain real-time performance.
    /// </summary>
    class Program
    {
        // Configuration for the AI Model (Simulated ONNX model path)
        const string ModelPath = "anomaly_detection_model.onnx";

        static void Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Hardware Accelerated Inference Engine ===");
            Console.WriteLine($"Target Platform: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Architecture: {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine();

            // 1. Detect Hardware Capabilities
            var availableProviders = DetectHardwareCapabilities();

            // 2. Select the Optimal Execution Provider
            string optimalProvider = SelectOptimalProvider(availableProviders);

            Console.WriteLine($"[System] Selected Execution Provider: {optimalProvider}");
            Console.WriteLine("--------------------------------------------------");

            // 3. Initialize Inference Session with the selected provider
            // In a real scenario, we would pass SessionOptions configured with the specific provider.
            // For this simulation, we will mock the session initialization based on the provider.
            using (var inferenceSession = InitializeInferenceSession(optimalProvider))
            {
                // 4. Simulate Real-Time Sensor Data Stream
                Console.WriteLine("[System] Starting sensor data stream...");
                Console.WriteLine("[System] Press Ctrl+C to stop.\n");

                // Simulate 5 batches of sensor data
                for (int i = 0; i < 5; i++)
                {
                    // Generate mock sensor data (e.g., vibration, temperature, pressure)
                    float[] sensorData = GenerateSensorDataBatch(i);

                    // 5. Run Inference
                    var result = RunInference(inferenceSession, sensorData);

                    // 6. Process Results (Anomaly Detection Logic)
                    ProcessResults(result, i);
                    
                    // Simulate real-time delay
                    System.Threading.Thread.Sleep(1000);
                }
            }

            Console.WriteLine("\n[System] Session terminated.");
        }

        /// <summary>
        /// Detects available hardware and installed ONNX Runtime Execution Providers (EPs).
        /// </summary>
        /// <returns>A list of supported EP names.</returns>
        static List<string> DetectHardwareCapabilities()
        {
            var providers = new List<string>();

            Console.WriteLine("[Diagnostics] Scanning hardware...");

            // Check for NVIDIA CUDA (GPU Acceleration)
            // In a real app, we might check CUDA DLLs or use a library like ManagedCuda.
            // Here we simulate detection based on environment variables or OS capabilities.
            bool hasCuda = CheckForCuda();
            if (hasCuda)
            {
                providers.Add("CUDAExecutionProvider");
                Console.WriteLine("  [✓] NVIDIA CUDA Detected (GPU Acceleration Ready)");
            }

            // Check for DirectML (Windows GPU - DirectX 12 compatible)
            // DirectML works on AMD, Intel, and NVIDIA GPUs on Windows 10/11.
            bool hasDirectML = CheckForDirectML();
            if (hasDirectML)
            {
                providers.Add("DmlExecutionProvider");
                Console.WriteLine("  [✓] DirectML Detected (Windows GPU Ready)");
            }

            // Check for NPU (Neural Processing Unit)
            // Emerging standard for edge devices (e.g., Intel Movidius, Qualcomm Hexagon, Apple Neural Engine).
            bool hasNPU = CheckForNPU();
            if (hasNPU)
            {
                providers.Add("NpuExecutionProvider"); // Hypothetical EP name for NPU
                Console.WriteLine("  [✓] NPU Detected (Edge Acceleration Ready)");
            }

            // CPU is always available as a fallback
            providers.Add("CPUExecutionProvider");
            Console.WriteLine("  [✓] CPU Execution Provider (Fallback)");

            return providers;
        }

        /// <summary>
        /// Logic to select the best provider based on priority: NPU > CUDA > DirectML > CPU.
        /// </summary>
        static string SelectOptimalProvider(List<string> providers)
        {
            // Priority Order for Edge AI:
            // 1. NPU (Lowest power consumption, dedicated AI hardware)
            // 2. CUDA (Highest raw performance for NVIDIA GPUs)
            // 3. DirectML (Broad compatibility for Windows GPUs)
            // 4. CPU (Software fallback)

            if (providers.Contains("NpuExecutionProvider")) return "NpuExecutionProvider";
            if (providers.Contains("CUDAExecutionProvider")) return "CUDAExecutionProvider";
            if (providers.Contains("DmlExecutionProvider")) return "DmlExecutionProvider";
            
            return "CPUExecutionProvider";
        }

        /// <summary>
        /// Initializes the ONNX Runtime InferenceSession.
        /// In a production app, we would configure SessionOptions here.
        /// </summary>
        static InferenceSession InitializeInferenceSession(string provider)
        {
            try
            {
                // NOTE: Real ONNX Runtime usage requires SessionOptions configuration.
                // Example: 
                // var options = new SessionOptions();
                // if (provider == "CUDAExecutionProvider") options.AppendExecutionProvider_CUDA();
                // if (provider == "DmlExecutionProvider") options.AppendExecutionProvider_Dml();
                
                // Since we don't have a physical .onnx file in this text-based environment,
                // we will simulate the session object.
                Console.WriteLine($"[System] Initializing ONNX Runtime with {provider}...");
                
                // Simulate loading time based on hardware speed
                int loadTime = provider == "CPUExecutionProvider" ? 500 : 200;
                System.Threading.Thread.Sleep(loadTime);
                
                Console.WriteLine($"[System] Model loaded successfully.");
                
                // Return a mock session (In real code, this is: new InferenceSession(modelPath, options);)
                return new MockInferenceSession(provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to initialize session: {ex.Message}");
                // Fallback to CPU if specific provider fails
                Console.WriteLine("[System] Falling back to CPU...");
                return new MockInferenceSession("CPUExecutionProvider");
            }
        }

        /// <summary>
        /// Simulates the inference process.
        /// Prepares input tensor, runs the model, and retrieves output.
        /// </summary>
        static float[] RunInference(InferenceSession session, float[] inputData)
        {
            // 1. Prepare Input Tensor
            // In ONNX Runtime, inputs are typically DenseTensors.
            // For a batch of 1 with 10 features:
            var inputTensor = new DenseTensor<float>(inputData, new[] { 1, 10 });

            // 2. Create Input Container (ReadOnlyMemoryContainer)
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            // 3. Run Inference
            // This is where the hardware acceleration happens (CUDA/DML/NPU kernels execute here).
            // We simulate the execution time difference.
            int inferenceTime = session.Provider switch
            {
                "NpuExecutionProvider" => 5,   // Fastest
                "CUDAExecutionProvider" => 10, // Fast
                "DmlExecutionProvider" => 20,  // Medium
                _ => 100                       // Slow (CPU)
            };
            
            // Simulate processing delay
            System.Threading.Thread.Sleep(inferenceTime);

            // 4. Mock Output Generation
            // In a real scenario: using var results = session.Run(inputs);
            // We generate a mock anomaly score (0.0 to 1.0)
            float anomalyScore = new Random().Next(0, 100) / 100.0f;
            
            return new float[] { anomalyScore };
        }

        /// <summary>
        /// Analyzes the inference result and triggers alerts if necessary.
        /// </summary>
        static void ProcessResults(float[] results, int batchId)
        {
            float score = results[0];
            string status = score > 0.75 ? "ANOMALY DETECTED" : "Normal";
            string color = score > 0.75 ? "Red" : "Green";

            Console.WriteLine($"[Batch {batchId}] Score: {score:F4} | Status: {status}");
            
            if (score > 0.75)
            {
                // In a real app, this would trigger an actuator or send a network alert
                Console.WriteLine($"  >>> ALERT: High vibration detected! Triggering safety protocol.");
            }
        }

        // ---------------------------------------------------------
        // HELPER METHODS (Simulation Logic)
        // ---------------------------------------------------------

        static bool CheckForCuda()
        {
            // Simulate detection logic. 
            // Real check: look for nvcuda.dll or use CUDA API calls.
            return Environment.GetEnvironmentVariable("CUDA_PATH") != null;
        }

        static bool CheckForDirectML()
        {
            // Real check: Verify Windows 10+ and DirectX 12 support.
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        static bool CheckForNPU()
        {
            // Simulate finding an NPU on an ARM-based edge device or Intel Movidius.
            // Real check: Check Device IDs in Device Manager or specific vendor SDKs.
            string arch = RuntimeInformation.ProcessArchitecture.ToString();
            return arch.Contains("Arm") || arch.Contains("ARM"); // Common for NPUs
        }

        static float[] GenerateSensorDataBatch(int seed)
        {
            // Generate 10 random sensor readings (e.g., 3-axis accel, temp, etc.)
            var rng = new Random(seed);
            var data = new float[10];
            for (int i = 0; i < 10; i++)
            {
                data[i] = (float)rng.NextDouble();
            }
            return data;
        }
    }

    /// <summary>
    /// Mock class to simulate ONNX Runtime InferenceSession behavior 
    /// since we cannot load a real file in this text environment.
    /// </summary>
    public class MockInferenceSession : IDisposable
    {
        public string Provider { get; }

        public MockInferenceSession(string provider)
        {
            Provider = provider;
        }

        public void Dispose()
        {
            // Cleanup logic
        }
    }
}
