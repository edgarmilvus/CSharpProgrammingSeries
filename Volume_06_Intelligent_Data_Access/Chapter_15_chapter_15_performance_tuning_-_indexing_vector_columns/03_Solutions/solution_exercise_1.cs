
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// 1. Model Definition
public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    // Note: In a real scenario, you would use the Npgsql vector type.
    // For EF Core mapping, we often map this to a float[] or a specific type provided by the provider.
    // Here we use float[] for simplicity, but the migration will handle the specific 'vector' type.
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

public class SearchContext : DbContext
{
    public DbSet<Document> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Replace with your actual connection string
        optionsBuilder.UseNpgsql("Host=localhost;Database=vector_db;Username=postgres;Password=password");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 3. Index Configuration
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.Embedding)
            .HasMethod("hnsw") // Specifies HNSW algorithm
            .HasOperators("vector_l2_ops") // Euclidean distance
            .HasParameter("m", 16) // Number of bi-directional links
            .HasParameter("ef_construction", 64); // Size of dynamic candidate list during build
    }
}

public class SemanticSearchService
{
    private readonly SearchContext _context;

    public SemanticSearchService(SearchContext context)
    {
        _context = context;
    }

    // 5. Query Implementation
    public async Task<List<Document>> FindNearestDocumentsAsync(float[] queryEmbedding, int topK = 5)
    {
        // Note: The specific method name might vary based on the provider version.
        // Npgsql EF Core provider typically uses OrderByNearest or similar Linq extensions.
        // If OrderByNearest is not available in your specific version, raw SQL might be required, 
        // but standard EF Core patterns are shown here.
        
        // Since EF Core providers for pgvector evolve, we simulate the intent.
        // In modern Npgsql, you might need a custom DbFunction or use raw SQL for complex vector ops,
        // but the standard pattern is ordering by distance.
        
        return await _context.Documents
            // Simulating the vector distance calculation in LINQ (provider translates this)
            .OrderBy(d => EuclideanDistance(d.Embedding, queryEmbedding)) 
            .Take(topK)
            .ToListAsync();
    }

    // Helper to simulate distance calculation for LINQ translation
    private static double EuclideanDistance(float[] v1, float[] v2)
    {
        // This logic is usually handled by the database provider (Npgsql)
        // We define it here to satisfy the compiler, but it won't execute in-memory efficiently.
        double sum = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            double diff = v1[i] - v2[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }
    
    // Migration Generation (Console App or Startup Logic)
    public static async Task SetupDatabaseAsync()
    {
        using var context = new SearchContext();
        // 4. Migration: Generate and apply
        await context.Database.MigrateAsync();
    }
}

// 6. Performance Analysis (Instructor's Analysis section below)
