
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class ModelManager : IDisposable
{
    private InferenceSession _currentSession;
    private string _basePath;
    
    // Dictionary to map user selection to model filenames
    private readonly Dictionary<string, string> _modelMap = new Dictionary<string, string>
    {
        { "Standard", "yolov8n.onnx" },
        { "Quantized", "yolov8n_int8.onnx" } // Hypothetical optimized model
    };

    public ModelManager(string basePath)
    {
        _basePath = basePath;
    }

    public void LoadModel(string modelKey, bool useGpu = false)
    {
        // 1. Clean up previous session
        Dispose();

        if (!_modelMap.TryGetValue(modelKey, out var fileName))
        {
            throw new ArgumentException("Invalid model key");
        }

        string modelPath = Path.Combine(_basePath, fileName);
        if (!File.Exists(modelPath)) throw new FileNotFoundException($"Model not found at {modelPath}");

        // 2. Configure Session Options
        var options = new SessionOptions();
        
        // Graph Optimization Level (ORT_ENABLE_ALL is aggressive)
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

        // 3. Execution Provider Selection
        if (useGpu)
        {
            // Check available providers dynamically
            var availableProviders = InferenceSession.GetAvailableProviders();
            
            if (Array.Exists(availableProviders, p => p == "CUDAExecutionProvider"))
            {
                options.AppendExecutionProvider_CUDA();
                Console.WriteLine("Loaded with CUDA Execution Provider.");
            }
            else if (Array.Exists(availableProviders, p => p == "TensorrtExecutionProvider"))
            {
                options.AppendExecutionProvider_TensorRT();
                Console.WriteLine("Loaded with TensorRT Execution Provider.");
            }
            else if (Array.Exists(availableProviders, p => p == "OpenVINOExecutionProvider"))
            {
                options.AppendExecutionProvider_OpenVINO();
                Console.WriteLine("Loaded with OpenVINO Execution Provider.");
            }
            else
            {
                Console.WriteLine("GPU requested but no compatible EP found. Falling back to CPU.");
            }
        }
        else
        {
            Console.WriteLine("Loaded with CPU Execution Provider.");
        }

        // 4. Load Session
        _currentSession = new InferenceSession(modelPath, options);
    }

    public (Tensor<float> Output, long MemoryUsage) RunInference(Tensor<float> inputTensor)
    {
        if (_currentSession == null) throw new InvalidOperationException("Model not loaded.");

        // Measure Memory Usage (Approximate)
        long memBefore = GC.GetTotalMemory(true);
        
        using var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };
        using var results = _currentSession.Run(inputs);
        
        // Get output tensor
        var outputTensor = results.First().AsTensor<float>();
        
        // Clone output to avoid disposing it when 'results' is disposed, 
        // or keep 'results' alive if using it directly in downstream logic.
        // For this example, we assume the caller handles the output lifecycle.
        // To measure memory accurately, we force GC after the operation is done.
        
        long memAfter = GC.GetTotalMemory(false); // Don't force collection yet to see live usage
        
        return (outputTensor, memAfter - memBefore);
    }

    public void Dispose()
    {
        if (_currentSession != null)
        {
            _currentSession.Dispose();
            _currentSession = null;
        }
    }

    // Method to get VRAM usage (Platform specific, usually requires interop)
    public long GetVRamUsage()
    {
        // This is highly platform-dependent. 
        // On Windows with NVIDIA, one might use NVML (NVAPI) interop.
        // For this exercise, we return a placeholder or 0 if not implemented.
        // In a real app, you would bind to nvml.dll or similar.
        return 0; 
    }
}

// UI Interaction Example (Console based for brevity)
public class ModelSwitcherDemo
{
    public void Run()
    {
        var manager = new ModelManager("./models");
        
        // 1. Load Standard Model (CPU)
        manager.LoadModel("Standard", useGpu: false);
        
        // 2. Simulate Inference
        var dummyInput = new DenseTensor<float>(new[] { 1, 3, 640, 640 });
        var result1 = manager.RunInference(dummyInput);
        Console.WriteLine($"Standard Model - Memory Delta: {result1.MemoryUsage} bytes");

        // 3. Switch to Optimized/Quantized Model (Try GPU)
        manager.LoadModel("Quantized", useGpu: true); // Will try to find GPU
        
        var result2 = manager.RunInference(dummyInput);
        Console.WriteLine($"Optimized Model - Memory Delta: {result2.MemoryUsage} bytes");
        
        manager.Dispose();
    }
}
