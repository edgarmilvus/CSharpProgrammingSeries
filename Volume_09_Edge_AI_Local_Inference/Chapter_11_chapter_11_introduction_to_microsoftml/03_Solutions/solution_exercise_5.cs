
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

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HybridOnnxPipeline
{
    // 1. Data Classes
    public class ModelInput { [VectorType(128)] public float[] Features { get; set; } }
    
    // Intermediate class for ONNX output
    public class OnnxOutput { [VectorType(2)] public float[] SentimentLogits { get; set; } }
    
    // Final Hybrid Prediction
    public class HybridPrediction 
    { 
        public string ModelLabel { get; set; } 
        public string FinalLabel { get; set; } // "Negative", "Positive", or "Uncertain"
        public float ConfidenceScore { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 1);

            // 2. Create Ambiguous Test Data
            // We create a specific input where logits are close: e.g., [0.51, 0.49]
            // This simulates an ambiguous sentence.
            var ambiguousFeatures = new float[128];
            Array.Fill(ambiguousFeatures, 0.5f); 
            // Let's tweak slightly to make it ambiguous but leaning positive
            ambiguousFeatures[0] = 0.6f; // Boost positive slightly
            ambiguousFeatures[1] = 0.4f; // Lower negative slightly

            var data = new List<ModelInput> 
            { 
                new ModelInput { Features = ambiguousFeatures } 
            };
            var dataView = mlContext.Data.LoadFromEnumerable(data);

            // 3. Build Hybrid Pipeline
            // Step A: ONNX Inference
            var pipeline = mlContext.Transforms.ApplyOnnxModel(
                outputColumnNames: new[] { "SentimentLogits" },
                inputColumnNames: new[] { "Features" },
                modelFile: "TinyBERT_Sentiment.onnx")
                
                // Step B: Custom Hybrid Logic
                // We map OnnxOutput -> HybridPrediction
                .Append(mlContext.Transforms.CustomMapping<OnnxOutput, HybridPrediction>((input, output) => 
                {
                    // Calculate Softmax to get probabilities
                    float logitNeg = input.SentimentLogits[0];
                    float logitPos = input.SentimentLogits[1];
                    
                    // Softmax calculation
                    float maxLogit = Math.Max(logitNeg, logitPos);
                    float sumExp = (float)(Math.Exp(logitNeg - maxLogit) + Math.Exp(logitPos - maxLogit));
                    float probPos = (float)Math.Exp(logitPos - maxLogit) / sumExp;
                    float probNeg = 1.0f - probPos;
                    
                    // Determine Max Confidence
                    float confidence = Math.Max(probNeg, probPos);
                    string modelLabel = probPos > probNeg ? "Positive" : "Negative";

                    // Apply Threshold Rule
                    const float threshold = 0.6f;
                    output.ModelLabel = modelLabel;
                    output.ConfidenceScore = confidence;

                    if (confidence < threshold)
                    {
                        output.FinalLabel = "Uncertain";
                    }
                    else
                    {
                        output.FinalLabel = modelLabel;
                    }
                }, contractName: null));

            // 4. Fit and Predict
            var model = pipeline.Fit(dataView);
            var engine = mlContext.Model.CreatePredictionEngine<ModelInput, HybridPrediction>(model);

            var prediction = engine.Predict(data[0]);

            // 5. Output Results
            Console.WriteLine("--- Hybrid Prediction Result ---");
            Console.WriteLine($"Model Raw Label: {prediction.ModelLabel}");
            Console.WriteLine($"Confidence: {prediction.ConfidenceScore:P2}");
            Console.WriteLine($"Final Decision: {prediction.FinalLabel}");
            Console.WriteLine("--------------------------------");
        }
    }
}
