
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LlmParameterTuning
{
    public class LlmEngine
    {
        // 1. UI Parameters (Properties to bind to Sliders/CheckBoxes)
        public float Temperature { get; set; } = 0.7f;
        public int MaxLength { get; set; } = 100;
        public bool IsDeterministic { get; set; } = false;

        public void RunInference(string prompt, InferenceSession session)
        {
            // 2. ONNX Runtime Graph Manipulation / Sampling Logic
            // Note: Most ONNX LLMs output 'logits' as raw floats.
            // We must process these in C# to apply dynamic parameters.
            
            // Simulate retrieving raw logits from the model (Batch=1, SeqLen, VocabSize)
            // In reality: var results = session.Run(inputs);
            // var logitsTensor = results.First().AsTensor<float>();
            
            // Mocking logits for demonstration
            float[] rawLogits = new float[100]; 
            new Random().NextBytes(rawLogits.SelectMany(BitConverter.GetBytes).ToArray());

            // Apply Parameters
            var processedTokens = ApplySampling(rawLogits);
            
            // Use 'processedTokens' to select the next token ID
        }

        private string ApplySampling(float[] logits)
        {
            // 3. Logic Implementation
            
            // A. Deterministic Mode (Greedy Search)
            if (IsDeterministic || Temperature == 0.0f)
            {
                int maxIndex = Array.IndexOf(logits, logits.Max());
                return $"Token_{maxIndex} (Greedy)";
            }

            // B. Temperature Scaling
            // Formula: logits = logits / temperature
            var scaledLogits = logits.Select(l => l / Temperature).ToArray();

            // C. Softmax (Simplified) & Sampling
            // Convert to probabilities
            var exp = scaledLogits.Select(Math.Exp).ToArray();
            var sumExp = exp.Sum();
            var probabilities = exp.Select(e => e / sumExp).ToArray();

            // Weighted Random Selection (Top-P / Top-K logic can be added here)
            var random = new Random();
            var r = random.NextDouble();
            double cumulative = 0.0;

            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulative += probabilities[i];
                if (cumulative > r)
                {
                    return $"Token_{i} (Temp: {Temperature})";
                }
            }

            return "Token_0";
        }
    }
}
