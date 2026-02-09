
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

public class LLMInferenceEngine : IDisposable
{
    private readonly InferenceSession _session;
    private readonly SimpleTokenizer _tokenizer; // Reusing logic from Ex 2
    private const int MaxContextLength = 2048;

    public LLMInferenceEngine(string modelPath)
    {
        var options = new SessionOptions();
        options.AppendExecutionProvider_CPU(0);
        _session = new InferenceSession(modelPath, options);
        _tokenizer = new SimpleTokenizer(); // Mock tokenizer
    }

    public string Generate(string context, string question)
    {
        // 1. Construct Prompt
        string prompt = $@"<|system|>
You are a helpful AI assistant. Answer the question based only on the provided context.
<|user|>
Context:
{context}

Question: {question}
<|assistant|>";

        // 2. Tokenize
        var inputIds = _tokenizer.Encode(prompt);
        
        // Truncate if necessary
        if (inputIds.Count > MaxContextLength)
        {
            // Keep the end of the prompt (system/user/assistant headers) and truncate context
            inputIds = inputIds.TakeLast(MaxContextLength).ToList();
        }

        // 3. Inference Loop (Greedy Decoding)
        var generatedTokens = new List<long>();
        var currentTokens = inputIds.Select(x => (long)x).ToList();
        
        int maxNewTokens = 100;
        
        for (int i = 0; i < maxNewTokens; i++)
        {
            // Prepare Input Tensor [1, current_length]
            var inputTensor = new DenseTensor<long>(currentTokens.ToArray(), new[] { 1, currentTokens.Count });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", inputTensor) };

            using var results = _session.Run(inputs);
            
            // Get Logits: Shape [1, current_length, vocab_size]
            // We only care about the logits for the *last* token in the sequence
            var logitsTensor = results.First().AsTensor<float>();
            
            // Calculate index of last token's logits
            int vocabSize = logitsTensor.Dimensions[2];
            int lastTokenLogitIndex = (currentTokens.Count - 1) * vocabSize;
            
            // Find Argmax (Token with highest score)
            float maxLogit = float.MinValue;
            int predictedTokenId = 0;
            
            // Optimization: Span access for performance
            var logitsSpan = logitsTensor.Buffer.Span;
            for (int j = 0; j < vocabSize; j++)
            {
                float logit = logitsSpan[lastTokenLogitIndex + j];
                if (logit > maxLogit)
                {
                    maxLogit = logit;
                    predictedTokenId = j;
                }
            }

            // Stop condition (Simulated EOS token, e.g., ID 0 or specific stop word)
            if (predictedTokenId == 0) break; 

            generatedTokens.Add(predictedTokenId);
            currentTokens.Add(predictedTokenId);
        }

        // 4. Decode
        return _tokenizer.Decode(generatedTokens);
    }

    public void Dispose() => _session?.Dispose();
}

// Mock Tokenizer for completeness
public class SimpleTokenizer 
{
    // In a real scenario, this maps strings to IDs and vice versa
    public List<int> Encode(string text) => text.Split(' ').Select(w => Math.Abs(w.GetHashCode()) % 1000).ToList();
    public string Decode(List<long> tokens) => string.Join(" ", tokens.Select(t => $"tok_{t}"));
}
