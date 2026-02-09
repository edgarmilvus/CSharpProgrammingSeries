
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

namespace OfflineRAGApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. SETUP: Define the knowledge base (local directory) and initialize the pipeline.
            // In a real scenario, this would be a folder with .txt, .pdf, or .md files.
            string knowledgeBasePath = Path.Combine(Directory.GetCurrentDirectory(), "KnowledgeBase");
            Directory.CreateDirectory(knowledgeBasePath); // Ensure directory exists for the example.

            // For this simulation, we will create a dummy file if it doesn't exist to demonstrate the logic.
            string sampleFilePath = Path.Combine(knowledgeBasePath, "Project_Alpha_Notes.txt");
            if (!File.Exists(sampleFilePath))
            {
                File.WriteAllText(sampleFilePath, 
                    "Project Alpha uses a microservices architecture. " +
                    "The primary database is PostgreSQL version 15. " +
                    "Authentication is handled via OAuth2. " +
                    "Deployment occurs on Tuesdays at 2:00 AM UTC.");
            }

            Console.WriteLine("=== Offline RAG: Local Document Query ===");
            Console.WriteLine($"Scanning directory: {knowledgeBasePath}\n");

            // 2. INGESTION & CHUNKING: Read files and split text into manageable chunks.
            // We use a simple sentence-based chunking strategy for readability.
            List<TextChunk> allChunks = new List<TextChunk>();
            string[] files = Directory.GetFiles(knowledgeBasePath, "*.txt");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                string[] sentences = content.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < sentences.Length; i++)
                {
                    // Create a chunk for each sentence. In production, overlapping windows are used.
                    TextChunk chunk = new TextChunk
                    {
                        Id = $"{Path.GetFileName(file)}_chunk_{i}",
                        Content = sentences[i].Trim(),
                        SourceFile = file
                    };
                    allChunks.Add(chunk);
                }
            }

            Console.WriteLine($"Ingested {allChunks.Count} text chunks from {files.Length} files.\n");

            // 3. EMBEDDING GENERATION (SIMULATED): Convert text to vector representations.
            // Since we cannot load a heavy ONNX model here, we simulate vector generation 
            // using a frequency-based approach (Bag of Words) for semantic matching.
            Console.WriteLine("Generating local embeddings (Simulated)...");
            VectorDatabase db = new VectorDatabase();
            
            foreach (var chunk in allChunks)
            {
                float[] vector = GenerateEmbedding(chunk.Content);
                db.AddChunk(chunk, vector);
            }

            // 4. QUERY PROCESSING: Take user input and generate a query vector.
            Console.WriteLine("\nEnter your query (e.g., 'What database is used?'):");
            string userQuery = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userQuery))
            {
                userQuery = "What database is used?"; // Default fallback
                Console.WriteLine($"Using default query: {userQuery}");
            }

            // Generate embedding for the query using the same method
            float[] queryVector = GenerateEmbedding(userQuery);

            // 5. SEMANTIC SEARCH: Retrieve relevant chunks based on vector similarity.
            // We calculate Cosine Similarity to find the best matches.
            Console.WriteLine("\nSearching local vector database...");
            List<RetrievalResult> retrievedDocs = db.Search(queryVector, topK: 3);

            // 6. CONTEXT CONSTRUCTION: Build the dynamic prompt for the LLM.
            StringBuilder contextBuilder = new StringBuilder();
            if (retrievedDocs.Count > 0)
            {
                contextBuilder.AppendLine("Relevant Context:");
                foreach (var result in retrievedDocs)
                {
                    contextBuilder.AppendLine($"- {result.Chunk.Content} (Source: {Path.GetFileName(result.Chunk.SourceFile)})");
                }
            }
            else
            {
                contextBuilder.AppendLine("No relevant context found in local files.");
            }

            // 7. LOCAL INFERENCE (SIMULATED): Generate the final answer using the context.
            // In a real app, this would call the ONNX LLM (e.g., Phi-3) via ML.NET.
            Console.WriteLine("\n--- Local Inference (ONNX LLM Simulation) ---");
            Console.WriteLine($"User Query: {userQuery}");
            Console.WriteLine(contextBuilder.ToString());
            
            string finalAnswer = SimulateLocalLLM(userQuery, contextBuilder.ToString());
            Console.WriteLine($"\nGenerated Answer: {finalAnswer}");
        }

        // --- Helper Methods ---

        /// <summary>
        /// Simulates embedding generation by converting text into a frequency vector.
        /// In a real scenario, this would run the ONNX embedding model.
        /// </summary>
        static float[] GenerateEmbedding(string text)
        {
            // Simple tokenizer: split by space and punctuation
            string[] tokens = text.ToLower().Split(new[] { ' ', '.', ',', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Create a fixed-size vector (e.g., 10 dimensions for simulation)
            float[] vector = new float[10];
            
            // Hash-based mapping to simulate vector dimensions
            foreach (string token in tokens)
            {
                int index = Math.Abs(token.GetHashCode()) % 10;
                vector[index] += 1.0f; // Increment frequency
            }

            // Normalize (L2 Norm)
            float magnitude = 0;
            foreach (float val in vector) magnitude += val * val;
            magnitude = (float)Math.Sqrt(magnitude);
            
            if (magnitude > 0)
            {
                for (int i = 0; i < vector.Length; i++) vector[i] /= magnitude;
            }

            return vector;
        }

        /// <summary>
        /// Simulates the response generation of a local ONNX LLM (like Phi-3).
        /// </summary>
        static string SimulateLocalLLM(string query, string context)
        {
            // Rule-based fallback to demonstrate the logic without a heavy model
            if (context.Contains("PostgreSQL"))
            {
                return "Based on the retrieved documents, Project Alpha uses PostgreSQL version 15.";
            }
            if (context.Contains("OAuth2"))
            {
                return "The documents indicate that authentication is handled via OAuth2.";
            }
            return "I could not find a specific answer in the local files.";
        }
    }

    // --- Data Structures ---

    public class TextChunk
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string SourceFile { get; set; }
    }

    public class RetrievalResult
    {
        public TextChunk Chunk { get; set; }
        public float Score { get; set; }
    }

    /// <summary>
    /// A simple in-memory vector store.
    /// </summary>
    public class VectorDatabase
    {
        private List<(TextChunk Chunk, float[] Vector)> _storage = new List<(TextChunk, float[])>();

        public void AddChunk(TextChunk chunk, float[] vector)
        {
            _storage.Add((chunk, vector));
        }

        public List<RetrievalResult> Search(float[] queryVector, int topK)
        {
            var results = new List<RetrievalResult>();

            foreach (var item in _storage)
            {
                // Calculate Cosine Similarity
                float dotProduct = 0;
                float queryMagnitude = 0;
                float itemMagnitude = 0;

                for (int i = 0; i < queryVector.Length; i++)
                {
                    dotProduct += queryVector[i] * item.Vector[i];
                    queryMagnitude += queryVector[i] * queryVector[i];
                    itemMagnitude += item.Vector[i] * item.Vector[i];
                }

                float similarity = dotProduct / ((float)Math.Sqrt(queryMagnitude) * (float)Math.Sqrt(itemMagnitude));

                // Filter out low scores (threshold)
                if (similarity > 0.1)
                {
                    results.Add(new RetrievalResult { Chunk = item.Chunk, Score = similarity });
                }
            }

            // Sort by score descending
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

            // Return top K
            if (results.Count > topK)
            {
                results.RemoveRange(topK, results.Count - topK);
            }

            return results;
        }
    }
}
