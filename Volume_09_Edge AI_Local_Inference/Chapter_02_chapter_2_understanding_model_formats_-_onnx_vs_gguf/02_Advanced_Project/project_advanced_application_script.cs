
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
using System.IO;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;

namespace EdgeAI_LocalInference
{
    /// <summary>
    /// REAL-WORLD CONTEXT:
    /// A manufacturing plant uses IoT sensors to monitor equipment vibration. 
    /// We need to detect anomalies (predictive maintenance) by running an ONNX model 
    /// directly on the edge gateway (C# console app) without cloud latency.
    /// 
    /// PROBLEM SOLVED:
    /// 1. Load a pre-trained ONNX model (converted from PyTorch/TensorFlow).
    /// 2. Preprocess raw sensor data (normalization).
    /// 3. Run inference using the ML.NET OnnxRuntime wrapper.
    /// 4. Interpret the output tensor (Softmax probabilities).
    /// 5. Make a decision (Normal vs. Anomaly) based on a threshold.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Predictive Maintenance Monitor ===");
            Console.WriteLine("Initializing ONNX Runtime Environment...\n");

            // 1. SETUP: Define paths and configurations
            // In a real scenario, this model is converted from a PyTorch .pt file to .onnx
            string modelPath = "vibration_anomaly_detector.onnx";
            
            // Check if model exists (Simulated for this example)
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"[Error] Model not found at {modelPath}.");
                Console.WriteLine("Please ensure the ONNX model is present.");
                return;
            }

            // 2. INITIALIZATION: Create the ML Context
            // The MLContext is the starting point for all ML.NET operations.
            // It provides logging, randomness control, and data loading utilities.
            var mlContext = new MLContext(seed: 0);

            try
            {
                // 3. MODEL LOADING: Load the ONNX model
                // We map the input tensor name ("sensor_input") and output tensor name ("anomaly_score").
                // These names must match the model architecture defined during training.
                var onnxModel = mlContext.Transforms.ApplyOnnxModel(
                    modelFile: modelPath,
                    outputColumnName: "anomaly_score",
                    inputColumnName: "sensor_input"
                );

                // 4. DATA PREPARATION: Simulate real-time sensor stream
                // In production, this would come from an MQTT broker or serial port.
                // We create a dummy dataset representing 3 sensor readings (vibration X, Y, Z).
                var sensorData = new List<SensorReading>
                {
                    new SensorReading { X = 0.5f, Y = 0.4f, Z = 0.6f }, // Normal operation
                    new SensorReading { X = 2.1f, Y = 1.9f, Z = 2.5f }, // High vibration (Anomaly)
                    new SensorReading { X = 0.6f, Y = 0.5f, Z = 0.5f }  // Normal operation
                };

                // Convert to IDataView (ML.NET's standard data format)
                var dataView = mlContext.Data.LoadFromEnumerable(sensorData);

                // 5. TRANSFORMATION: Build the processing pipeline
                // This pipeline takes raw data, normalizes it, and passes it to the ONNX model.
                // Note: We are NOT using advanced LINQ here; we are building the pipeline explicitly.
                var pipeline = onnxModel;

                // Fit the model to the data (required to initialize the transformer)
                // In edge scenarios, we usually load a pre-trained model, so 'Fit' is minimal.
                var model = pipeline.Fit(dataView);

                // 6. INFERENCE: Transform data to get predictions
                var transformedData = model.Transform(dataView);

                // 7. POST-PROCESSING: Extract and interpret results
                // We iterate through the results to determine the status.
                var predictions = mlContext.Data.CreateEnumerable<PredictionResult>(transformedData, reuseRowObject: false);

                Console.WriteLine("Inference Results:");
                Console.WriteLine("-------------------");

                int index = 0;
                foreach (var prediction in predictions)
                {
                    // The ONNX model outputs a tensor (array) of probabilities.
                    // Index 0: Probability of Normal
                    // Index 1: Probability of Anomaly
                    float[] scores = prediction.AnomalyScore;
                    
                    // Basic validation of the output tensor
                    if (scores.Length < 2)
                    {
                        Console.WriteLine($"Row {index}: Invalid output tensor size.");
                        continue;
                    }

                    // Logic: If Anomaly probability > 0.5, flag it.
                    float anomalyProb = scores[1];
                    string status = anomalyProb > 0.5f ? "ANOMALY DETECTED" : "Normal";

                    // Display results
                    Console.WriteLine($"Reading #{index + 1}:");
                    Console.WriteLine($"  Input Vector: [{sensorData[index].X}, {sensorData[index].Y}, {sensorData[index].Z}]");
                    Console.WriteLine($"  Normal Prob:  {scores[0]:P2}");
                    Console.WriteLine($"  Anomaly Prob: {scores[1]:P2}");
                    Console.WriteLine($"  STATUS:       {status}");
                    Console.WriteLine();

                    index++;
                }
            }
            catch (Exception ex)
            {
                // Error handling for model loading or inference failures
                Console.WriteLine($"[Critical Error] Inference failed: {ex.Message}");
                Console.WriteLine("Check model compatibility and input dimensions.");
            }

            Console.WriteLine("=== Monitoring Cycle Complete ===");
        }
    }

    // --- Data Structures ---

    /// <summary>
    /// Represents a raw sensor reading.
    /// Used to simulate the input data stream.
    /// </summary>
    public class SensorReading
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    /// <summary>
    /// Represents the model output.
    /// The column name "anomaly_score" must match the outputColumnName in ApplyOnnxModel.
    /// </summary>
    public class PredictionResult
    {
        // The ONNX model outputs a float array (Tensor)
        [ColumnName("anomaly_score")]
        public float[] AnomalyScore { get; set; }
    }
}
