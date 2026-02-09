
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

using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class TopKSamplingEngine
{
    // 1. Vocabulary Caching (Simulated as a static or injected service)
    private static readonly Dictionary<string, int> _cachedVocab = new Dictionary<string, int> 
    { 
        { "The", 1 }, { " quick", 2 }, { " brown", 3 }, { " fox", 4 }, { " jumps", 5 }, { " over", 6 }, { " lazy", 7 }, { " dog", 8 }, { "", 0 } 
    };
    private static readonly Dictionary<int, string> _reverseCachedVocab = _cachedVocab.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public async IAsyncEnumerable<string> GenerateTokensAsync(string prompt, int k, int? seed = null)
    {
        // Random generator for sampling
        Random rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var inputTokens = new List<int> { 1 }; // "The"
        
        // Simulated ONNX output logits (Shape: [1, 1, VocabSize])
        // We will mock the output to demonstrate the math
        int vocabSize = _cachedVocab.Count;
        
        for (int i = 0; i < 10; i++)
        {
            // --- MOCK ONNX OUTPUT ---
            // Generate random logits to simulate model output
            var logits = new float[vocabSize];
            for(int j=0; j<vocabSize; j++) logits[j] = (float)rng.NextDouble() * 10;
            
            // 2. Edge Case: Handle NaN/Infinity
            // Clamp values to safe range to prevent Softmax explosion
            const float maxLogit = 20.0f;
            for (int j = 0; j < vocabSize; j++)
            {
                if (float.IsNaN(logits[j]) || float.IsInfinity(logits[j])) logits[j] = -100; // Mask bad values
                logits[j] = Math.Clamp(logits[j], -maxLogit, maxLogit);
            }

            // --- TOP-K LOGIC ---
            
            // 3. Get Top K indices and values
            var topK = logits
                .Select((val, idx) => new { Value = val, Index = idx })
                .OrderByDescending(x => x.Value)
                .Take(k)
                .ToList();

            // 4. Normalize Probabilities (Softmax on top K only)
            // Subtract max logit for numerical stability before exp
            float maxTopKLogit = topK.Max(x => x.Value);
            float sumExp = topK.Sum(x => MathF.Exp(x.Value - maxTopKLogit));
            
            // Select based on probability
            double r = rng.NextDouble();
            double cumulative = 0.0;
            int selectedId = topK[0].Index; // Default to first

            foreach (var item in topK)
            {
                float prob = MathF.Exp(item.Value - maxTopKLogit) / sumExp;
                cumulative += prob;
                if (r <= cumulative)
                {
                    selectedId = item.Index;
                    break;
                }
            }

            if (selectedId == 0) yield break;

            yield return _reverseCachedVocab[selectedId];
            inputTokens.Add(selectedId);
        }
    }
}
