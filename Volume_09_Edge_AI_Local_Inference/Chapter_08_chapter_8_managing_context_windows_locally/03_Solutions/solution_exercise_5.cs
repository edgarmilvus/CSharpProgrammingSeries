
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalLLM.Inference
{
    // 1. Attention Mask Generator
    public class SlidingWindowMask
    {
        public static Tensor<float> CreateMask(int sequenceLength, int windowSize)
        {
            // Create a lower triangular matrix restricted to the window size
            // Standard attention is full lower triangular. Sliding window is band-limited.
            var mask = new DenseTensor<float>(new[] { sequenceLength, sequenceLength });
            
            for (int row = 0; row < sequenceLength; row++)
            {
                for (int col = 0; col < sequenceLength; col++)
                {
                    // Allow attention if column (query) is within window of row (key)
                    // and column <= row (causal mask)
                    if (col <= row && row - col <= windowSize)
                    {
                        mask[row, col] = 0.0f; // 0.0 allows attention (standard ONNX mask logic)
                    }
                    else
                    {
                        mask[row, col] = -10000.0f; // -inf blocks attention
                    }
                }
            }
            return mask;
        }
    }

    // 2. Token Streamer & 3. C# Implementation
    public class SlidingWindowStreamer
    {
        private readonly int _windowSize;
        private readonly Queue<float[]> _globalMemoryBuffer; // Stores summaries
        private readonly object _lock = new object();

        public SlidingWindowStreamer(int windowSize = 512)
        {
            _windowSize = windowSize;
            _globalMemoryBuffer = new Queue<float[]>();
        }

        public (Tensor<int> InputIds, Tensor<float> AttentionMask) PrepareNextWindow(List<int> fullTokenHistory)
        {
            // 4. Constraint Handling: Determine the active chunk
            int startIdx = Math.Max(0, fullTokenHistory.Count - _windowSize);
            int length = fullTokenHistory.Count - startIdx;
            
            // Extract the last 'windowSize' tokens
            var windowTokens = fullTokenHistory.Skip(startIdx).Take(length).ToArray();

            // Create Input Tensor
            var inputTensor = new DenseTensor<int>(new[] { 1, length }, new[] { 1, length });
            for (int i = 0; i < length; i++) inputTensor[0, i] = windowTokens[i];

            // Create Attention Mask
            var attentionMask = SlidingWindowMask.CreateMask(length, _windowSize);

            return (inputTensor, attentionMask);
        }

        // 4. Global Memory Summary Logic
        public void ProcessWindowSummary(List<int> previousWindowTokens, OnnxInferenceService summarizer)
        {
            // Every N windows (e.g., every 10 windows), generate a summary
            // In a real scenario, we would check a counter here.
            
            // 1. Convert tokens back to text (abstracted)
            string text = Tokenizer.Decode(previousWindowTokens);
            
            // 2. Use a smaller model (Phi-2) to summarize
            // Note: We assume 'summarizer' is a lightweight instance
            string summary = summarizer.GenerateResponseAsync($"Summarize the following text: {text}", new InferenceOptions()).Result;

            // 3. Store summary in global buffer
            // In a real implementation, this summary would be injected into the system prompt of the next window
            Console.WriteLine($"Generated Global Summary: {summary}");
        }
    }

    // Helper for decoding (Mock)
    public static class Tokenizer
    {
        public static string Decode(List<int> tokens) => $"[Decoded text of {tokens.Count} tokens]";
    }

    /*
    // 5. Visualization (Graphviz DOT representation)
    digraph G {
        rankdir=TB;
        node [shape=box];
        
        LongSequence [label="Long Sequence (5000 tokens)"];
        Window1 [label="Window 1 (Tokens 0-512)"];
        Window2 [label="Window 2 (Tokens 512-1024)"];
        WindowN [label="Window N (Tokens 4480-4992)"];
        
        Model [label="ONNX Model (Fixed Context)"];
        Mask [label="Custom Attention Mask (Band: 512)"];
        
        GlobalSummary [label="Global Summary Buffer"];
        
        LongSequence -> Window1;
        LongSequence -> Window2;
        LongSequence -> WindowN;
        
        Window1 -> Model;
        Window2 -> Model;
        WindowN -> Model;
        
        Model -> Mask [label="Applies Constraint"];
        
        // Summary Injection
        GlobalSummary -> WindowN [label="Injected as Context"];
    }
    */
}
