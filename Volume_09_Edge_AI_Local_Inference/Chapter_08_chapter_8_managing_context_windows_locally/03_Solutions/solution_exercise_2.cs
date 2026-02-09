
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalLLM.Managers
{
    // 1. Embedding Service (Mock implementation for structure, requires ONNX model file)
    public class EmbeddingService : IDisposable
    {
        private readonly InferenceSession _session;
        
        public EmbeddingService(string modelPath)
        {
            // Load a local ONNX embedding model (e.g., BERT base)
            var options = new SessionOptions();
            // In a real scenario, you might use GPU execution provider
            _session = new InferenceSession(modelPath, options);
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            // In a real implementation, you would tokenize the text here
            // For this exercise, we simulate the output shape of a standard embedding model (e.g., 384 or 768 dimensions)
            // This allows the logic to function without needing the actual model file.
            
            // SIMULATION START
            await Task.Delay(10); // Simulate async processing
            var random = new Random(text.GetHashCode()); // Deterministic random based on text
            var vector = new float[768];
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)random.NextDouble();
            }
            // Normalize for cosine similarity
            var magnitude = (float)Math.Sqrt(vector.Sum(x => x * x));
            for (int i = 0; i < vector.Length; i++) vector[i] /= magnitude;
            // SIMULATION END
            
            return vector;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }

    // 2. Similarity Calculation
    public static class VectorMath
    {
        public static float CosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) throw new ArgumentException("Vectors must be same length");
            float dotProduct = 0f;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
            }
            // Note: Vectors are assumed normalized, otherwise divide by magnitudes
            return dotProduct; 
        }
    }

    public class HistoryItem
    {
        public string Message { get; set; }
        public float[] Embedding { get; set; }
        public int TokenCount { get; set; }
    }

    // 3. Semantic Context Manager
    public class SemanticContextManager
    {
        private readonly EmbeddingService _embeddingService;
        private readonly TokenEstimator _tokenEstimator;
        private readonly List<HistoryItem> _history;
        private const int MaxContextTokens = 2048;

        public SemanticContextManager(EmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
            _tokenEstimator = new TokenEstimator();
            _history = new List<HistoryItem>();
        }

        public async Task AddMessageAsync(string message)
        {
            var embedding = await _embeddingService.GetEmbeddingAsync(message);
            var tokens = _tokenEstimator.CountTokens(message);
            _history.Add(new HistoryItem { Message = message, Embedding = embedding, TokenCount = tokens });
        }

        // 4. Integration: Pruning based on similarity
        public async Task<(string PrunedContext, int RemainingTokens)> GetOptimizedContext(string newQuery)
        {
            // Get embedding for the new query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(newQuery);
            int newQueryTokens = _tokenEstimator.CountTokens(newQuery);

            // Calculate similarity scores
            // We create a list of tuples (Item, Score) to sort
            var scoredHistory = _history
                .Select(h => new 
                { 
                    Item = h, 
                    Score = VectorMath.CosineSimilarity(queryEmbedding, h.Embedding) 
                })
                .OrderBy(x => x.Score) // Sort by lowest similarity (least relevant first)
                .ToList();

            var keptItems = new List<HistoryItem>();
            int currentTokenCount = newQueryTokens;

            // Edge Case: Never prune the immediate previous message (N-1)
            // We identify the last item (if exists) and treat it as "protected"
            HistoryItem lastItem = _history.LastOrDefault();
            
            // Start with the protected item if it exists (but don't double count if it's also in sorted list)
            if (lastItem != null)
            {
                keptItems.Add(lastItem);
                currentTokenCount += lastItem.TokenCount;
            }

            // Iterate through the sorted list (least relevant first)
            foreach (var scoredItem in scoredHistory)
            {
                // Skip the last item if it was already added
                if (lastItem != null && scoredItem.Item == lastItem) continue;

                // Check if adding this item exceeds budget
                if (currentTokenCount + scoredItem.Item.TokenCount <= MaxContextTokens)
                {
                    keptItems.Add(scoredItem.Item);
                    currentTokenCount += scoredItem.Item.TokenCount;
                }
                else
                {
                    // Prune this item (do not add)
                    continue;
                }
            }

            // Reconstruct context string (preserving original order)
            // We need to sort keptItems by their original appearance in _history
            var orderedContext = keptItems
                .OrderBy(x => _history.IndexOf(x))
                .Select(x => x.Message)
                .ToList();

            string contextString = string.Join("\n", orderedContext);
            return (contextString, MaxContextTokens - currentTokenCount);
        }
    }
    
    // 5. Visualization (Graphviz DOT representation in comments)
    /*
    digraph G {
        rankdir=LR;
        node [shape=box, style=rounded];
        
        RawHistory [label="Raw Text History"];
        Embeddings [label="Vector Embeddings (ONNX)"];
        Query [label="Current Query"];
        Similarity [label="Cosine Similarity Calc"];
        Pruning [label="Sort & Prune (Lowest Score)"];
        FinalContext [label="Optimized Context String"];
        
        RawHistory -> Embeddings;
        Query -> Similarity;
        Embeddings -> Similarity;
        Similarity -> Pruning;
        Pruning -> FinalContext;
    }
    */
}
