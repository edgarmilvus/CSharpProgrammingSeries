
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Transforms;

namespace EdgeAI_HelloWorld
{
    // 1. Define input data schema
    // Represents the input tensor for the model. 
    // For this example, we assume a model expecting a 1D float array of size 3.
    public class ModelInput
    {
        // The 'ColumnName' attribute maps this property to the specific input tensor name 
        // defined in the ONNX model (usually found via Netron).
        // If the model has a generic input name like 'input', use that here.
        [ColumnName("input")]
        public float[] Features { get; set; }
    }

    // 2. Define output data schema
    // Represents the prediction result returned by the model.
    public class ModelOutput
    {
        // 'ColumnName' must match the output tensor name in the ONNX model.
        [ColumnName("output")]
        public float[] Predictions { get; set; }

        // Optional: Add a property for the predicted label (if classification)
        public string PredictedLabel { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize the MLContext
            // This is the starting point for all ML.NET operations. 
            // It provides logging, catalog access, and environment configuration.
            var mlContext = new MLContext(seed: 0);

            // --- MOCK DATA GENERATION ---
            // In a real scenario, you would load data from a file or stream.
            // Here, we create a dummy data collection to simulate input.
            // We assume the ONNX model expects a vector of 3 floats.
            var dummyData = new List<ModelInput>
            {
                new ModelInput { Features = new float[] { 1.0f, 2.0f, 3.0f } },
                new ModelInput { Features = new float[] { 0.1f, 0.2f, 0.3f } }
            };

            // Convert the list to an IDataView, which is the standard data format for ML.NET pipelines.
            IDataView dataView = mlContext.Data.LoadFromEnumerable(dummyData);

            // --- MODEL LOADING ---
            // Define the path to the ONNX model. 
            // For this example, we point to a hypothetical file. 
            // Ensure the file exists or create a dummy one for the code to run without error.
            // Note: In production, use Path.Combine with application directories.
            string modelPath = "model.onnx"; 

            // Check if model exists; if not, create a dummy file for demonstration purposes.
            // WARNING: This is purely for the code snippet to be executable. 
            // Real usage requires a valid ONNX model file (e.g., exported from PyTorch/TensorFlow).
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"Model file '{modelPath}' not found. Creating a dummy file for demonstration.");
                // In a real scenario, you would download or locate the actual model.
                // We cannot execute inference without a valid binary ONNX file.
                // For the sake of the example, we will skip the actual transformation step 
                // if the file is missing, but show the syntax.
                return; 
            }

            // Create a pipeline to load the ONNX model.
            // We use the 'OnnxTransformer' to map the input data to the model.
            var pipeline = mlContext.Transforms.ApplyOnnxModel(
                modelFile: modelPath,
                inputColumnNames: new[] { "input" }, // Matches ModelInput column name
                outputColumnNames: new[] { "output" } // Matches ModelOutput column name
            );

            // Fit the model to the data (loading the model into memory)
            // In ONNX scenarios with ML.NET, 'Fit' primarily prepares the transformer 
            // with the model file path and configuration. It doesn't train weights.
            var model = pipeline.Fit(dataView);

            // --- INFERENCE ---
            // Create a PredictionEngine to perform single-item inference.
            // This engine is not thread-safe.
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);

            // Create a test input
            var testInput = new ModelInput
            {
                Features = new float[] { 5.0f, 10.0f, 15.0f }
            };

            // Predict
            var prediction = predictionEngine.Predict(testInput);

            // Output results
            Console.WriteLine("Inference Complete.");
            Console.WriteLine($"Input Vector: [{string.Join(", ", testInput.Features)}]");
            Console.WriteLine($"Output Vector: [{string.Join(", ", prediction.Predictions)}]");
            
            // --- BATCH PREDICTION (Optional but recommended for performance) ---
            Console.WriteLine("\n--- Batch Prediction Example ---");
            var batchData = new List<ModelInput>
            {
                new ModelInput { Features = new float[] { 1f, 1f, 1f } },
                new ModelInput { Features = new float[] { 2f, 2f, 2f } }
            };
            
            var batchView = mlContext.Data.LoadFromEnumerable(batchData);
            var transformedBatch = model.Transform(batchView);
            
            // Retrieve predictions from the IDataView
            var batchPredictions = mlContext.Data.CreateEnumerable<ModelOutput>(transformedBatch, reuseRowObject: false).ToList();

            foreach (var pred in batchPredictions)
            {
                Console.WriteLine($"Batch Output: [{string.Join(", ", pred.Predictions)}]");
            }
        }
    }
}
