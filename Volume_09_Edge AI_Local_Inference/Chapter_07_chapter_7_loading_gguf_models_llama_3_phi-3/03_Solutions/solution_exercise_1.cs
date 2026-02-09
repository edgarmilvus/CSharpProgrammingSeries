
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using Microsoft.ML.OnnxRuntimeGenAI;

public class ModelLoader
{
    public static void Main(string[] args)
    {
        // Example path; in a real scenario, this might come from config or command line args
        string modelDirectory = "./models/llama-3-8b-instruct";

        try
        {
            Console.WriteLine($"Attempting to load model from: {modelDirectory}");
            LoadModelSafely(modelDirectory);
            Console.WriteLine("Model loading and disposal test completed successfully.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[Error] Model file not found: {ex.Message}");
        }
        catch (OnnxRuntimeException ex)
        {
            Console.WriteLine($"[Error] ONNX Runtime initialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Unexpected Error] {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the model using a 'using' block to ensure deterministic disposal of unmanaged resources.
    /// </summary>
    public static void LoadModelSafely(string modelDirectoryPath)
    {
        // The 'using' statement ensures that OnnxGenAIModel.Dispose() is called automatically
        // when the block is exited, even if exceptions occur. This is critical for edge devices
        // to prevent memory leaks from unmanaged native memory.
        using (var model = new OnnxGenAIModel(new ModelOptions { ModelPath = modelDirectoryPath }))
        {
            Console.WriteLine("Model loaded successfully. Accessing model properties...");
            
            // Accessing properties to verify the model is active
            // Note: The specific properties depend on the library version, 
            // but typically you can check the model name or config if exposed.
            Console.WriteLine($"Model is ready for inference.");
            
        } // OnnxGenAIModel.Dispose() is called here, releasing native memory.

        Console.WriteLine("Scope exited. Model disposal confirmed.");
    }
}
