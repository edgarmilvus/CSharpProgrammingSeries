
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BertEdgeExercises
{
    public record MultiLabelResult(List<string> Labels, List<float> Confidences);

    public class MultiLabelClassifier
    {
        private readonly List<string> _possibleLabels;
        private readonly float _threshold;

        public MultiLabelClassifier(List<string> possibleLabels, float threshold = 0.5f)
        {
            _possibleLabels = possibleLabels;
            _threshold = threshold;
        }

        public MultiLabelResult ProcessOutput(DenseTensor<float> outputTensor)
        {
            // Output shape is assumed [1, NumLabels]
            // We take the first (and only) row of the batch
            var logits = new float[outputTensor.Dimensions[1]];
            for (int i = 0; i < logits.Length; i++)
            {
                logits[i] = outputTensor[0, i];
            }

            // Apply Sigmoid: 1 / (1 + e^-x)
            // This converts logits to probabilities independent for each label
            var probabilities = logits.Select(x => 1.0f / (1.0f + (float)Math.Exp(-x))).ToArray();

            var predictedLabels = new List<string>();
            var confidences = new List<float>();

            for (int i = 0; i < probabilities.Length; i++)
            {
                if (probabilities[i] >= _threshold)
                {
                    predictedLabels.Add(_possibleLabels[i]);
                    confidences.Add(probabilities[i]);
                }
            }

            return new MultiLabelResult(predictedLabels, confidences);
        }
    }

    public class InteractiveConsoleApp
    {
        public void Run(InferenceSession session, BertInputFactory factory, MultiLabelClassifier classifier)
        {
            Console.WriteLine("Interactive Multi-Label Sentiment Analysis (Type 'exit' to quit)");

            while (true)
            {
                Console.Write("\nEnter text: ");
                string inputText = Console.ReadLine();
                if (string.IsNullOrEmpty(inputText) || inputText.ToLower() == "exit") break;

                // 1. Tokenize (Assuming factory helper exists from Ex 1)
                var input = factory.CreateFromSingleText(inputText); 
                
                // 2. Prepare Onnx Inputs
                var inputTensors = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(input.InputIds, new[] { 1, input.SequenceLength })),
                    NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(input.AttentionMask, new[] { 1, input.SequenceLength }))
                };

                // 3. Run Inference
                using (var results = session.Run(inputTensors))
                {
                    var output = results.First().AsTensor<float>();
                    var denseOutput = output as DenseTensor<float>; // Ensure dense for easy access
                    
                    // 4. Process Output (Sigmoid + Thresholding)
                    var result = classifier.ProcessOutput(denseOutput);

                    // 5. Display Results
                    Console.WriteLine("Predicted Labels:");
                    if (result.Labels.Count == 0)
                    {
                        Console.WriteLine("  None");
                    }
                    else
                    {
                        for (int i = 0; i < result.Labels.Count; i++)
                        {
                            Console.WriteLine($"  - {result.Labels[i]} ({result.Confidences[i]:P2})");
                        }
                    }
                }
            }
        }
    }
    
    // Helper class placeholder for context
    public class BertInputFactory 
    {
        // Implementation details omitted for brevity, assumes logic from Ex 1
        public BertInput CreateFromSingleText(string text) => new BertInput(new long[128], new long[128], new long[128], 128);
    }
}
