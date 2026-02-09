
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LocalContextManagement
{
    // Represents a single message in the conversation history
    public record ConversationMessage(string Role, string Content, float[] Embedding);

    public class ContextWindowManager
    {
        private readonly int _maxTokenBudget;
        private readonly List<ConversationMessage> _history = new();

        // In a real scenario, we would use a dedicated embedding model (e.g., All-MiniLM-L6-v2)
        // For this example, we simulate embeddings as simple numeric vectors.
        public ContextWindowManager(int maxTokenBudget)
        {
            _maxTokenBudget = maxTokenBudget;
        }

        public void AddMessage(string role, string content)
        {
            // Simulate generating an embedding for the content
            // In reality, this would be an ONNX inference call
            var embedding = GenerateMockEmbedding(content);
            
            var message = new ConversationMessage(role, content, embedding);
            _history.Add(message);
        }

        // Core logic: Prune history based on token budget and semantic relevance
        public List<ConversationMessage> GetOptimizedContext(string currentQuery)
        {
            Console.WriteLine($"[System] Current History Size: {_history.Count} messages");
            
            // 1. Calculate approximate tokens (1 token ~= 4 chars for English text)
            int currentTokens = _history.Sum(m => m.Content.Length / 4);
            
            if (currentTokens <= _maxTokenBudget)
            {
                Console.WriteLine("[System] Context fits within budget. Returning full history.");
                return _history;
            }

            Console.WriteLine($"[System] Context exceeds budget ({currentTokens} > {_maxTokenBudget}). Pruning...");

            // 2. Generate embedding for the current query to find relevance
            var queryEmbedding = GenerateMockEmbedding(currentQuery);

            // 3. Score messages by relevance to the current query (Cosine Similarity)
            var scoredHistory = _history
                .Select(msg => new 
                { 
                    Message = msg, 
                    Score = CalculateCosineSimilarity(queryEmbedding, msg.Embedding) 
                })
                .OrderByDescending(x => x.Score) // Keep most relevant
                .ToList();

            // 4. Sliding Window: Reconstruct context until budget is met
            var optimizedContext = new List<ConversationMessage>();
            int accumulatedTokens = 0;

            foreach (var item in scoredHistory)
            {
                int msgTokens = item.Message.Content.Length / 4;
                
                if (accumulatedTokens + msgTokens <= _maxTokenBudget)
                {
                    optimizedContext.Add(item.Message);
                    accumulatedTokens += msgTokens;
                }
                else
                {
                    // We stop adding once the budget is full, prioritizing by score
                    break;
                }
            }

            // 5. Sort by original chronological order for the LLM to understand flow
            // (Optional, but usually preferred for chat continuity)
            var finalContext = optimizedContext
                .OrderBy(m => _history.IndexOf(m))
                .ToList();

            Console.WriteLine($"[System] Pruned context size: {finalContext.Count} messages. Estimated tokens: {accumulatedTokens}");
            
            return finalContext;
        }

        // --- Helper Methods ---

        // Simulates an embedding vector (e.g., 384 dimensions)
        private float[] GenerateMockEmbedding(string text)
        {
            var rnd = new Random(text.GetHashCode()); // Deterministic based on text
            var vector = new float[384];
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)rnd.NextDouble();
            }
            return vector;
        }

        // Calculates Cosine Similarity between two vectors (0 to 1, where 1 is identical)
        private float CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0;

            float dotProduct = 0;
            float magnitudeA = 0;
            float magnitudeB = 0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                magnitudeA += vecA[i] * vecA[i];
                magnitudeB += vecB[i] * vecB[i];
            }

            magnitudeA = (float)Math.Sqrt(magnitudeA);
            magnitudeB = (float)Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;
            return dotProduct / (magnitudeA * magnitudeB);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Simulate a constrained environment (e.g., 512 tokens)
            var contextManager = new ContextWindowManager(maxTokenBudget: 512);

            // 1. Populate history with various topics
            // Note: We add enough data to exceed the token budget
            contextManager.AddMessage("system", "You are a helpful assistant specialized in C# and AI.");
            contextManager.AddMessage("user", "What is the capital of France?");
            contextManager.AddMessage("assistant", "The capital of France is Paris.");
            
            // Add a long technical discussion to fill the context
            contextManager.AddMessage("user", "Can you explain how sliding window attention works in ONNX Runtime?");
            contextManager.AddMessage("assistant", "Sliding window attention restricts the attention mechanism to a fixed-size window of previous tokens, reducing memory complexity from quadratic to linear.");
            
            // Add noise/unrelated history
            contextManager.AddMessage("user", "What did I have for breakfast?");
            contextManager.AddMessage("assistant", "I don't have access to your personal data unless you tell me.");
            
            contextManager.AddMessage("user", "Tell me more about the linear complexity part.");
            contextManager.AddMessage("assistant", "In standard attention, every token attends to every other token. With a window size W, each token only attends to W neighbors, significantly lowering VRAM usage.");

            // 2. Simulate a new user query
            string newQuery = "How does sliding window attention affect the context length in ONNX?";
            
            Console.WriteLine($"\n--- Processing Query: \"{newQuery}\" ---\n");

            // 3. Retrieve optimized context
            var context = contextManager.GetOptimizedContext(newQuery);

            // 4. Simulate passing to the LLM (Local Inference)
            Console.WriteLine("\n--- Final Context Sent to ONNX Model ---");
            foreach (var msg in context)
            {
                Console.WriteLine($"[{msg.Role.ToUpper()}]: {msg.Content}");
            }
        }
    }
}
