
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

// Real-World Problem: "CodeMatch" - A Developer Knowledge-Sharing Platform
// Scenario: A large software company has thousands of code snippets and algorithms stored in a
// legacy SQL database. Developers often struggle to find existing solutions to common problems.
// For example, a developer writing a "binary search" algorithm might not know that a colleague
// already implemented a highly optimized version in a different project.
//
// Traditional SQL can only search by exact keywords (e.g., "search", "array"), which is brittle.
// We need an "Intelligent Search" that understands the *semantic meaning* of code.
// If a developer types "find item in sorted list", the system should retrieve the "binary search" code
// because the *intent* is similar, even if the words are different.
//
// This application simulates the Vector Store architecture introduced in Chapter 6.
// We will generate Vector Embeddings for code snippets, store them, and perform a Cosine Similarity search.

namespace VectorStoreDemo
{
    // 1. The Data Model (Simulating a SQL Table)
    // Represents a single code snippet in our database.
    public class CodeSnippet
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }

        // 2. The Vector Embedding (Simulating a Vector Column)
        // In a real vector database (like Pinecone), this is a high-dimensional array (e.g., 1536 dimensions).
        // Here, we use a simplified 3D vector to demonstrate the math visually.
        // This vector represents the "semantic fingerprint" of the text.
        public float[] Vector { get; set; }
    }

    public class Program
    {
        // Mock Database: A list of code snippets.
        static List<CodeSnippet> _knowledgeBase = new List<CodeSnippet>();

        public static void Main(string[] args)
        {
            Console.WriteLine("--- Intelligent Data Access: Vector Search Demo ---\n");

            // Step 1: Ingest Data & Generate Embeddings
            // In a real system, an AI model (like BERT or OpenAI Ada) generates these vectors.
            // We will simulate this process to keep the code runnable without external APIs.
            InitializeDatabase();

            // Step 2: The User Query
            // The developer searches for something semantically similar to "binary search",
            // but uses different words to describe the intent.
            string userQuery = "Algorithm to locate value in ordered collection";
            Console.WriteLine($"User Query: \"{userQuery}\"");

            // Step 3: Convert Query to Vector
            // The query must be converted into the same vector space as the database.
            float[] queryVector = SimulateEmbeddingGeneration(userQuery);
            Console.WriteLine($"Query Vector: [{string.Join(", ", queryVector)}]\n");

            // Step 4: Perform Vector Search (Cosine Similarity)
            // We iterate through the database and calculate the similarity score between
            // the query vector and every stored vector.
            CodeSnippet bestMatch = null;
            double highestSimilarity = -1.0; // Range is -1 to 1

            Console.WriteLine("--- Calculating Similarity Scores ---");
            foreach (var snippet in _knowledgeBase)
            {
                double similarity = CalculateCosineSimilarity(queryVector, snippet.Vector);
                
                // Output the comparison
                Console.WriteLine($"  Comparing Query vs [ID: {snippet.Id}, Title: {snippet.Title}]");
                Console.WriteLine($"    Score: {similarity:P2}");

                // Step 5: Retrieve Best Match
                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    bestMatch = snippet;
                }
            }

            // Step 6: Present Results
            Console.WriteLine("\n--- Search Results ---");
            if (bestMatch != null && highestSimilarity > 0.5) // Threshold check
            {
                Console.WriteLine($"Found relevant result (Score: {highestSimilarity:P2}):");
                Console.WriteLine($"  Title: {bestMatch.Title}");
                Console.WriteLine($"  Description: {bestMatch.Description}");
                Console.WriteLine($"  Code Preview:\n{bestMatch.Code}");
            }
            else
            {
                Console.WriteLine("No relevant matches found.");
            }
        }

        // --- Helper Methods ---

        // Simulates the Vector Database Architecture.
        // In production, this would be handled by EF Core providers for Vector Stores.
        static void InitializeDatabase()
        {
            // Snippet 1: Binary Search (The hidden gem we want to find)
            _knowledgeBase.Add(new CodeSnippet
            {
                Id = 101,
                Title = "Binary Search",
                Description = "Efficiently finds the position of a target value within a sorted array.",
                // Simulated Vector for: "binary search sorted array algorithm"
                Vector = new float[] { 0.9f, 0.8f, 0.1f } 
            });

            // Snippet 2: Bubble Sort (Unrelated topic)
            _knowledgeBase.Add(new CodeSnippet
            {
                Id = 102,
                Title = "Bubble Sort",
                Description = "Simple sorting algorithm that repeatedly steps through the list.",
                // Simulated Vector for: "bubble sort list simple algorithm"
                Vector = new float[] { 0.1f, 0.9f, 0.2f }
            });

            // Snippet 3: File IO (Unrelated topic)
            _knowledgeBase.Add(new CodeSnippet
            {
                Id = 103,
                Title = "File Reader",
                Description = "Reads text content from a file system.",
                // Simulated Vector for: "file read text io system"
                Vector = new float[] { 0.0f, 0.1f, 0.9f }
            });
        }

        // Simulates an AI Embedding Model.
        // Maps keywords to specific dimensions in our vector space.
        static float[] SimulateEmbeddingGeneration(string text)
        {
            float[] vector = new float[3];
            
            // Feature Extraction Logic (Simplified)
            if (text.Contains("search") || text.Contains("locate") || text.Contains("find")) vector[0] += 0.8f;
            if (text.Contains("ordered") || text.Contains("sorted") || text.Contains("array")) vector[1] += 0.8f;
            if (text.Contains("value") || text.Contains("item") || text.Contains("collection")) vector[2] += 0.2f;

            // Normalize (Simplified) to ensure vectors are comparable
            // In reality, this involves complex normalization math.
            float length = (float)Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
            if (length > 0)
            {
                vector[0] /= length;
                vector[1] /= length;
                vector[2] /= length;
            }

            return vector;
        }

        // The Core Math of Vector Search: Cosine Similarity
        // Measures the cosine of the angle between two vectors.
        // Result 1.0 = Identical meaning.
        // Result 0.0 = Unrelated.
        // Result -1.0 = Opposite meaning.
        static double CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            double dotProduct = 0.0;
            double magnitudeA = 0.0;
            double magnitudeB = 0.0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += (vecA[i] * vecB[i]);
                magnitudeA += (vecA[i] * vecA[i]);
                magnitudeB += (vecB[i] * vecB[i]);
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }
}
