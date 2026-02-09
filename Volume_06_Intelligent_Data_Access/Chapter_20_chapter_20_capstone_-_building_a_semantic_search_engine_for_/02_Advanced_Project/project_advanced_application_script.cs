
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
using System.IO;
using System.Text;

namespace SemanticSearchEngine
{
    // Simulates a Vector Database for storing and retrieving embeddings.
    // In a real scenario, this would be a connection to a service like PostgreSQL with pgvector, Qdrant, or Redis.
    public class VectorDatabase
    {
        private List<DocumentChunk> _chunks;

        public VectorDatabase()
        {
            _chunks = new List<DocumentChunk>();
        }

        // Adds a document chunk with its vector representation to the database.
        public void AddChunk(DocumentChunk chunk)
        {
            _chunks.Add(chunk);
            Console.WriteLine($"[VectorDB] Indexed chunk ID: {chunk.Id} for Doc: {chunk.DocumentId}");
        }

        // Performs a Cosine Similarity search to find the most relevant chunks.
        public List<DocumentChunk> Search(float[] queryVector, int topK)
        {
            Console.WriteLine("\n[VectorDB] Starting similarity search...");
            var results = new List<SearchResult>();

            foreach (var chunk in _chunks)
            {
                double similarity = CalculateCosineSimilarity(queryVector, chunk.Vector);
                results.Add(new SearchResult { Chunk = chunk, Score = similarity });
            }

            // Sort results by score descending (highest similarity first)
            for (int i = 0; i < results.Count - 1; i++)
            {
                for (int j = i + 1; j < results.Count; j++)
                {
                    if (results[j].Score > results[i].Score)
                    {
                        var temp = results[i];
                        results[i] = results[j];
                        results[j] = temp;
                    }
                }
            }

            // Return top K results
            var topResults = new List<DocumentChunk>();
            int limit = Math.Min(topK, results.Count);
            for (int i = 0; i < limit; i++)
            {
                topResults.Add(results[i].Chunk);
                Console.WriteLine($"  -> Match: Chunk {results[i].Chunk.Id} (Score: {results[i].Score:F4})");
            }

            return topResults;
        }

        // Helper method to calculate Cosine Similarity between two vectors.
        private double CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) throw new ArgumentException("Vector dimensions must match.");

            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += vecA[i] * vecA[i];
                normB += vecB[i] * vecB[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }

