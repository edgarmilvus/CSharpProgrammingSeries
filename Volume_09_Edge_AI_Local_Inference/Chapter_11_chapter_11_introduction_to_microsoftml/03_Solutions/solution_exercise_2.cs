
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Linq;

namespace MlNetOnnxPipeline
{
    // 1. Data Structures
    public class ModelInput
    {
        // Represents the 128 token embeddings
        [VectorType(128)]
        public float[] Features { get; set; }
    }

    public class ModelOutput
    {
        // Represents the output logits [Negative, Positive]
        [VectorType(2)]
        public float[] SentimentLogits { get; set; }
    }

    public class Prediction
    {
        public string PredictedLabel { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 1);

            // 2. Create Dummy Data
            // In a real scenario, this would come from a file or database.
            // We simulate the feature extraction step here by generating random floats.
            var dummyData = new List<ModelInput> 
            { 
                new ModelInput { Features = GenerateRandomFeatures(128) } 
            };
            var dataView = mlContext.Data.LoadFromEnumerable(dummyData);

            // 3. Build Pipeline
            // Step A: Apply the ONNX Model
            // We map the input column "Features" to the model's input, and the output to "SentimentLogits"
            var pipeline = mlContext.Transforms.ApplyOnnxModel(
                outputColumnNames: new[] { "SentimentLogits" },
                inputColumnNames: new[] { "Features" },
                modelFile: "TinyBERT_Sentiment.onnx");

            // Step B: Post-Processing (Custom Mapping)
            // We need to convert the float[] logits to a string label.
            // We can use a custom mapping or a simple transformation. 
            // Here, we use a custom Action to map the result to the Prediction class directly.
            // Note: In complex scenarios, we might chain a custom ITransformer, but for simple logic,
            // we can use the PredictionEngine's post-processing or a Lambda transform.
            
            // Let's fit the pipeline to the data (this loads the ONNX model)
            var model = pipeline.Fit(dataView);

            // 4. Create Prediction Engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, Prediction>(model);

            // 5. Predict
            var input = dummyData.First();
            var prediction = predictionEngine.Predict(input);

            // 6. Output
            Console.WriteLine($"Predicted Sentiment: {prediction.PredictedLabel}");
        }

        // Helper to generate random features
        private static float[] GenerateRandomFeatures(int length)
        {
            var rnd = new Random();
            var features = new float[length];
            for (int i = 0; i < length; i++) features[i] = (float)rnd.NextDouble();
            return features;
        }
    }
    
    // NOTE: The standard ApplyOnnxModel outputs raw tensors. 
    // To map 'SentimentLogits' (float[]) to 'PredictedLabel' (string), 
    // we need a way to transform the data. 
    // Since the requirement asks for a Prediction class with a string label,
    // we can achieve this by chaining a custom mapping or using a Transformer.
    // However, the `CreatePredictionEngine<ModelInput, Prediction>` expects the output
    // class to match the data flow. 
    
    // To strictly solve this in the prompt's context (Output -> Prediction):
    // We need to modify the pipeline to include a mapping step. 
    // A common ML.NET pattern is to output to a class that contains the logits, 
    // then map that to the final Prediction.
    
    // Let's refine the pipeline logic slightly to ensure it actually produces the string:
    
    /*
       Refined Pipeline Logic (Conceptual):
       1. Input (ModelInput) -> ONNX -> (ModelOutput containing Logits)
       2. ModelOutput -> Custom Logic -> Prediction (String)
       
       Since ApplyOnnxModel maps Input -> Output columns, we can chain a transformation
       that calculates the label.
    */
}

// Re-implementation of the core logic to strictly adhere to the output requirement
namespace MlNetOnnxPipeline_Corrected
{
    public class ModelInput { [VectorType(128)] public float[] Features { get; set; } }
    
    // Intermediate class to hold ONNX output
    public class ModelOutput { [VectorType(2)] public float[] SentimentLogits { get; set; } }
    
    // Final class
    public class FinalPrediction { public string PredictedLabel { get; set; } }

    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 1);
            
            // 1. Load Data
            var data = new List<ModelInput> { new ModelInput { Features = new float[128] } }; // Dummy data
            var dataView = mlContext.Data.LoadFromEnumerable(data);

            // 2. Define Pipeline
            // We use a generic class for the prediction to capture the logits first
            var pipeline = mlContext.Transforms.ApplyOnnxModel(
                outputColumnNames: new[] { "SentimentLogits" },
                inputColumnNames: new[] { "Features" },
                modelFile: "TinyBERT_Sentiment.onnx")
                .Append(mlContext.Transforms.CustomMapping<ModelOutput, FinalPrediction>((input, output) => 
                {
                    // Custom Logic: Compare logits
                    // input.SentimentLogits[0] = Negative, [1] = Positive
                    if (input.SentimentLogits[1] > input.SentimentLogits[0])
                        output.PredictedLabel = "Positive";
                    else
                        output.PredictedLabel = "Negative";
                }, contractName: null));

            // 3. Fit and Create Engine
            var model = pipeline.Fit(dataView);
            var engine = mlContext.Model.CreatePredictionEngine<ModelInput, FinalPrediction>(model);

            // 4. Predict
            var result = engine.Predict(data[0]);
            Console.WriteLine($"Sentiment: {result.PredictedLabel}");
        }
    }
}
