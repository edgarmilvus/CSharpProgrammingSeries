
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ML.Tokenizers;

namespace LocalLLM.Managers
{
    // 1. Token Estimation using Microsoft.ML.Tokenizers
    public class TokenEstimator
    {
        private readonly Tokenizer _tokenizer;

        public TokenEstimator()
        {
            // Using the GPT-2 tokenizer as a standard approximation for local models
            _tokenizer = Tokenizer.CreateGpt2Tokenizer();
        }

        public int CountTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            // CountTokens returns a TokenCount struct containing the total count
            return _tokenizer.CountTokens(text).Count;
        }
    }

    // 2. System Prompt Variants
    public enum PromptVariant { Detailed, Standard, Minimal }

    public static class SystemPrompts
    {
        // Target: ~500 tokens
        public const string Detailed = @"You are a helpful, precise, and creative AI assistant. 
        Follow these rules strictly:
        1. Always format code blocks using markdown syntax.
        2. If the user asks for factual information, provide citations if available.
        3. Maintain a polite and professional tone at all times.
        4. Example: User: 'Write a poem.' -> Assistant: 'Roses are red...'
        
        Additional Context: You are running locally on ONNX Runtime. Ensure responses are optimized for local execution.";

        // Target: ~150 tokens
        public const string Standard = @"You are a helpful AI assistant. 
        Format code in markdown blocks. 
        Be concise but accurate. 
        Maintain a polite tone.";

        // Target: ~20 tokens
        public const string Minimal = @"You are a helpful assistant.";

        public static string GetPrompt(PromptVariant variant) => variant switch
        {
            PromptVariant.Detailed => Detailed,
            PromptVariant.Standard => Standard,
            PromptVariant.Minimal => Minimal,
            _ => Minimal
        };
    }

    // 3. Context Manager with Dynamic Selection Logic
    public class ContextWindowManager
    {
        private readonly TokenEstimator _tokenEstimator;
        private readonly List<string> _history;
        private const int MaxContextTokens = 2048;

        public ContextWindowManager()
        {
            _tokenEstimator = new TokenEstimator();
            _history = new List<string>();
        }

        // Adds a message to history and returns the optimized context for the model
        public (string SystemPrompt, List<string> OptimizedHistory) GetOptimizedContext(string newMessage)
        {
            // Calculate tokens for the new message
            int newMessageTokens = _tokenEstimator.CountTokens(newMessage);

            // Determine which system prompt to use
            // We try from most detailed to least detailed to preserve quality if possible
            PromptVariant selectedVariant = PromptVariant.Detailed;
            string systemPrompt = SystemPrompts.GetPrompt(selectedVariant);
            int systemPromptTokens = _tokenEstimator.CountTokens(systemPrompt);

            // Calculate current history tokens
            int historyTokens = _history.Sum(h => _tokenEstimator.CountTokens(h));

            // Check if we fit with Detailed prompt
            if (systemPromptTokens + historyTokens + newMessageTokens > MaxContextTokens)
            {
                // Try Standard
                selectedVariant = PromptVariant.Standard;
                systemPrompt = SystemPrompts.GetPrompt(selectedVariant);
                systemPromptTokens = _tokenEstimator.CountTokens(systemPrompt);

                if (systemPromptTokens + historyTokens + newMessageTokens > MaxContextTokens)
                {
                    // Try Minimal
                    selectedVariant = PromptVariant.Minimal;
                    systemPrompt = SystemPrompts.GetPrompt(selectedVariant);
                    systemPromptTokens = _tokenEstimator.CountTokens(systemPrompt);

                    // If even Minimal + History + NewMessage exceeds limit, we must prune history
                    if (systemPromptTokens + historyTokens + newMessageTokens > MaxContextTokens)
                    {
                        // Sliding window: Remove oldest messages until we fit
                        // Note: In a real scenario, we might prioritize recent messages.
                        // Here we simply remove from the start.
                        var prunedHistory = new List<string>(_history);
                        while (prunedHistory.Count > 0 && 
                               (systemPromptTokens + 
                                prunedHistory.Sum(h => _tokenEstimator.CountTokens(h)) + 
                                newMessageTokens) > MaxContextTokens)
                        {
                            prunedHistory.RemoveAt(0);
                        }
                        return (systemPrompt, prunedHistory);
                    }
                }
            }

            // If we fit, return the current history
            return (systemPrompt, new List<string>(_history));
        }

        public void AddToHistory(string message)
        {
            _history.Add(message);
        }
    }
}