    // Represents a single segment of a document.
    public class DocumentChunk
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string Text { get; set; }
        public float[] Vector { get; set; } // The embedding representation
    }

    // Helper class for search results
    public class SearchResult
    {
        public DocumentChunk Chunk { get; set; }
        public double Score { get; set; }
    }

    // Simulates an Embedding Generator (e.g., calling OpenAI or a local model).
    // Since we cannot use real APIs here, we simulate vector generation based on text content.
    public class EmbeddingService
    {
        // Simulates generating a vector embedding.
        // In reality, this would be a heavy neural network operation.
        // Here, we map specific keywords to vector dimensions to simulate semantic meaning.
        public float[] GenerateEmbedding(string text)
        {
            // Let's assume a 5-dimensional vector space for simplicity.
            // Dimensions: [0]: Tech, [1]: Database, [2]: Code, [3]: Architecture, [4]: General
            float[] vector = new float[5];

            string lowerText = text.ToLower();

            if (lowerText.Contains("sql") || lowerText.Contains("database"))
            {
                vector[1] += 0.9f; // Strong Database signal
                vector[0] += 0.2f;
            }
            if (lowerText.Contains("c#") || lowerText.Contains("code"))
            {
                vector[2] += 0.9f; // Strong Code signal
                vector[0] += 0.2f;
            }
            if (lowerText.Contains("server") || lowerText.Contains("cloud"))
            {
                vector[3] += 0.8f; // Architecture signal
            }
            if (lowerText.Contains("tutorial") || lowerText.Contains("guide"))
            {
                vector[4] += 0.5f; // General signal
            }

            // Add some noise to simulate real-world variance
            vector[0] += 0.1f; 
            
            // Normalize slightly to ensure non-zero vectors for demo
            if (vector[0] == 0 && vector[1] == 0 && vector[2] == 0 && vector[3] == 0 && vector[4] == 0)
            {
                vector[4] = 1.0f; 
            }

            return vector;
        }
    }

    // Handles user input and orchestrates the RAG pipeline.
    public class RAGPipeline
    {
        private readonly VectorDatabase _db;
        private readonly EmbeddingService _embedder;
        private readonly MemoryCache _memoryCache;

        public RAGPipeline(VectorDatabase db, EmbeddingService embedder, MemoryCache cache)
        {
            _db = db;
            _embedder = embedder;
            _memoryCache = cache;
        }

        public void ExecuteSearch(string userQuery)
        {
            Console.WriteLine($"\n==================================================");
            Console.WriteLine($"USER QUERY: \"{userQuery}\"");
            Console.WriteLine($"==================================================");

            // 1. Check Memory Cache
            string cachedResponse = _memoryCache.Get(userQuery);
            if (cachedResponse != null)
            {
                Console.WriteLine("[RAG Pipeline] Result found in Memory Cache!");
                Console.WriteLine($"[RAG Pipeline] Cached Answer: {cachedResponse}");
                return;
            }

            // 2. Generate Query Embedding
            float[] queryVector = _embedder.GenerateEmbedding(userQuery);

            // 3. Retrieve Relevant Context (Semantic Search)
            List<DocumentChunk> relevantChunks = _db.Search(queryVector, topK: 2);

            if (relevantChunks.Count == 0)
            {
                Console.WriteLine("[RAG Pipeline] No relevant documentation found.");
                return;
            }

            // 4. Construct Context for LLM (Simulated)
            StringBuilder contextBuilder = new StringBuilder();
            foreach (var chunk in relevantChunks)
            {
                contextBuilder.AppendLine($"- {chunk.Text}");
            }

            // 5. Simulate LLM Generation (RAG Step)
            string generatedAnswer = GenerateAnswerFromContext(userQuery, contextBuilder.ToString());
            
            // 6. Store in Cache for future queries
            _memoryCache.Set(userQuery, generatedAnswer);

            Console.WriteLine($"\n[RAG Pipeline] Generated Answer:\n{generatedAnswer}");
        }

        // Simple logic to simulate an LLM generating an answer based on retrieved chunks.
        private string GenerateAnswerFromContext(string query, string context)
        {
            if (context.Contains("SQL") && context.Contains("EF Core"))
            {
                return "Based on the documentation, EF Core supports raw SQL queries using `FromSqlRaw` while maintaining LINQ capabilities for standard operations.";
            }
            if (context.Contains("Server") && context.Contains("Cloud"))
            {
                return "The documentation suggests deploying the application to Azure App Service for cloud scalability.";
            }
            return "I found some relevant documentation, but the specific answer requires further synthesis.";
        }
    }

    // Local Memory Storage to cache query results.
    public class MemoryCache
    {
        // Using a Dictionary for O(1) lookups.
        private Dictionary<string, string> _cache;

        public MemoryCache()
        {
            _cache = new Dictionary<string, string>();
        }

        public string Get(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }
            return null;
        }

        public void Set(string key, string value)
        {
            // In a real app, we would implement eviction policies (LRU, TTL).
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = value;
                Console.WriteLine($"[MemoryCache] Stored new entry for: \"{key}\"");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialize Services
            var db = new VectorDatabase();
            var embedder = new EmbeddingService();
            var cache = new MemoryCache();
            var pipeline = new RAGPipeline(db, embedder, cache);

            // 2. Ingest Documentation (Simulating EF Core / SQL Docs)
            // In a real app, this would read .md or .pdf files.
            var doc1 = new DocumentChunk { Id = 1, DocumentId = 101, Text = "EF Core is an object-relational mapper for .NET. It supports LINQ queries." };
            doc1.Vector = embedder.GenerateEmbedding(doc1.Text);
            db.AddChunk(doc1);

            var doc2 = new DocumentChunk { Id = 2, DocumentId = 101, Text = "To execute raw SQL, use the FromSqlRaw method on DbSet." };
            doc2.Vector = embedder.GenerateEmbedding(doc2.Text);
            db.AddChunk(doc2);

            var doc3 = new DocumentChunk { Id = 3, DocumentId = 102, Text = "Deploying to Azure Cloud requires configuring the server resources." };
            doc3.Vector = embedder.GenerateEmbedding(doc3.Text);
            db.AddChunk(doc3);

            // 3. Execute Queries
            // First query: Will hit DB
            pipeline.ExecuteSearch("How do I run raw SQL in EF Core?");

            // Second query: Exact match, will hit Cache
            pipeline.ExecuteSearch("How do I run raw SQL in EF Core?");

            // Third query: Different context, will hit DB
            pipeline.ExecuteSearch("Where should I host this app?");
        }
    }
}
