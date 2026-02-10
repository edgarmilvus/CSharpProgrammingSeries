
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace LlamaSharpExercises
{
    public enum SamplingMode { Creative, Precise }

    public class CustomSampler
    {
        public float Temperature { get; set; } = 0.8f;
        public int TopK { get; set; } = 40;
        public float TopP { get; set; } = 0.9f;
        private Random _random = new Random();

        public void SetMode(SamplingMode mode)
        {
            if (mode == SamplingMode.Creative)
            {
                Temperature = 0.9f;
                TopK = 40;
                TopP = 0.95f;
                Console.WriteLine("[System] Switched to Creative Mode");
            }
            else
            {
                Temperature = 0.1f;
                TopK = 1;
                TopP = 1.0f;
                Console.WriteLine("[System] Switched to Precise Mode");
            }
        }

        public int Sample(float[] logits)
        {
            // 1. Temperature Scaling
            ApplyTemperature(logits, Temperature);

            // 2. Top-K Filtering
            if (TopK > 0 && TopK < logits.Length)
            {
                ApplyTopK(logits, TopK);
            }

            // 3. Top-P (Nucleus) Filtering
            if (TopP > 0 && TopP < 1.0f)
            {
                ApplyTopP(logits, TopP);
            }

            // 4. Token Selection (Random Sampling from filtered distribution)
            return SelectToken(logits);
        }

        private void ApplyTemperature(float[] logits, float temp)
        {
            if (temp == 0.0f) return; // Deterministic

            // Numerical stability: clamp temp
            float safeTemp = Math.Max(temp, 0.01f);

            for (int i = 0; i < logits.Length; i++)
            {
                logits[i] /= safeTemp;
            }
        }

        private void ApplyTopK(float[] logits, int k)
        {
            // Efficiently find the k-th largest value using LINQ (Partial Sort concept)
            // For high-performance C++, we'd use a heap, but for C# exercise this is acceptable.
            var sortedIndices = logits
                .Select((val, idx) => (val, idx))
                .OrderByDescending(x => x.val)
                .Take(k)
                .ToList();

            float kthValue = sortedIndices.Last().val;

            // Mask logits
            for (int i = 0; i < logits.Length; i++)
            {
                if (logits[i] < kthValue)
                {
                    logits[i] = float.NegativeInfinity;
                }
            }
        }

        private void ApplyTopP(float[] logits, float p)
        {
            // Convert logits to probabilities (Softmax)
            // We compute softmax on the whole array first
            float[] probs = Softmax(logits);

            // Sort probabilities descending
            var sortedProbs = probs
                .Select((val, idx) => (val, idx))
                .OrderByDescending(x => x.val)
                .ToList();

            float cumulativeProb = 0.0f;
            List<int> validIndices = new List<int>();

            foreach (var (prob, idx) in sortedProbs)
            {
                cumulativeProb += prob;
                validIndices.Add(idx);
                if (cumulativeProb >= p) break;
            }

            // Mask logits: set all non-valid indices to -Infinity
            for (int i = 0; i < logits.Length; i++)
            {
                if (!validIndices.Contains(i))
                {
                    logits[i] = float.NegativeInfinity;
                }
            }
        }

        private int SelectToken(float[] logits)
        {
            // After masking, we need to sample from the remaining valid logits.
            // Convert masked logits to probs again (only non-infinity values matter)
            float[] probs = Softmax(logits);
            
            // Weighted random choice
            double r = _random.NextDouble();
            double cdf = 0.0;

            for (int i = 0; i < probs.Length; i++)
            {
                cdf += probs[i];
                if (r < cdf)
                {
                    return i;
                }
            }

            // Fallback (should rarely happen due to floating point precision)
            return probs.Length - 1;
        }

        private float[] Softmax(float[] logits)
        {
            // Numerically stable softmax
            float maxLogit = logits.Max();
            float sum = 0.0f;
            float[] probs = new float[logits.Length];

            for (int i = 0; i < logits.Length; i++)
            {
                // Subtract max to prevent overflow
                probs[i] = (float)Math.Exp(logits[i] - maxLogit);
                sum += probs[i];
            }

            // Normalize
            for (int i = 0; i < logits.Length; i++)
            {
                probs[i] /= sum;
            }

            return probs;
        }
    }
}
