
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

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicOnnxShapes
{
    // Input class with variable length vector (using Array instead of fixed VectorType)
    public class DynamicInput
    {
        public float[] Features { get; set; }
    }

    public class ModelOutput
    {
        [VectorType(2)]
        public float[] SentimentLogits { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 1);

            // 1. Create Data with Varying Lengths
            // Note: Standard ML.NET IDataView requires fixed column types. 
            // However, for ONNX dynamic batching, we usually need to ensure the data 
            // is padded to a uniform length for the IDataView structure, OR we rely on 
            // the ONNX Runtime's ability to handle variable shapes if the model supports it.
            
            // CRITICAL: ML.NET's IDataView is strongly typed. A column usually has a fixed vector size.
            // To simulate dynamic batching in ML.NET, we typically pad inputs to the max length in the batch 
            // or use a specialized data structure. 
            // However, if the ONNX model has dynamic axes (e.g., [batch_size, sequence_length]), 
            // ML.NET's OnnxTransformer can often handle batching if the input tensor is a vector of variable length.
            
            // For this exercise, we will assume the model accepts [batch_size, sequence_length].
            // We will create a batch of 3 inputs.
            
            var data = new List<DynamicInput>
            {
                new DynamicInput { Features = GenerateRandomFeatures(50) },  // Short sequence
                new DynamicInput { Features = GenerateRandomFeatures(100) }, // Medium sequence
                new DynamicInput { Features = GenerateRandomFeatures(128) }  // Long sequence
            };

            // Load data
            var dataView = mlContext.Data.LoadFromEnumerable(data);

            // 2. Configure OnnxTransformer for Dynamic Shapes
            // The standard ApplyOnnxModel might try to infer fixed shapes. 
            // To handle dynamic axes, we often need to use the 'shapeDictionary' parameter or 
            // ensure the model is exported with dynamic axes properly.
            
            // In ML.NET, if the model has dynamic axes, the transformer usually infers the shape 
            // from the incoming IDataView row. Since our IDataView column is defined as 'float[]' (variable),
            // ML.NET will pass the actual array length to the ONNX runtime.
            
            // However, to explicitly force dynamic handling or if inference fails, we use shapeDictionary.
            // Key: Column Name, Value: Array of dimensions (use -1 for dynamic).
            var shapeDictionary = new Dictionary<string, int[]>
            {
                { "Features", new[] { -1, -1 } } // -1 indicates dynamic axis (Batch, Sequence)
            };

            var pipeline = mlContext.Transforms.ApplyOnnxModel(
                outputColumnNames: new[] { "SentimentLogits" },
                inputColumnNames: new[] { "Features" },
                modelFile: "DynamicTextClassifier.onnx", // Assuming this model exists
                shapeDictionary: shapeDictionary,
                fallbackToCpu: true);

            // 3. Fit and Transform
            // Note: If the model file doesn't exist, this will throw an exception.
            // We wrap it in a try-catch for the sake of the exercise solution.
            try 
            {
                var model = pipeline.Fit(dataView);
                var transformedData = model.Transform(dataView);

                // 4. Extract Predictions
                var predictions = mlContext.Data.CreateEnumerable<ModelOutput>(transformedData, reuseRowObject: false).ToList();

                Console.WriteLine("--- Batch Inference Results ---");
                for (int i = 0; i < predictions.Count; i++)
                {
                    var pred = predictions[i];
                    Console.WriteLine($"Row {i + 1} (Input Length: {data[i].Features.Length}): " +
                                      $"Neg: {pred.SentimentLogits[0]:F2}, Pos: {pred.SentimentLogits[1]:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inference failed (likely due to missing model file): {ex.Message}");
                Console.WriteLine("Logic applied: Used shapeDictionary with [-1, -1] to allow dynamic batching.");
            }
        }

        private static float[] GenerateRandomFeatures(int length)
        {
            var rnd = new Random();
            return Enumerable.Range(0, length).Select(_ => (float)rnd.NextDouble()).ToArray();
        }
    }
}
