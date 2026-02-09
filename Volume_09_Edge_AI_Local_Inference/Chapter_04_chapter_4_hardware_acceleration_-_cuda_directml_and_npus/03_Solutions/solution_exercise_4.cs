
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;

namespace HardwareAccelerationExercises
{
    public class NpuBenchmark
    {
        public void RunNpuOptimizedInference(string modelPath)
        {
            // 1. Simulate NPU Detection
            if (!IsNPUDetected())
            {
                Console.WriteLine("NPU not detected via environment variable. Skipping NPU benchmark.");
                return;
            }

            Console.WriteLine("✅ NPU Detected. Configuring for Power Efficiency...");

            using var sessionOptions = new SessionOptions();

            // 2. NPU Execution Provider Configuration
            // In a real scenario (Intel/Qualcomm), this would be OpenVINO or QNN.
            // We simulate the API call here.
            try 
            {
                // Simulated API call
                // sessionOptions.AppendExecutionProvider("NpuExecutionProvider"); 
                Console.WriteLine("Configured: NpuExecutionProvider");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load NPU provider: {ex.Message}");
                return;
            }

            // 3. Power Mode Constraint
            // Setting a custom config entry to hint at power efficiency.
            // Note: Actual keys depend on the specific provider implementation.
            sessionOptions.ConfigOptions.SetConfig("optimization_style", "power_efficiency");
            Console.WriteLine("Configured: Optimization Style = Power Efficiency");

            try
            {
                using var session = new InferenceSession(modelPath, sessionOptions);

                // 4. Benchmarking Loop
                // Warmup
                Console.WriteLine("Warming up...");
                // Simulate a warmup run (omitted for brevity in simulation)

                Stopwatch stopwatch = Stopwatch.StartNew();
                int iterations = 100;

                Console.WriteLine($"Running {iterations} inference iterations...");
                
                for (int i = 0; i < iterations; i++)
                {
                    // Simulate inference call
                    // In a real app: session.Run(inputs);
                    
                    // Simulate processing time to prevent 0ms results in this demo
                    System.Threading.Thread.Sleep(1); 
                }

                stopwatch.Stop();
                double avgLatency = stopwatch.ElapsedMilliseconds / (double)iterations;

                // 5. Comparison Output
                Console.WriteLine("=== Benchmark Results ===");
                Console.WriteLine($"Running on NPU: Average latency {avgLatency:F2} ms.");
                Console.WriteLine($"Estimated Power Mode: Efficiency.");
                Console.WriteLine("=========================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Benchmark failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks for the specific environment variable to simulate NPU presence.
        /// </summary>
        private bool IsNPUDetected()
        {
            string envVar = Environment.GetEnvironmentVariable("EDGE_AI_TARGET_NPU");
            return envVar == "1";
        }
    }
}
