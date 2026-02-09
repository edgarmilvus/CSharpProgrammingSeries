
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Define the Data Model
// We represent a document with a text summary and a pre-computed vector embedding.
// In a real scenario, the vector would be a float[] or a specialized type like pgvector's Vector.
public class LibraryDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    
    // Simulating a vector embedding (e.g., from Azure OpenAI or local ONNX model)
    // For this "Hello World", we use a simple 3-dimensional vector for readability.
    public string VectorEmbedding { get; set; } = string.Empty; 
}

// 2. Define the DbContext
public class LibraryContext : DbContext
{
    public DbSet<LibraryDocument> Documents { get; set; }

    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the vector storage. 
        // Note: EF Core doesn't natively support vector types out-of-the-box.
        // We store it as a string for this simulation, but in production (e.g., with pgvector),
        // you would map this to a 'vector' column type.
        modelBuilder.Entity<LibraryDocument>()
            .Property(e => e.VectorEmbedding)
            .HasColumnType("text");
    }
}

// 3. The Hybrid Search Service
public class HybridSearchService
{
    private readonly LibraryContext _context;

    public HybridSearchService(LibraryContext context)
    {
        _context = context;
    }

    // Main entry point for the hybrid query
    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        // Step A: Keyword Search (BM25/Full-text approximation)
        // We filter documents where the title or summary contains the query terms.
        var keywordResults = await _context.Documents
            .Where(d => d.Title.Contains(query) || d.Summary.Contains(query))
            .Select(d => new { d.Id, d.Title })
            .ToListAsync();

        // Step B: Vector Search (Semantic Similarity)
        // We fetch all documents to calculate cosine similarity in memory (for this demo).
        // In production, this logic is pushed down to the DB (e.g., using 'pgvector').
        var allDocs = await _context.Documents.ToListAsync();
        
        // Convert query string to a vector (Mocked for this example)
        var queryVector = MockEmbeddingGenerator.Generate(query);

        var vectorResults = allDocs
            .Select(d => new 
            { 
                d.Id, 
                d.Title, 
                Score = CalculateCosineSimilarity(
                    queryVector, 
                    ParseVector(d.VectorEmbedding)
                ) 
            })
            .Where(x => x.Score > 0.1) // Threshold to filter noise
            .ToList();

        // Step C: Result Fusion (Reciprocal Rank Fusion - Simplified)
        // We combine the two lists, giving weight to both keyword matches and semantic matches.
        var fusedResults = new Dictionary<int, (string Title, double FinalScore)>();

        // Add Keyword Scores (Rank-based weighting)
        for (int i = 0; i < keywordResults.Count; i++)
        {
            // RRF formula: 1 / (rank + k) (k=60 is standard, simplified here)
            double score = 1.0 / (i + 1 + 60); 
            fusedResults[keywordResults[i].Id] = (keywordResults[i].Title, score);
        }

        // Add Vector Scores (Similarity-based weighting)
        foreach (var vRes in vectorResults)
        {
            // We normalize vector scores to a 0-1 range and apply a weight
            double weight = 0.5; // Give vector search 50% importance
            double score = vRes.Score * weight;

            if (fusedResults.ContainsKey(vRes.Id))
            {
                var existing = fusedResults[vRes.Id];
                fusedResults[vRes.Id] = (existing.Title, existing.FinalScore + score);
            }
            else
            {
                fusedResults[vRes.Id] = (vRes.Title, score);
            }
        }

        // Step D: Sort and Return
        return fusedResults
            .OrderByDescending(x => x.Value.FinalScore)
            .Select(x => new SearchResult { Id = x.Key, Title = x.Value.Title, Score = x.Value.FinalScore })
            .ToList();
    }

    // Helper: Parse string "1.0,2.0,3.0" to double[]
    private double[] ParseVector(string vectorStr)
    {
        return vectorStr.Split(',').Select(double.Parse).ToArray();
    }

    // Helper: Calculate Cosine Similarity
    private double CalculateCosineSimilarity(double[] vecA, double[] vecB)
    {
        if (vecA.Length != vecB.Length) throw new ArgumentException("Vectors must be same length");

        double dotProduct = 0.0;
        double magnitudeA = 0.0;
        double magnitudeB = 0.0;

        for (int i = 0; i < vecA.Length; i++)
        {
            dotProduct += vecA[i] * vecB[i];
            magnitudeA += vecA[i] * vecA[i];
            magnitudeB += vecB[i] * vecB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0) return 0;
        return dotProduct / (magnitudeA * magnitudeB);
    }
}

// 4. Mock Data Generator (To make the example runnable without external services)
public static class MockEmbeddingGenerator
{
    // Simulates an AI model turning text into numbers.
    // "Optimization" -> [0.9, 0.1, 0.2]
    // "Performance"  -> [0.8, 0.2, 0.3]
    public static double[] Generate(string text)
    {
        // Simple hash-based generation for deterministic output
        double x = text.Length * 0.1;
        double y = text.Contains("Optimization") ? 0.9 : 0.1;
        double z = text.Contains("Performance") ? 0.8 : 0.2;
        return new[] { x, y, z };
    }
}

public class SearchResult
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Score { get; set; }
}

// 5. Main Execution
class Program
{
    static async Task Main(string[] args)
    {
        // Setup Dependency Injection (Simulated)
        var services = new ServiceCollection();
        services.AddDbContext<LibraryContext>(options => 
            options.UseInMemoryDatabase("HybridSearchDb"));
        services.AddScoped<HybridSearchService>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Seed Data
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
            await context.Database.EnsureCreatedAsync();

            // Vector embeddings are pre-calculated. 
            // In reality, these come from a model (e.g., text-embedding-ada-002).
            context.Documents.AddRange(
                new LibraryDocument { Title = "Intro to C#", Summary = "Basics of the language", VectorEmbedding = "0.1,0.1,0.1" },
                new LibraryDocument { Title = "Performance Tuning", Summary = "How to optimize code", VectorEmbedding = "0.9,0.9,0.1" }, // High similarity to "Optimization"
                new LibraryDocument { Title = "Database Indexing", Summary = "Improving query speed", VectorEmbedding = "0.8,0.8,0.2" }  // Medium similarity
            );
            await context.SaveChangesAsync();
        }

        // Execute Search
        using (var scope = serviceProvider.CreateScope())
        {
            var searchService = scope.ServiceProvider.GetRequiredService<HybridSearchService>();
            
            // User Query: "Optimization" (Keywords match 'Optimize', Vector matches 'Performance Tuning')
            string userQuery = "Optimization";
            Console.WriteLine($"Searching for: '{userQuery}'\n");

            var results = await searchService.SearchAsync(userQuery);

            foreach (var result in results)
            {
                Console.WriteLine($"[Score: {result.Score:F4}] {result.Title}");
            }
        }
    }
}
