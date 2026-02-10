
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

// Simulating a Vector Database (like Pinecone or Weaviate) in-memory for this "Hello World" example.
// In a real application, you would replace this class with an actual client SDK.
public class VectorStore
{
    // The core data structure: A dictionary mapping an ID to its vector representation.
    // In production, this would be a distributed, optimized index (e.g., HNSW graph).
    private readonly Dictionary<string, float[]> _index = new();

    // Inserts a vector into the store.
    public void Upsert(string id, float[] vector)
    {
        _index[id] = vector;
    }

    // Performs a similarity search using Cosine Similarity.
    // Returns the top 'k' most similar vectors.
    public IEnumerable<(string Id, float Similarity)> Search(float[] queryVector, int topK = 3)
    {
        return _index
            .Select(kvp => (
                Id: kvp.Key,
                Similarity: CosineSimilarity(queryVector, kvp.Value)
            ))
            .OrderByDescending(x => x.Similarity)
            .Take(topK);
    }

    // Helper method to calculate Cosine Similarity (range: -1 to 1, where 1 is identical).
    private static float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be of the same dimension.");

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = MathF.Sqrt(magnitudeA);
        magnitudeB = MathF.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0) return 0;
        
        return dotProduct / (magnitudeA * magnitudeB);
    }
}

// Simulating an Embedding Service (like OpenAI or Azure AI).
// This converts raw text into a high-dimensional vector.
public static class EmbeddingService
{
    // In a real scenario, this calls a remote API.
    // Here, we simulate it by generating deterministic vectors based on text length and characters
    // to ensure the example is reproducible and demonstrates the concept.
    public static float[] GenerateEmbedding(string text)
    {
        const int dimensions = 128; // Standard embedding size (e.g., OpenAI's text-embedding-ada-002 is 1536)
        var vector = new float[dimensions];
        
        // Simple deterministic hash-like generation for demonstration
        for (int i = 0; i < dimensions; i++)
        {
            // Modulate based on character code and index to create distinct patterns
            vector[i] = (text.Length % 10) + (i % 5) + (text.GetHashCode() % 100) / 1000f;
        }
        
        return vector;
    }
}

class Program
{
    static void Main()
    {
        // 1. Setup: Initialize our vector store.
        var vectorDb = new VectorStore();

        // 2. Data: Define a list of documents (unstructured data) we want to search through.
        var documents = new List<(string Id, string Content)>
        {
            ("doc_1", "The quick brown fox jumps over the lazy dog."),
            ("doc_2", "A quick brown dog races past a sleeping fox."),
            ("doc_3", "The weather today is sunny and warm."),
            ("doc_4", "I love programming in C# and .NET."),
            ("doc_5", "Machine learning involves training models on data.")
        };

        Console.WriteLine("--- Indexing Documents ---");

        // 3. Indexing: Convert text to vectors and store them.
        foreach (var doc in documents)
        {
            var embedding = EmbeddingService.GenerateEmbedding(doc.Content);
            vectorDb.Upsert(doc.Id, embedding);
            Console.WriteLine($"Indexed {doc.Id}: \"{doc.Content}\"");
        }

        Console.WriteLine("\n--- Performing Search ---");

        // 4. Querying: Define a search query.
        string query = "fast brown animal jumps";
        Console.WriteLine($"Query: \"{query}\"");

        // 5. Search: Convert query to vector and perform similarity search.
        var queryVector = EmbeddingService.GenerateEmbedding(query);
        var results = vectorDb.Search(queryVector, topK: 2);

        // 6. Results: Display the matches.
        Console.WriteLine("\nTop Matches:");
        foreach (var result in results)
        {
            // Find the original text for display purposes
            var originalDoc = documents.First(d => d.Id == result.Id);
            Console.WriteLine($"- ID: {result.Id} | Similarity: {result.Similarity:F4}");
            Console.WriteLine($"  Content: \"{originalDoc.Content}\"");
        }
    }
}
