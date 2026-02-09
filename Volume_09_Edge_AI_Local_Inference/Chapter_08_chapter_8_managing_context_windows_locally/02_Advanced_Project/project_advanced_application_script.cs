
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalLLMContextManager
{
    /// <summary>
    /// Represents a single message in the conversation history.
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } // "User", "Assistant", "System"
        public string Content { get; set; }
        public int TokenCount { get; set; }
        public float[] Embedding { get; set; } // Semantic vector representation
    }

    /// <summary>
    /// Core engine for managing context windows using sliding window and semantic pruning.
    /// </summary>
    public class ContextWindowManager
    {
        private readonly int _maxTokenBudget;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly int _embeddingDimension = 5; // Simplified for demo (usually 384 or 768)

        public ContextWindowManager(int maxTokenBudget)
        {
            _maxTokenBudget = maxTokenBudget;
            _conversationHistory = new List<ChatMessage>();
        }

        /// <summary>
        /// Adds a message, calculates its "cost" (tokens), and prunes if necessary.
        /// </summary>
        public void AddMessage(string role, string content)
        {
            // 1. Simulate Tokenization (Basic heuristic: 1 word ~ 1.3 tokens)
            int tokenCount = (int)(content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 1.3);
            
            // 2. Generate Semantic Embedding (Simulated for this demo)
            float[] embedding = GenerateSimulatedEmbedding(content);

            var message = new ChatMessage
            {
                Role = role,
                Content = content,
                TokenCount = tokenCount,
                Embedding = embedding
            };

            _conversationHistory.Add(message);
            Console.WriteLine($"[Added] {role}: '{content.Substring(0, Math.Min(30, content.Length))}...' (Cost: {tokenCount} tokens)");

            // 3. Check Budget and Prune
            ManageContextWindow();
        }

        /// <summary>
        /// Implements the Sliding Window + Semantic Pruning logic.
        /// </summary>
        private void ManageContextWindow()
        {
            int currentTokens = CalculateTotalTokens();
            
            // If we are within budget, do nothing.
            if (currentTokens <= _maxTokenBudget) return;

            Console.WriteLine($"[Alert] Budget Exceeded! Current: {currentTokens}. Pruning required...");

            // STRATEGY 1: Sliding Window (Keep recent messages)
            // We define a "Safety Buffer" of recent messages that are NEVER pruned to maintain immediate coherence.
            int safetyBufferCount = 2; 
            int indexToStartPruningFrom = 0;

            if (_conversationHistory.Count > safetyBufferCount)
            {
                indexToStartPruningFrom = _conversationHistory.Count - safetyBufferCount;
            }

            // STRATEGY 2: Semantic Pruning (Remove least relevant historical messages)
            // We calculate the "Semantic Distance" of older messages from the most recent user query.
            // If a message is semantically redundant or irrelevant to the current context, it is a candidate for removal.
            
            // Identify the most recent user message as the "Anchor" for relevance.
            ChatMessage anchorMessage = GetLastUserMessage();
            if (anchorMessage == null) return;

            List<int> indicesToRemove = new List<int>();

            // Iterate through older messages (excluding the safety buffer)
            for (int i = 0; i < indexToStartPruningFrom; i++)
            {
                if (_conversationHistory[i] == null) continue;

                // Calculate Cosine Similarity (Simplified Dot Product for demo)
                float similarity = CalculateCosineSimilarity(_conversationHistory[i].Embedding, anchorMessage.Embedding);

                // If similarity is low (dissimilar) or very high (redundant), mark for removal.
                // Thresholds are tunable.
                if (similarity < 0.2f || similarity > 0.95f)
                {
                    indicesToRemove.Add(i);
                }
            }

            // Remove marked messages (in reverse order to avoid index shifting issues)
            // We sort descending to remove from end to start
            indicesToRemove.Sort((a, b) => b.CompareTo(a));

            int removedTokenCount = 0;
            foreach (int index in indicesToRemove)
            {
                removedTokenCount += _conversationHistory[index].TokenCount;
                Console.WriteLine($"[Pruned] Removing irrelevant message: '{_conversationHistory[index].Content.Substring(0, 20)}...'");
                _conversationHistory.RemoveAt(index);
            }

            // STRATEGY 3: Hard Truncation (Fallback)
            // If pruning semantically relevant messages didn't free up enough space, 
            // we perform a hard cut on the oldest messages.
            currentTokens = CalculateTotalTokens();
            if (currentTokens > _maxTokenBudget)
            {
                Console.WriteLine("[Warning] Semantic pruning insufficient. Applying hard truncation...");
                int index = 0;
                while (currentTokens > _maxTokenBudget && index < _conversationHistory.Count)
                {
                    currentTokens -= _conversationHistory[index].TokenCount;
                    Console.WriteLine($"[Truncated] Removing oldest: '{_conversationHistory[index].Content.Substring(0, 20)}...'");
                    _conversationHistory.RemoveAt(index);
                    // Do not increment index, as removal shifts the list left
                }
            }

            Console.WriteLine($"[Done] Context stabilized. Remaining tokens: {CalculateTotalTokens()}");
        }

        // --- Helper Methods (Simulation of Complex Logic) ---

        private int CalculateTotalTokens()
        {
            int sum = 0;
            foreach (var msg in _conversationHistory)
            {
                sum += msg.TokenCount;
            }
            return sum;
        }

        private ChatMessage GetLastUserMessage()
        {
            for (int i = _conversationHistory.Count - 1; i >= 0; i--)
            {
                if (_conversationHistory[i].Role == "User") return _conversationHistory[i];
            }
            return null;
        }

        private float[] GenerateSimulatedEmbedding(string text)
        {
            // In a real ONNX scenario, we would run the text through an embedding model (e.g., All-MiniLM-L6-v2).
            // Here, we generate a deterministic vector based on character codes to simulate semantic representation.
            float[] vector = new float[_embeddingDimension];
            float sum = 0;
            for (int i = 0; i < _embeddingDimension; i++)
            {
                // Create a pseudo-random but deterministic vector based on the text
                vector[i] = (text.Length * (i + 1) * 0.123f) % 1.0f;
                sum += vector[i] * vector[i];
            }

            // Normalize (L2 Normalization)
            float magnitude = (float)Math.Sqrt(sum);
            for (int i = 0; i < _embeddingDimension; i++)
            {
                vector[i] /= magnitude;
            }
            return vector;
        }

        private float CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0f;
            
            float dotProduct = 0f;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
            }
            return dotProduct; // Vectors are normalized, so dot product = cosine similarity
        }

        public void PrintCurrentContext()
        {
            Console.WriteLine("\n--- Current Context Window ---");
            foreach (var msg in _conversationHistory)
            {
                Console.WriteLine($"[{msg.Role}]: {msg.Content} ({msg.TokenCount} tokens)");
            }
            Console.WriteLine($"Total Tokens: {CalculateTotalTokens()}/{_maxTokenBudget}");
            Console.WriteLine("------------------------------\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize with a strict budget (e.g., simulating a GPU with limited VRAM)
            // We set it low to force pruning logic quickly for demonstration.
            var manager = new ContextWindowManager(maxTokenBudget: 60);

            // 1. Initial Greeting (Low cost)
            manager.AddMessage("System", "You are a helpful coding assistant.");
            
            // 2. User asks a specific coding question
            manager.AddMessage("User", "How do I reverse a string in C#?");
            
            // 3. Assistant responds
            manager.AddMessage("Assistant", "You can use Array.Reverse or a loop. Example: new string(input.Reverse().ToArray());");
            
            // 4. User asks a follow-up (Context is growing)
            manager.AddMessage("User", "Can you explain how the memory management works in that approach?");
            
            // 5. User asks an OFF-TOPIC question (This is the target for semantic pruning)
            // This message is semantically distant from the coding topic.
            manager.AddMessage("User", "By the way, what is the capital of France?");
            
            // 6. Assistant answers the off-topic question
            manager.AddMessage("Assistant", "The capital of France is Paris.");
            
            // 7. User returns to the coding topic (Triggering Pruning)
            // This message is semantically similar to message #2, making the "France" message irrelevant.
            manager.AddMessage("User", "Back to C#, is StringBuilder more efficient for large concatenations?");

            // 8. Display final state
            manager.PrintCurrentContext();
        }
    }
}
