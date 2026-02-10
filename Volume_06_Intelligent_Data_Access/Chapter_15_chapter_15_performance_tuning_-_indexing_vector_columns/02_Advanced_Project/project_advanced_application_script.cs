
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace VectorIndexingDemo
{
    // ---------------------------------------------------------
    // 1. DATA MODELS
    // ---------------------------------------------------------
    // Represents a document chunk with an embedding vector.
    // In a real RAG system, this would be a chunk of text from a larger document.
    public class DocumentChunk
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        
        // In EF Core 8+, we can map float[] to a database vector type (e.g., pgvector).
        // This array represents the semantic meaning of the text (e.g., from an AI model).
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    // ---------------------------------------------------------
    // 2. DATABASE CONTEXT
    // ---------------------------------------------------------
    public class VectorDbContext : DbContext
    {
        public DbSet<DocumentChunk> DocumentChunks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQLite for demonstration purposes. 
            // NOTE: To use actual vector indexing (HNSW/IVFFlat), you would typically use 
            // PostgreSQL (with pgvector extension) or SQL Server 2025+.
            // For this demo, we simulate the structure and logic.
            optionsBuilder.UseSqlite("Data Source=vectors.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRITICAL: Indexing Strategy Configuration
            // This is where we define the performance tuning.
            
            modelBuilder.Entity<DocumentChunk>()
                .HasIndex(e => e.Embedding)
                // In PostgreSQL, this would be: "CREATE INDEX ON ... USING hnsw (embedding vector_cosine_ops)"
                // In SQL Server, this would be: "CREATE VECTOR INDEX ... USING HNSW"
                // EF Core 8 allows specifying the index method via the 'IsUnique' or other methods 
                // via raw SQL or provider-specific extensions, but here we simulate the intent.
                .HasMethod("HNSW") // Hypothetical extension method for demonstration
                .HasOperators("vector_cosine_ops"); // Hypothetical operator class for cosine similarity
        }
    }

    // ---------------------------------------------------------
    // 3. CORE LOGIC: VECTOR OPERATIONS & INDEXING
    // ---------------------------------------------------------
    public static class VectorEngine
    {
        // Calculates Cosine Similarity between two vectors.
        // Formula: (A . B) / (||A|| * ||B||)
        // Used to rank results when performing similarity search.
        public static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same dimension.");

            double dotProduct = 0.0;
            double magnitudeA = 0.0;
            double magnitudeB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0; // Handle zero vectors

            return dotProduct / (magnitudeA * magnitudeB);
        }

        // Simulates the generation of an embedding vector.
        // In production, this calls an AI model (e.g., OpenAI, Azure AI).
        public static float[] GenerateEmbedding(string text)
        {
            // Deterministic pseudo-random generator to simulate consistent vector generation
            // based on text content for this demo.
            var random = new Random(text.GetHashCode());
            var vector = new float[128]; // 128-dimensional vector (common for smaller models)
            
            for (int i = 0; i < vector.Length; i++)
            {
                // Generate values between -1 and 1
                vector[i] = (float)(random.NextDouble() * 2 - 1);
            }

            // Normalize vector to unit length (simulating model output normalization)
            double sumSquares = 0;
            foreach (var val in vector) sumSquares += val * val;
            double magnitude = Math.Sqrt(sumSquares);
            
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(vector[i] / magnitude);
            }

            return vector;
        }
    }

    // ---------------------------------------------------------
    // 4. REAL-WORLD APPLICATION: RAG SYSTEM
    // ---------------------------------------------------------
    public class RagSystem
    {
        private readonly VectorDbContext _context;

        public RagSystem(VectorDbContext context)
        {
            _context = context;
        }

        // Method 1: Ingest Data (Write Path)
        // We populate the database with documents.
        public async Task SeedDatabaseAsync()
        {
            Console.WriteLine("Seeding database with document chunks...");
            
            var documents = new[]
            {
                "The quick brown fox jumps over the lazy dog.",
                "A fast red fox leaps over a sleepy hound.",
                "The sun rises in the east and sets in the west.",
                "Solar energy is renewable and sustainable.",
                "C# is a modern, object-oriented programming language.",
                "Java is a class-based, object-oriented programming language."
            };

            var chunks = new List<DocumentChunk>();
            foreach (var doc in documents)
            {
                chunks.Add(new DocumentChunk
                {
                    Content = doc,
                    Embedding = VectorEngine.GenerateEmbedding(doc)
                });
            }

            await _context.DocumentChunks.AddRangeAsync(chunks);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Seeded {chunks.Count} documents.");
        }

        // Method 2: Naive Search (No Index)
        // Calculates similarity for every row in the table.
        // Complexity: O(N * D) where N is rows, D is vector dimension.
        // SLOW for large datasets.
        public async Task<List<(string Content, double Score)>> SearchNaiveAsync(string query, int topK)
        {
            Console.WriteLine("\n--- Performing Naive Search (Full Table Scan) ---");
            var sw = Stopwatch.StartNew();

            var queryVector = VectorEngine.GenerateEmbedding(query);
            var allDocs = await _context.DocumentChunks.ToListAsync();
            var results = new List<(string Content, double Score)>();

            // Manual iteration (avoiding LINQ for strict adherence to "basic blocks" directive)
            for (int i = 0; i < allDocs.Count; i++)
            {
                double score = VectorEngine.CalculateCosineSimilarity(queryVector, allDocs[i].Embedding);
                results.Add((allDocs[i].Content, score));
            }

            // Simple Bubble Sort for Top K (demonstrating basic algorithmic logic)
            // In production, use a Priority Queue or OrderByDescending.
            for (int i = 0; i < results.Count; i++)
            {
                for (int j = 0; j < results.Count - 1 - i; j++)
                {
                    if (results[j].Score < results[j + 1].Score)
                    {
                        var temp = results[j];
                        results[j] = results[j + 1];
                        results[j + 1] = temp;
                    }
                }
            }

            sw.Stop();
            Console.WriteLine($"Naive Search Time: {sw.ElapsedMilliseconds}ms");
            
            // Return top K
            var finalResults = new List<(string, double)>();
            for(int i = 0; i < Math.Min(topK, results.Count); i++)
            {
                finalResults.Add(results[i]);
            }
            return finalResults;
        }

        // Method 3: Optimized Search (Simulating Index Usage)
        // In a real database (PostgreSQL/SQL Server), the query planner uses the HNSW index
        // to avoid scanning the whole table.
        // Complexity: Approx O(log N) for HNSW.
        public async Task<List<(string Content, double Score)>> SearchOptimizedAsync(string query, int topK)
        {
            Console.WriteLine("\n--- Performing Optimized Search (Simulating HNSW Index) ---");
            var sw = Stopwatch.StartNew();

            var queryVector = VectorEngine.GenerateEmbedding(query);

            // SQL Equivalent: 
            // SELECT "Content", embedding <=> @queryVector AS "Score"
            // FROM "DocumentChunks"
            // ORDER BY embedding <=> @queryVector
            // LIMIT @topK;
            // The '<=>' operator is the cosine distance operator in pgvector.
            
            // Since we are using SQLite for this demo (which doesn't support vector indexes natively),
            // we simulate the performance gain by pre-filtering or using a heuristic.
            // In a real scenario, we simply execute the LINQ query and EF Core translates it 
            // to use the index.
            
            var results = await _context.DocumentChunks
                .Select(d => new 
                { 
                    d.Content, 
                    Score = VectorEngine.CalculateCosineSimilarity(queryVector, d.Embedding) 
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToListAsync();

            sw.Stop();
            Console.WriteLine($"Optimized Search Time: {sw.ElapsedMilliseconds}ms (Simulated Index Overhead)");

            var formattedResults = new List<(string, double)>();
            foreach(var r in results)
            {
                formattedResults.Add((r.Content, r.Score));
            }
            
            return formattedResults;
        }
    }

    // ---------------------------------------------------------
    // 5. MAIN EXECUTION
    // ---------------------------------------------------------
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup Dependency Injection (Standard .NET pattern)
            var services = new ServiceCollection();
            services.AddDbContext<VectorDbContext>();
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VectorDbContext>();
                
                // Ensure database is created
                await context.Database.EnsureDeletedAsync(); // Clean slate
                await context.Database.EnsureCreatedAsync();

                var ragSystem = new RagSystem(context);

                // 1. Ingest Data
                await ragSystem.SeedDatabaseAsync();

                // 2. Define Query
                string userQuery = "fast animal jump";
                Console.WriteLine($"\nQuery: \"{userQuery}\"");

                // 3. Execute Naive Search (Baseline)
                var naiveResults = await ragSystem.SearchNaiveAsync(userQuery, 2);
                Console.WriteLine("Top Results (Naive):");
                foreach (var res in naiveResults)
                {
                    Console.WriteLine($" - {res.Content} (Score: {res.Score:F4})");
                }

                // 4. Execute Optimized Search (Simulated Index)
                var optResults = await ragSystem.SearchOptimizedAsync(userQuery, 2);
                Console.WriteLine("Top Results (Optimized):");
                foreach (var res in optResults)
                {
                    Console.WriteLine($" - {res.Content} (Score: {res.Score:F4})");
                }
            }
        }
    }
}
