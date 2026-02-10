
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AccuracyAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Synthetic Dataset
            var prompts = new List<string>();
            for (int i = 0; i < 50; i++) prompts.Add($"Prompt number {i} with varying text content.");

            // 2. Dual Inference Setup
            string fp32Model = "model_fp32.onnx";
            string int8Model = "model_int8.onnx";
            
            var resultsCsv = new List<string>();
            resultsCsv.Add("Prompt,FP32_Variance,INT8_Variance,Cosine_Similarity,Status");

            foreach (var prompt in prompts)
            {
                // Tokenize (Mocking tokenization to create float input)
                var inputTensor = MockTokenize(prompt); 

                // Run Inference
                var fp32Output = RunInference(fp32Model, inputTensor);
                var int8Output = RunInference(int8Model, inputTensor);

                // 3. Metric Calculation
                double cosineSim = CalculateCosineSimilarity(fp32Output, int8Output);
                double varFp32 = CalculateVariance(fp32Output);
                double varInt8 = CalculateVariance(int8Output);

                // 4. Thresholding
                bool pass = cosineSim >= 0.95;
                string status = pass ? "Pass" : "Fail";

                // 5. Reporting
                string csvLine = $"\"{prompt.Substring(0, Math.Min(20, prompt.Length))}\",{varFp32:F4},{varInt8:F4},{cosineSim:F4},{status}";
                resultsCsv.Add(csvLine);
            }

            File.WriteAllLines("degradation_report.csv", resultsCsv);
            Console.WriteLine("Analysis complete. Report saved to degradation_report.csv");
        }

        static float[] RunInference(string modelPath, Tensor<float> input)
        {
            using var session = new InferenceSession(modelPath);
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", input) };
            using var results = session.Run(inputs);
            return results.First().AsTensor<float>().ToArray();
        }

        static double CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            // 3. Cosine Similarity Formula
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += vecA[i] * vecA[i];
                normB += vecB[i] * vecB[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        static double CalculateVariance(float[] vec)
        {
            double mean = vec.Average();
            double sumSq = vec.Sum(x => Math.Pow(x - mean, 2));
            return sumSq / vec.Length;
        }

        static DenseTensor<float> MockTokenize(string prompt)
        {
            // Mocking tokenization to float tensor [1, 128]
            var rng = new Random(prompt.GetHashCode()); // Deterministic based on prompt
            var data = new float[128];
            for (int i = 0; i < 128; i++) data[i] = (float)rng.NextDouble();
            return new DenseTensor<float>(data, new[] { 1, 128 });
        }
    }
}
