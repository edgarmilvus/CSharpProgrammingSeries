
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

// Project: VectorStore.Data
// File: Product.cs

using System;

namespace VectorStore.Data
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        
        // The vector representation (1536 dimensions typical for OpenAI)
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}

// Project: VectorStore.Data
// File: ProductContext.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VectorStore.Data
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Description).HasMaxLength(1000);

                // STRATEGY SELECTION:
                // Option A (JSON) is chosen for this solution because it is database-agnostic 
                // (works on SQL Server, SQLite, etc.) and human-readable. 
                // Option B (Binary) is more performant but less portable without specific converters.
                // Option C (pgvector) is the best for production Postgres but locks us into a specific DB.
                
                // Value Converter to serialize float[] to JSON string and back.
                var floatArrayConverter = new ValueConverter<float[], string>(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<float[]>(v, (System.Text.Json.JsonSerializerOptions)null)
                );

                entity.Property(p => p.Embedding)
                      .HasConversion(floatArrayConverter)
                      .HasColumnType("nvarchar(max)"); // JSON stored as text
            });
        }
    }
}

// Project: VectorStore.Data
// File: HybridSearchExtensions.cs

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace VectorStore.Data
{
    public static class HybridSearchExtensions
    {
        /// <summary>
        /// Conceptual LINQ structure for Hybrid Search.
        /// Note: Actual vector similarity calculation cannot be executed in standard SQL 
        /// via EF Core without a custom function mapping or stored procedure.
        /// </summary>
        public static IQueryable<Product> HybridSearch(
            this IQueryable<Product> products, 
            string searchTerm, 
            float[] queryVector, 
            decimal minPrice)
        {
            // 1. Traditional SQL Filter (Executed in DB)
            // Using Full-Text Search (FTS) would be ideal here, but EF Core supports simple string matching.
            var filtered = products.Where(p => p.Price >= minPrice);

            // 2. Semantic Search (Conceptual)
            // We cannot calculate Cosine Similarity in SQL via standard LINQ.
            // To push this to the DB, we would need:
            // a) A User Defined Function (UDF) in SQL Server mapping to C# math.
            // b) A database extension like pgvector (PostgreSQL).
            
            // If we were using pgvector, the query would look like:
            // filtered.Where(p => p.Embedding.CosineDistance(queryVector) < 0.5);
            
            // Since we are simulating a standard SQL DB here, we must perform the 
            // vector comparison in memory (Application Layer) after fetching candidates.
            
            return filtered;
        }
    }
}
