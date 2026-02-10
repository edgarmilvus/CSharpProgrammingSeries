
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

public class Product
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

public class EcommerceContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=ecommerce;Username=postgres;Password=password");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 2. Composite Indexing Strategy
        // Option B: Metadata Filtering (Where clause pushdown)
        // We create a standard B-Tree index on scalar fields to speed up the filtering phase.
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.Category, p.Price });
            
        // Vector index (HNSW) is separate.
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Embedding)
            .HasMethod("hnsw");
    }
}

public class HybridSearchService
{
    private readonly EcommerceContext _context;

    public HybridSearchService(EcommerceContext context)
    {
        _context = context;
    }

    // 3. Query Implementation (Pre-filtering)
    public async Task<List<Product>> SearchElectronicsAsync(float[] targetEmbedding)
    {
        // The Where clause is applied BEFORE the vector search in the query plan.
        // This reduces the search space for the vector index, avoiding "visited nodes" limits.
        return await _context.Products
            .Where(p => p.Category == "Electronics" && p.Price < 1000)
            // Simulating OrderByNearest with a custom distance function
            .OrderBy(p => EuclideanDistance(p.Embedding, targetEmbedding))
            .Take(10)
            .ToListAsync();
    }

    // 4. Interactive Challenge: Conditional Scoring
    public async Task<List<ProductResult>> SearchWithPenaltyAsync(float[] targetEmbedding)
    {
        // This requires client-side evaluation of the score modifier if the DB provider 
        // doesn't support custom scoring functions in LINQ directly.
        // However, we can structure the query to fetch data and project it.
        
        var query = _context.Products
            .Select(p => new 
            {
                Product = p,
                // Calculate base similarity (simulated)
                BaseSimilarity = 1.0 - EuclideanDistance(p.Embedding, targetEmbedding) // Normalized placeholder
            })
            .OrderByDescending(x => x.BaseSimilarity)
            .Take(50); // Fetch a larger window to apply penalty logic

        var intermediateResults = await query.ToListAsync();

        // Apply penalty logic in memory (or use a SQL CASE statement via raw SQL for efficiency)
        var finalResults = intermediateResults
            .Select(x => new ProductResult
            {
                Product = x.Product,
                // Penalize "Outdoors" by subtracting a fixed value (e.g., 0.2)
                FinalScore = x.BaseSimilarity - (x.Product.Category == "Outdoors" ? 0.2 : 0)
            })
            .OrderByDescending(r => r.FinalScore)
            .Take(10)
            .ToList();

        return finalResults;
    }

    private double EuclideanDistance(float[] v1, float[] v2)
    {
        double sum = 0;
        for (int i = 0; i < v1.Length; i++) sum += Math.Pow(v1[i] - v2[i], 2);
        return Math.Sqrt(sum);
    }
}

public class ProductResult 
{
    public Product Product { get; set; }
    public double FinalScore { get; set; }
}
