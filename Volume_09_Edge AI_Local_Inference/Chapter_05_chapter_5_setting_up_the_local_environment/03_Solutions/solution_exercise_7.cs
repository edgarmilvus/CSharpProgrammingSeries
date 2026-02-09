
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

# Source File: solution_exercise_7.cs
# Description: Solution for Exercise 7
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalAI.Inference
{
    public class OutputParser
    {
        public static void ParseLogits(OrtValue tensor)
        {
            // 1. Get Span (Zero-copy access to native memory)
            // Assuming the output is a float tensor
            ReadOnlySpan<float> logitsSpan = tensor.GetTensorDataAsSpan<float>();

            // 2. Convert to Array (Copy)
            float[] logits = logitsSpan.ToArray();

            // 3. Calculate Softmax
            // Softmax(x_i) = exp(x_i) / sum(exp(x_j))
            // We use Math.Exp which works on double, so we cast.
            // For production, use Vector<T> for SIMD acceleration.
            
            var softmaxScores = new double[logits.Length];
            double sumExp = 0.0;

            for (int i = 0; i < logits.Length; i++)
            {
                double exp = Math.Exp(logits[i]);
                softmaxScores[i] = exp;
                sumExp += exp;
            }

            // Normalize
            for (int i = 0; i < softmaxScores.Length; i++)
            {
                softmaxScores[i] /= sumExp;
            }

            // 4. Get Top 5
            var top5 = softmaxScores
                .Select((score, index) => new { Index = index, Score = score })
                .OrderByDescending(x => x.Score)
                .Take(5);

            Console.WriteLine("Top 5 Predicted Tokens:");
            foreach (var item in top5)
            {
                Console.WriteLine($"Token ID: {item.Index}, Probability: {item.Score:P2}");
            }
        }
    }
}
