
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
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
*/

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Real-world context: You are building a simple sentiment analysis feature for a .NET console application.
// You have a pre-trained ONNX model (e.g., a distilled version of BERT) that classifies text as Positive, Negative, or Neutral.
// This example demonstrates loading the model, preparing input data, running inference, and interpreting the output.

namespace OnnxInferenceHelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("ONNX Runtime Hello World - Sentiment Analysis");
            Console.WriteLine("==============================================\n");

            // 1. Define the path to the ONNX model.
            // In a real app, this might be downloaded from a server or bundled as a resource.
            // For this example, we assume a model named 'sentiment_bert.onnx' exists in the execution directory.
            // The model expects a string input and outputs logits for 3 classes.
            string modelPath = "sentiment_bert.onnx";

            try
            {
                // 2. Define the input data.
                // We want to analyze the sentiment of the sentence: "The new update runs incredibly fast!"
                string inputText = "The new update runs incredibly fast!";

                // 3. Pre-processing: Convert text to numerical input (Tensor).
                // Real-world apps use Tokenizers (e.g., Microsoft.ML.Tokenizers) to map words to IDs.
                // For this "Hello World", we simulate token IDs for demonstration purposes.
                // We assume the model expects input shape [1, 128] (Batch Size 1, Sequence Length 128).
                // We will fill the first few tokens with dummy IDs and pad the rest with 0.
                long[] tokenIds = new long[128];
                
                // Simulating tokenization: "The"=101, "new"=2054, "update"=8321, "runs"=4567, "incredibly"=9876, "fast"=3456, "!"=102
                // In a real scenario, this is done by a dedicated Tokenizer class.
                var simulatedTokens = new long[] { 101, 2054, 8321, 4567, 9876, 3456, 102 };
                Array.Copy(simulatedTokens, tokenIds, simulatedTokens.Length);

                // 4. Create the ONNX Runtime Session.
                // We use 'Using' statements to ensure resources are disposed of correctly.
                // We specify the execution provider. CPU is the most compatible. 
                // If you have a GPU, you would use 'ExecutionProvider.Dml' (DirectML) or 'Cuda'.
                var sessionOptions = new SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOG_LEVEL_WARNING;

                // Load the model into memory.
                // This validates the model structure but doesn't run it yet.
                using var session = new InferenceSession(modelPath, sessionOptions);

                // 5. Prepare the Inputs.
                // ONNX Runtime expects a list of 'NamedOnnxValue' objects.
                // The name "input_ids" must match the input name in the ONNX model file exactly.
                // We wrap our long array into a DenseTensor (a standard tensor implementation).
                var inputTensor = new DenseTensor<long>(tokenIds, new[] { 1, 128 });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
                };

                Console.WriteLine($"Running inference on text: \"{inputText}\"");
                Console.WriteLine($"Input Tensor Shape: [1, 128]");

                // 6. Run Inference.
                // This is the heavy lifting. The session executes the computational graph.
                // We use 'RunAsync' to keep the UI responsive if this were a GUI app.
                using var results = await session.RunAsync(inputs);

                // 7. Post-processing: Extract and Interpret Results.
                // The model outputs a tensor of logits (raw scores).
                // We look for the output name. Usually, it's something like "logits" or "output".
                // For this example, we assume the output name is "logits".
                var outputTensor = results.First().AsTensor<float>();
                
                // Convert logits to probabilities using Softmax (simplified here for clarity).
                // We simply find the index of the highest score (ArgMax).
                // 0 = Negative, 1 = Neutral, 2 = Positive
                int predictedClass = 0;
                float maxScore = outputTensor.GetValue(0);
                
                for (int i = 1; i < outputTensor.Dimensions[1]; i++)
                {
                    float currentScore = outputTensor.GetValue(i);
                    if (currentScore > maxScore)
                    {
                        maxScore = currentScore;
                        predictedClass = i;
                    }
                }

                string sentiment = predictedClass switch
                {
                    0 => "Negative",
                    1 => "Neutral",
                    2 => "Positive",
                    _ => "Unknown"
                };

                Console.WriteLine("\n--- Inference Results ---");
                Console.WriteLine($"Raw Logits: [{string.Join(", ", outputTensor.ToArray().Select(f => f.ToString("F4")))}]");
                Console.WriteLine($"Predicted Class Index: {predictedClass}");
                Console.WriteLine($"Detected Sentiment: {sentiment}");
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: Model file not found at '{modelPath}'.");
                Console.WriteLine("Please ensure the ONNX model file exists in the application directory.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
