
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
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class OnnxInspector
{
    /// <summary>
    /// Loads an ONNX model and inspects its metadata and signature.
    /// </summary>
    /// <param name="modelPath">The file path to the .onnx model.</param>
    /// <param name="targetInputName">The specific input name to search for.</param>
    public static void InspectModel(string modelPath, string targetInputName = "input_ids")
    {
        try
        {
            // Using statement ensures proper disposal of the InferenceSession and underlying resources.
            using var session = new InferenceSession(modelPath);
            
            // 1. Retrieve Model Metadata
            var modelMetadata = session.ModelMetadata;
            Console.WriteLine("--- Model Metadata ---");
            Console.WriteLine($"Producer Name: {modelMetadata.ProducerName}");
            Console.WriteLine($"Graph Name:    {modelMetadata.GraphName}");
            Console.WriteLine($"Version:       {modelMetadata.Version}");
            Console.WriteLine();

            // 2. Inspect Input Nodes
            Console.WriteLine("--- Input Signatures ---");
            bool targetInputFound = false;
            
            // GetInputMetadata returns a read-only dictionary of input names to NodeMetadata
            foreach (var input in session.InputMetadata)
            {
                string name = input.Key;
                var meta = input.Value;
                
                // Handle dynamic dimensions (represented as -1 in ONNX Runtime)
                // We map -1 to '?' as requested.
                var shapeStr = string.Join(", ", meta.Dimensions.Select(d => d == -1 ? "?" : d.ToString()));
                
                Console.WriteLine($"  Input: {name}");
                Console.WriteLine($"    Type: {meta.ElementKind}");
                Console.WriteLine($"    Shape: [{shapeStr}]");

                if (name == targetInputName)
                {
                    targetInputFound = true;
                }
            }
            Console.WriteLine();

            // 3. Inspect Output Nodes
            Console.WriteLine("--- Output Signatures ---");
            foreach (var output in session.OutputMetadata)
            {
                string name = output.Key;
                var meta = output.Value;
                
                Console.WriteLine($"  Output: {name}");
                Console.WriteLine($"    Type: {meta.ElementKind}");
            }
            Console.WriteLine();

            // 4. Challenge: Return boolean logic (simulated via console output here)
            if (targetInputFound)
            {
                Console.WriteLine($"[SUCCESS] Target input '{targetInputName}' was found in the model.");
            }
            else
            {
                Console.WriteLine($"[INFO] Target input '{targetInputName}' was NOT found.");
            }
        }
        catch (OnnxRuntimeException ex)
        {
            Console.WriteLine($"[ERROR] ONNX Runtime Exception: {ex.Message}");
            Console.WriteLine("Ensure the model path is correct and the file is a valid ONNX model.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected Exception: {ex.Message}");
        }
    }
}

// Example usage (commented out as it requires a physical .onnx file)
// public class Program
// {
//     public static void Main()
//     {
//         // Replace with a valid path to an ONNX model (e.g., from Hugging Face)
//         OnnxInspector.InspectModel("path/to/model.onnx", "input_ids");
//     }
// }
