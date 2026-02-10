
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace QuantizationBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            // Mock paths for demonstration. In a real scenario, these would be valid file paths.
            var modelPaths = new Dictionary<string, string>
            {
                { "FP32", "model_fp32.onnx" },
                { "FP16", "model_fp16.onnx" },
                { "INT8", "model_int8.onnx" }
            };

            // 1. Generate dummy input tensor [1, 128]
            var inputTensor = GenerateRandomTensor(1, 128);
            var inputName = "input_ids"; // Assuming standard input name, adjust as needed

            Console.WriteLine("{0,-10} | {1,-18} | {2,-18} | {3,-15} | {4,-20}", 
                "Model", "Time (ms)", "Memory (KB)", "Logits Variance", "First 5 Logits");
            Console.WriteLine(new string('-', 100));

            foreach (var kvp in modelPaths)
            {
                try 
                {
                    RunInference(kvp.Key, kvp.Value, inputName, inputTensor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{kvp.Key,-10} | Error: {ex.Message}");
                }
            }
        }

        static DenseTensor<float> GenerateRandomTensor(int batchSize, int seqLength)
        {
            var rng = new Random();
            var data = new float[batchSize * seqLength];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (float)rng.NextDouble(); // Values between 0.0 and 1.0
            }
            return new DenseTensor<float>(data, new[] { batchSize, seqLength });
        }

        static void RunInference(string modelName, string modelPath, string inputName, Tensor<float> inputTensor)
        {
            // 2. Setup Session Options
            using var sessionOptions = new SessionOptions();
            
            // For quantized models, ensure CPU provider is set explicitly if needed
            // Default is usually CPU, but explicit is safer for benchmarks
            sessionOptions.AppendExecutionProvider_CPU(); 

            // 3. Load Model
            using var session = new InferenceSession(modelPath, sessionOptions);

            // 4. Create OrtValue from Tensor
            using var inputOrtValue = OrtValue.CreateTensorValueFromMemory(inputTensor.Buffer, 
                inputTensor.Dimensions.ToArray());

            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputOrtValue) };

            // 5. Metrics Collection - Memory & Time
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long memBefore = GC.GetTotalMemory(true);

            var stopwatch = Stopwatch.StartNew();
            
            using var results = session.Run(inputs); // Run inference
            
            stopwatch.Stop();
            
            long memAfter = GC.GetTotalMemory(true);
            long memDiff = memAfter - memBefore;

            // 6. Output Analysis
            var outputTensor = results.First().AsTensor<float>();
            var outputArray = outputTensor.ToArray();

            // Calculate Variance
            double mean = outputArray.Average();
            double sumOfSquares = outputArray.Sum(val => Math.Pow(val - mean, 2));
            double variance = sumOfSquares / outputArray.Length;

            // Get first 5 values
            var firstFive = outputArray.Take(5).Select(v => v.ToString("F4"));

            // 7. Reporting
            Console.WriteLine("{0,-10} | {1,-18} | {2,-18} | {3,-15:F4} | {4,-20}", 
                modelName, 
                stopwatch.Elapsed.TotalMilliseconds.ToString("F2"), 
                (memDiff / 1024.0).ToString("F2"), 
                variance, 
                string.Join(", ", firstFive));
        }
    }
}
