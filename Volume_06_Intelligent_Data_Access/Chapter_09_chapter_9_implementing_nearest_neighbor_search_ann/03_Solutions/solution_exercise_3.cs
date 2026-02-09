
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chapter9.Exercise3
{
    // 1. Extended Data Model
    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    // 2. RagOrchestrator
    public class RagOrchestrator
    {
        private readonly List<Document> _database; // Simulating DB

        public RagOrchestrator(List<Document> database)
        {
            _database = database;
        }

        public async Task<List<Document>> SearchAsync(string textQuery, float[] queryVector, string? categoryFilter = null, DateTime? dateFilter = null)
        {
            // 4. Filtering: Apply metadata filters first
            var candidates = _database.AsQueryable();
            
            if (!string.IsNullOrEmpty(categoryFilter))
                candidates = candidates.Where(d => d.Category == categoryFilter);
            
            if (dateFilter.HasValue)
                candidates = candidates.Where(d => d.PublishDate >= dateFilter.Value);

            var candidateList = candidates.ToList();

            // 2. Vector Search Simulation (Top 20)
            // Assuming Cosine Similarity (1.0 is best)
            var vectorResults = candidateList
                .Select(d => new { Doc = d, Score = CalculateCosine(d.Embedding, queryVector) })
                .OrderByDescending(x => x.Score)
                .Take(20)
                .ToList();

            // 2. Full-Text Search Simulation (Top 20)
            // Simple keyword matching simulation
            var keywords = textQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var textResults = candidateList
                .Select(d => new { 
                    Doc = d, 
                    Score = keywords.Sum(k => (d.Title.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0) + 
                                             (d.Content.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(20)
                .ToList();

            // 3. Ranking Algorithm: Reciprocal Rank Fusion (RRF)
            // Formula: Score = 1 / (k + rank)
            const int k = 60; // Constant usually between 60-100

            var rrfScores = new Dictionary<Guid, double>();

            // Process Vector Ranks
            for (int i = 0; i < vectorResults.Count; i++)
            {
                var docId = vectorResults[i].Doc.Id;
                double score = 1.0 / (k + (i + 1)); // Rank is 1-based
                if (!rrfScores.ContainsKey(docId)) rrfScores[docId] = 0;
                rrfScores[docId] += score;
            }

            // Process Text Ranks
            for (int i = 0; i < textResults.Count; i++)
            {
                var docId = textResults[i].Doc.Id;
                double score = 1.0 / (k + (i + 1));
                if (!rrfScores.ContainsKey(docId)) rrfScores[docId] = 0;
                rrfScores[docId] += score;
            }

            // Get Top 5 based on RRF
            var topIds = rrfScores.OrderByDescending(x => x.Value).Take(5).Select(x => x.Key).ToList();
            var finalResults = candidateList.Where(d => topIds.Contains(d.Id)).ToList();

            // Interactive Challenge: Re-ranking
            // Simulate Cross-Encoder: A model that looks at the query and the document together.
            // Here we mock it by boosting score if both text match AND vector similarity is high.
            finalResults = await ReRankAsync(textQuery, finalResults, queryVector);

            return finalResults;
        }

        private Task<List<Document>> ReRankAsync(string query, List<Document> docs, float[] queryVector)
        {
            // Mock Cross-Encoder logic
            var ranked = docs.Select(d => {
                // Mock score: 70% vector, 30% keyword overlap
                double vectorScore = CalculateCosine(d.Embedding, queryVector);
                int keywordMatches = query.Split(' ').Count(k => d.Content.Contains(k));
                double finalScore = (vectorScore * 0.7) + (keywordMatches * 0.3);
                return new { Doc = d, Score = finalScore };
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Doc)
            .ToList();

            return Task.FromResult(ranked);
        }

        private double CalculateCosine(float[] a, float[] b)
        {
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++) {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }
    }
}
