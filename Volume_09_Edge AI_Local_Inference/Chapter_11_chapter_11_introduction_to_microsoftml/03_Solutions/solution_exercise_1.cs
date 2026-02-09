
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

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnnxInferenceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the model path (ensure this file exists in your execution directory)
            string modelPath = "TinyBERT_Sentiment.onnx";

            try
            {
                // 1. Initialize the ONNX Runtime Environment
                // Using 'using' ensures proper disposal of the native resources.
                using var ortEnv = OrtEnv.Instance();

                // 2. Create Session Options (Optional: configure execution providers)
                using var sessionOptions = new SessionOptions();
                // sessionOptions.AppendExecutionProvider_CPU(0); // Explicitly use CPU

                // 3. Load the Model
                using var session = new InferenceSession(modelPath, sessionOptions);

                // 4. Prepare Input Data
                var inputTensor = GenerateDummyInput();
                
                // Create the input name (usually defined in the model, but often defaults to "input_ids" or similar)
                // For this exercise, we assume the model has a single input. We can inspect it via session.InputMetadata.
                var inputName = session.InputMetadata.Keys.First();
                
                // Create the OrtValue from the Tensor
                // Note: OrtValue.CreateFromTensor wraps the memory efficiently without deep copying if possible.
                using var inputOrtValue = OrtValue.CreateFromTensor(inputTensor);

                // 5. Run Inference
                // Inputs are provided as a list of (name, OrtValue) pairs.
                var inputs = new List<NamedOrtValue>
                {
                    new NamedOrtValue(inputName, inputOrtValue)
                };

                // Run the session. Outputs are returned as a list of OrtValue objects.
                using var outputs = session.Run(inputs);

                // 6. Process Output
                // Assuming the model outputs a single tensor of shape [1, 2]
                var outputTensor = outputs[0].AsTensor<float>();
                
                // Extract logits
                float negativeLogit = outputTensor[0, 0];
                float positiveLogit = outputTensor[0, 1];

                // 7. Print Results
                Console.WriteLine("--- Inference Results ---");
                Console.WriteLine($"Negative Logit: {negativeLogit:F4}");
                Console.WriteLine($"Positive Logit: {positiveLogit:F4}");
                Console.WriteLine("-------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Ensure 'TinyBERT_Sentiment.onnx' is in the output directory.");
            }
        }

        /// <summary>
        /// Generates a dummy input tensor of shape [1, 128] filled with random floats.
        /// </summary>
        /// <returns>A DenseTensor<float> representing token embeddings.</returns>
        private static DenseTensor<float> GenerateDummyInput()
        {
            int batchSize = 1;
            int sequenceLength = 128;
            int totalElements = batchSize * sequenceLength;

            // Generate random data
            var random = new Random();
            var data = new float[totalElements];
            for (int i = 0; i < totalElements; i++)
            {
                data[i] = (float)random.NextDouble(); // Range 0.0 to 1.0
            }

            // Create the DenseTensor
            // The shape is passed as an array of integers.
            return new DenseTensor<float>(data, new[] { batchSize, sequenceLength });
        }
    }
}
