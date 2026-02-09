
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace LocalLLM.Hardware
{
    // 1. System Resource Monitor
    public class HardwareMonitor
    {
        public long GetAvailableVRamBytes()
        {
            // Note: .NET does not have a direct cross-platform API for VRAM.
            // This is a mock implementation. 
            // In a real Windows environment, you might use PerformanceCounter for GPU memory.
            // For Linux, you might parse 'nvidia-smi' output.
            
            // Simulation: Returning a value for demonstration purposes
            // In production, replace with actual GPU memory query.
            return 6L * 1024 * 1024 * 1024; // Simulating 6GB available
        }
    }

    // 2. Model Registry
    public class ModelConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long EstimatedMemoryBytes { get; set; }
        public bool IsGpuCompatible { get; set; }
    }

    public static class ModelRegistry
    {
        public static List<ModelConfig> AvailableModels => new List<ModelConfig>
        {
            new ModelConfig { Name = "Llama-7B-FP16", Path = @"models\llama-7b-fp16.onnx", EstimatedMemoryBytes = 14L * 1024 * 1024 * 1024, IsGpuCompatible = true },
            new ModelConfig { Name = "Llama-7B-INT4", Path = @"models\llama-7b-int4.onnx", EstimatedMemoryBytes = 4L * 1024 * 1024 * 1024, IsGpuCompatible = true },
            new ModelConfig { Name = "Phi-2-FP16",   Path = @"models\phi-2-fp16.onnx",   EstimatedMemoryBytes = 4L * 1024 * 1024 * 1024, IsGpuCompatible = true },
            new ModelConfig { Name = "Phi-2-INT4",   Path = @"models\phi-2-int4.onnx",   EstimatedMemoryBytes = 1536L * 1024 * 1024, IsGpuCompatible = true } // ~1.5GB
        };
    }

    // 3. Dynamic Loader & 4. Fallback Mechanism
    public class ModelLoader
    {
        private readonly HardwareMonitor _monitor;

        public ModelLoader()
        {
            _monitor = new HardwareMonitor();
        }

        public InferenceSession LoadOptimalModel()
        {
            long availableVRam = _monitor.GetAvailableVRamBytes();
            
            // Filter models that fit in memory, sorted by capability (prefer FP16 > INT4, higher params first)
            var candidateModels = ModelRegistry.AvailableModels
                .Where(m => m.EstimatedMemoryBytes <= availableVRam)
                .OrderByDescending(m => m.Name.Contains("FP16")) // Prefer FP16
                .ThenByDescending(m => m.Name.Contains("7B"))    // Prefer larger models
                .ToList();

            SessionOptions sessionOptions = new SessionOptions();
            InferenceSession session;

            if (candidateModels.Any())
            {
                var selectedModel = candidateModels.First();
                Console.WriteLine($"Loading model: {selectedModel.Name} (Requires: {selectedModel.EstimatedMemoryBytes / 1024 / 1024 / 1024}GB, Available: {availableVRam / 1024 / 1024 / 1024}GB)");

                // Configure GPU execution provider
                try
                {
                    sessionOptions.AppendExecutionProviderDml(0); // DirectML
                }
                catch (Exception)
                {
                    Console.WriteLine("Warning: GPU provider failed to initialize. Falling back to CPU.");
                    sessionOptions.AppendExecutionProviderCpu();
                }

                session = new InferenceSession(selectedModel.Path, sessionOptions);
            }
            else
            {
                // 5. Fallback to smallest model on CPU
                Console.WriteLine("Warning: Insufficient VRAM for any GPU model. Falling back to CPU with Phi-2-INT4.");
                var fallbackModel = ModelRegistry.AvailableModels.Last(); // Phi-2-INT4
                
                sessionOptions.AppendExecutionProviderCpu();
                session = new InferenceSession(fallbackModel.Path, sessionOptions);
            }

            return session;
        }
    }
}
