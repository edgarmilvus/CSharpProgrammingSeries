
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
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.Transforms.Onnx;

namespace EdgeAI_SentimentAnalyzer
{
    // 1. DATA MAPPING
    // We define a class to represent the raw input data. In a real-world scenario,
    // this might come from a user interface, a log file, or a sensor reading.
    // This class acts as the "Schema" for our application's input.
    public class SentimentData
    {
        // We map the "Text" column to the 'Comment' property.
        // This corresponds to the input tensor name expected by the ONNX model.
        public string Comment { get; set; }
    }

    // 2. PREDICTION SCHEMA
    // This class defines the structure of the output we expect from the model.
    // The ONNX model might output raw scores (logits) or probabilities.
    public class SentimentPrediction : SentimentData
    {
        // 'Score' holds the raw numerical output from the model.
        // For a sentiment model, this is often a single float representing
        // the probability of the text being positive (or negative).
        public float Score { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 3. INITIALIZATION
            // Instantiate the MLContext. This is the "Gateway" to the entire ML.NET API.
            // It provides logging, seed for randomness, and the environment handle.
            var mlContext = new MLContext();

            // 4. DATA PREPARATION
            // In a real edge application, data streams in constantly. We simulate this
            // by creating a list of raw input objects.
            var inputData = new List<SentimentData>
            {
                new SentimentData { Comment = "This device is incredibly fast and responsive." },
                new SentimentData { Comment = "The battery life is terrible, I am very disappointed." },
                new SentimentData { Comment = "It does the job, but the build quality feels cheap." }
            };

            // Convert the list to an IDataView, which is the standard data format
            // used by ML.NET pipelines.
            IDataView dataView = mlContext.Data.LoadFromEnumerable(inputData);

            // 5. ONNX MODEL CONFIGURATION
            // We define the path to our pre-trained model. In this example, we assume
            // a generic sentiment analysis ONNX model is present in the execution directory.
            // Note: For Phi-3/Llama, the logic is identical, but the input tensor names
            // and output processing logic would change.
            string modelPath = "sentiment_analyzer.onnx";

            // 6. PIPELINE CONSTRUCTION
            // We build a transformation pipeline. Even though we aren't training a model
            // (we are using a pre-trained one), we need a pipeline to transform raw data
            // into model-compatible input.
            var pipeline = mlContext.Transforms.ApplyOnnxModel(
                modelFile: modelPath,
                outputColumnNames: new[] { "score" }, // The name of the output tensor in the ONNX file
                inputColumnNames: new[] { "comment" }  // The name of the input tensor in the ONNX file
            );

            // 7. MODEL FITTING (TRANSFORMATION)
            // The 'Fit' method applies the pipeline to the data. In this context,
            // it loads the ONNX model into memory and prepares the execution engine.
            // The result is a 'Transformer' which can be used to process data.
            var model = pipeline.Fit(dataView);

            // 8. PREDICTION ENGINE CREATION
            // The PredictionEngine is a convenience API that allows for single-instance inference.
            // It is NOT thread-safe. It is designed for low-latency, single-request scenarios
            // common on edge devices.
            var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

            // 9. INFERENCE LOOP
            // We iterate through our raw data to perform local inference.
            Console.WriteLine("--- Edge AI Sentiment Analysis ---");
            foreach (var item in inputData)
            {
                // Run the prediction
                var prediction = predictionEngine.Predict(item);

                // 10. POST-PROCESSING
                // The model outputs a raw score (float). We apply business logic
                // to make this human-readable.
                string sentiment = prediction.Score > 0.5 ? "POSITIVE" : "NEGATIVE";

                // Output the results
                Console.WriteLine($"Input: \"{item.Comment}\"");
                Console.WriteLine($"Score: {prediction.Score:F4} | Verdict: {sentiment}");
                Console.WriteLine(new string('-', 40));
            }
        }
    }
}
