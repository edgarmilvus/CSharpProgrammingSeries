
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime; // Required for the ONNX runner

// 1. The Interface
public interface IModelInference
{
    Task<string> RunInferenceAsync(string input);
    void LoadModel(string modelPath);
}

// 2. ONNX Implementation
public class OnnxModelRunner : IModelInference
{
    private InferenceSession? _session;

    public void LoadModel(string modelPath)
    {
        if (!modelPath.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid extension for ONNX runner.");
            
        _session = new InferenceSession(modelPath);
        Console.WriteLine("ONNX Model loaded successfully.");
    }

    public async Task<string> RunInferenceAsync(string input)
    {
        if (_session == null) throw new InvalidOperationException("Model not loaded.");

        // Simulation of ONNX inference
        // In a real scenario, you would create an OrtValue, run _session.Run(), etc.
        await Task.Delay(50); // Simulate GPU latency
        return $"[ONNX Result] Processed: {input}";
    }
}

// 3. GGUF Implementation
public class GgufModelRunner : IModelInference
{
    public void LoadModel(string modelPath)
    {
        if (!modelPath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid extension for GGUF runner.");

        // We reuse the parser from Exercise 2 to validate the file
        try
        {
            // Assuming GGUFParser class exists from previous exercise
            // For self-containment, we can inline the magic check or assume it's available.
            using var fs = new FileStream(modelPath, FileMode.Open, FileAccess.Read);
            var magicBytes = new byte[4];
            fs.Read(magicBytes, 0, 4);
            if (System.Text.Encoding.ASCII.GetString(magicBytes) != "GGUF")
                throw new InvalidDataException("Invalid GGUF file.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to validate GGUF header.", ex);
        }

        Console.WriteLine("GGUF Model validated and ready (CPU mode).");
    }

    public async Task<string> RunInferenceAsync(string input)
    {
        // Simulate CPU-bound processing
        await Task.Delay(200); // Slower than ONNX in this simulation
        return $"[GGUF Result] CPU Processed: {input}";
    }
}

// 4. The Factory
public static class ModelAdapterFactory
{
    public static IModelInference CreateRunner(string modelPath)
    {
        if (modelPath.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
        {
            return new OnnxModelRunner();
        }
        else if (modelPath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase))
        {
            return new GgufModelRunner();
        }
        else
        {
            throw new NotSupportedException($"File format for {modelPath} is not supported.");
        }
    }
}

// 5. Main Program (Interactive)
public class RouterProgram
{
    public static async Task Main()
    {
        Console.WriteLine("Select Model Format:");
        Console.WriteLine("1. ONNX (GPU/CPU)");
        Console.WriteLine("2. GGUF (CPU Only)");
        Console.Write("Choice (1 or 2): ");
        
        var choice = Console.ReadLine();
        string extension = choice == "1" ? ".onnx" : ".gguf";
        
        // Simulate a file path based on choice
        string modelPath = $"model{extension}";
        
        try
        {
            // Factory creates the correct runner
            IModelInference runner = ModelAdapterFactory.CreateRunner(modelPath);
            
            // Load the model (validates file)
            runner.LoadModel(modelPath);

            // Interactive Inference
            Console.Write("Enter prompt: ");
            string? prompt = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                Console.WriteLine("Running inference...");
                string result = await runner.RunInferenceAsync(prompt);
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
    }
}
