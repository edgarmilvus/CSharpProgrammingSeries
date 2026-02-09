
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

// NuGet Packages Required:
// 1. Microsoft.EntityFrameworkCore.Sqlite
// 2. Microsoft.EntityFrameworkCore.Sqlite.Vss (Community fork or equivalent wrapper for Vector Search)
//    Note: For this example, we will mock the vector search logic if the extension isn't available,
//    but the code below assumes a standard EF Core setup with a hypothetical Vector property.

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticSearchHelloWorld
{
    // 1. Define the Entity
    // Represents a chunk of technical documentation.
    public class DocumentationChunk
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        
        // In a real vector database, this would be a specialized type (e.g., float[] or Vector).
        // For SQLite VSS, it's often stored as a blob or specific type.
        // We use string here for simplicity in this "Hello World" example, 
        // but we will simulate vector math in the service layer.
        public string? VectorEmbedding { get; set; } 
    }

    // 2. Define the DbContext
    public class DocsContext : DbContext
    {
        public DbSet<DocumentationChunk> DocumentationChunks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using an in-memory database for this example to ensure it runs without file locks.
            // In production, use: optionsBuilder.UseSqlite("Data Source=docs.db");
            optionsBuilder.UseInMemoryDatabase("HelloWorldDocs");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // If using SQLite VSS, you would configure the vector column here:
            // modelBuilder.Entity<DocumentationChunk>()
            //     .Property(e => e.VectorEmbedding)
            //     .HasColumnType("VECTOR(3)"); // Assuming 3 dimensions for simplicity
        }
    }

    // 3. The Semantic Search Service
    // This service handles the "Intelligent" part: converting text to vectors and searching.
    public class SemanticSearchService
    {
        private readonly DocsContext _context;

        public SemanticSearchService(DocsContext context)
        {
            _context = context;
        }

        // Simulates generating a vector embedding (e.g., using Azure OpenAI or local ONNX model).
        // In a real app, this calls an LLM API.
        // Here, we calculate a simple "hash" vector for demonstration.
        private float[] GenerateEmbedding(string text)
        {
            // A real embedding would be a high-dimensional array (e.g., 1536 dimensions).
            // We will use 3 dimensions for this demo.
            // Logic: Sum of char codes modulo 10, normalized.
            var sum = text.Sum(c => (int)c);
            return new float[] 
            { 
                (sum % 10) / 10f, 
                ((sum / 10) % 10) / 10f, 
                ((sum / 100) % 10) / 10f 
            };
        }

        // Calculates Euclidean distance between two vectors.
        // In production, use optimized vector distance functions provided by the DB.
        private float CalculateDistance(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) throw new ArgumentException("Vector dimensions must match.");
            
            float sumOfSquares = 0;
            for (int i = 0; i < vecA.Length; i++)
            {
                sumOfSquares += (vecA[i] - vecB[i]) * (vecA[i] - vecB[i]);
            }
            return (float)Math.Sqrt(sumOfSquares);
        }

        // Adds a document chunk to the database with its vector.
        public async Task AddDocumentAsync(string content)
        {
            var vector = GenerateEmbedding(content);
            
            // In a real Vector DB, we store the vector directly. 
            // Here we serialize it to string for storage in our simple entity.
            var chunk = new DocumentationChunk
            {
                Content = content,
                VectorEmbedding = string.Join(",", vector)
            };

            _context.DocumentationChunks.Add(chunk);
            await _context.SaveChangesAsync();
        }

        // Performs the semantic search.
        public async Task<List<(string Content, float Distance)>> SearchAsync(string query, int topK = 2)
        {
            // 1. Convert query to vector
            var queryVector = GenerateEmbedding(query);

            // 2. Retrieve all documents (In production, use DB-side vector search for performance)
            var allDocs = await _context.DocumentationChunks.ToListAsync();

            // 3. Calculate similarity (Distance) for each document
            var results = new List<(string Content, float Distance)>();

            foreach (var doc in allDocs)
            {
                if (doc.VectorEmbedding == null) continue;

                // Parse stored vector back to float array
                var docVector = doc.VectorEmbedding.Split(',')
                                                    .Select(float.Parse)
                                                    .ToArray();

                float distance = CalculateDistance(queryVector, docVector);
                
                // 4. Store result
                results.Add((doc.Content, distance));
            }

            // 5. Sort by smallest distance (closest match) and take top K
            return results.OrderBy(r => r.Distance).Take(topK).ToList();
        }
    }

    // 4. Main Program Execution
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Semantic Search Engine - Hello World ===\n");

            // Initialize Database
            using var context = new DocsContext();
            await context.Database.EnsureCreatedAsync();

            var searchService = new SemanticSearchService(context);

            // --- Populate Data ---
            Console.WriteLine("Indexing documentation chunks...");
            await searchService.AddDocumentAsync("EF Core is an object-relational mapper (O/RM).");
            await searchService.AddDocumentAsync("Vector databases store data as embeddings for semantic search.");
            await searchService.AddDocumentAsync("C# is a strongly-typed language developed by Microsoft.");
            await searchService.AddDocumentAsync("RAG stands for Retrieval-Augmented Generation.");
            Console.WriteLine("Indexing complete.\n");

            // --- Perform Search ---
            string userQuery = "What is EF Core?";
            Console.WriteLine($"Query: \"{userQuery}\"");

            var results = await searchService.SearchAsync(userQuery);

            Console.WriteLine("\nTop Relevant Results:");
            foreach (var result in results)
            {
                // Note: Lower distance means more similar
                Console.WriteLine($"[Dist: {result.Distance:F4}] {result.Content}");
            }
        }
    }
}
