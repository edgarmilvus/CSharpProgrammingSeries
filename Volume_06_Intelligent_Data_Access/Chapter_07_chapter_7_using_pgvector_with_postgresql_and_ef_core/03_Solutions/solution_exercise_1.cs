
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

// 1. Entity Definition
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    
    // Using 'float[]' for flexibility, EF Core can map this to 'vector(384)'
    // Alternatively, use the specific 'Vector' type from the library
    public float[] Embedding { get; set; } 
}

public class ProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set;
    public double SimilarityScore { get; set; } // 1 - Distance
}

// 2. DbContext Configuration
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=ecommerce;Username=postgres;Password=secret");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure the extension is created (usually done via migration raw SQL)
        // modelBuilder.HasPostgresExtension("vector"); 

        modelBuilder.Entity<Product>()
            .Property(p => p.Embedding)
            .HasColumnType("vector(384)");
    }
}

public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context) => _context = context;

    // 3. Hybrid Query Method
    public async Task<List<ProductDto>> SearchProducts(string queryText, string category, decimal maxPrice, int minStock)
    {
        // --- Edge Case Handling ---
        // If the query text is null or empty, we want to fall back to a pure SQL filter.
        // To avoid multiple round-trips or complex conditional logic that might break SQL generation,
        // we build the IQueryable dynamically. This ensures the entire query is executed in a single round-trip.
        
        // Mock embedding generation (In production, call an AI service)
        float[] queryVector = GenerateMockEmbedding(queryText, 384);

        var query = _context.Products
            .Where(p => p.Price <= maxPrice && p.StockQuantity >= minStock);

        // Dynamic filtering for Category
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        // 4. Vector Search Logic
        // If queryText is empty, we skip the vector distance calculation entirely to save compute.
        if (!string.IsNullOrEmpty(queryText))
        {
            // We calculate the distance and project it immediately
            // We use Cosine Similarity (1 - Distance) for the score
            query = query
                .Select(p => new
                {
                    Product = p,
                    Score = 1 - EF.Functions.VectorCosineDistance(p.Embedding, new Vector(queryVector))
                })
                .Where(x => x.Score > 0.0) // Optional: Filter out very irrelevant results
                .OrderByDescending(x => x.Score) // Order by similarity
                .Select(x => new ProductDto
                {
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    SimilarityScore = x.Score
                });
        }
        else
        {
            // Fallback sort (e.g., by Price or Name) if no semantic search is performed
            query = query.OrderBy(p => p.Price)
                         .Select(p => new ProductDto
                         {
                             Name = p.Name,
                             Price = p.Price,
                             SimilarityScore = 0 // No semantic search performed
                         });
        }

        return await query.Take(20).ToListAsync();
    }

    private float[] GenerateMockEmbedding(string text, int dimensions)
    {
        // Simulating a vector generation. 
        // In a real app, this calls a model like 'all-MiniLM-L6-v2'.
        // We generate a deterministic random vector based on the string length to make it reproducible for the exercise.
        var rng = new Random(string.IsNullOrEmpty(text) ? 0 : text.Length);
        var vector = new float[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            vector[i] = (float)rng.NextDouble();
        }
        return vector;
    }
}
