
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
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Storing vector as JSON string (Legacy format simulation)
    public string EmbeddingData { get; set; } = "[]"; 

    // Shadow Property will map to this in the DB
    [Column(TypeName = "vector(384)")]
    public float[]? Vector { get; set; } // Computed column target

    // Computed column for Keyword Search (concatenation)
    [Column(TypeName = "tsvector")]
    public string SearchVector { get; set; } = string.Empty; 
}

public class ECommerceContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Computed Column for Keyword Search
        // We define a SQL snippet that concatenates Title and Description.
        // Note: Syntax varies by provider (SQL Server vs PostgreSQL).
        // Using PostgreSQL syntax for vector compatibility:
        modelBuilder.Entity<Product>()
            .Property(p => p.SearchVector)
            .HasComputedColumnSql("to_tsvector('english', Title || ' ' || Description)", stored: true);

        // 2. Shadow Property for Vector
        // We map the shadow property 'Vector' to a computed column that parses the JSON 'EmbeddingData'.
        // This simulates converting legacy JSON data into a native vector type.
        // Note: JSON parsing syntax varies. This is a conceptual example.
        modelBuilder.Entity<Product>()
            .Property<float[]>("Vector") // Shadow property name
            .HasComputedColumnSql(
                "CAST(EmbeddingData::jsonb AS vector(384))", // Hypothetical SQL cast
                stored: true
            );

        // 3. Indexing Strategy
        var productModel = modelBuilder.Entity<Product>();

        // Full-Text Index on the Computed Column
        productModel.HasIndex(p => p.SearchVector)
            .HasMethod("GIN"); // GIN index is standard for tsvector in PostgreSQL

        // Vector Index on the Shadow Property (Computed Column)
        // We access the shadow property via string name in the index configuration.
        productModel.HasIndex("Vector")
            .HasMethod("ivfflat") // Approximate nearest neighbor
            .HasOperators("vector_cosine_ops");
    }
}

// Usage Example for Hybrid Search Query
public class OptimizedSearchService
{
    private readonly ECommerceContext _context;

    public OptimizedSearchService(ECommerceContext context)
    {
        _context = context;
    }

    public async Task SearchProducts(string query)
    {
        // Because we configured computed columns and indexes, 
        // EF Core generates efficient SQL.
        
        var results = await _context.Products
            // 1. Keyword Filter using the computed SearchVector column
            .Where(p => EF.Functions.ToTsVector("english", p.SearchVector).Matches(EF.Functions.ToTsQuery("english", query)))
            // 2. Ordering by Vector Similarity (using the computed Vector column)
            // Note: We must access the shadow property via EF.Property
            .OrderByDescending(p => EF.Functions.CosineDistance(
                EF.Property<float[]>(p, "Vector"), 
                new float[384] // Query vector
            ))
            .Take(20)
            .ToListAsync();
    }
}
