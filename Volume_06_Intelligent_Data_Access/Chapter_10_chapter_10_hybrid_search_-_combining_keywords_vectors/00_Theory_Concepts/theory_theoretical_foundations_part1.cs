
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class SearchableDocument
{
    public int Id { get; set; }
    
    [MaxLength(2000)]
    public string Title { get; set; }
    
    // The raw text for keyword indexing
    public string Content { get; set; }
    
    // The vector embedding, stored as a byte array or a specialized type
    // depending on the database provider (e.g., pgvector in PostgreSQL)
    public byte[] Embedding { get; set; } 
    
    // For efficient keyword search, we might also have a pre-computed
    // Full-Text Search vector column (tsvector in PostgreSQL)
    public string SearchVector { get; set; } 
}

public class HybridSearchContext : DbContext
{
    public DbSet<SearchableDocument> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuration for a database that supports both Full-Text Search and Vectors
        // e.g., PostgreSQL with pgvector and pg_trgm extensions.
        optionsBuilder.UseNpgsql("YourConnectionString");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the vector column for pgvector
        modelBuilder.Entity<SearchableDocument>()
            .Property(d => d.Embedding)
            .HasColumnType("vector(1536)"); // Dimensionality of the embedding model

        // Configure the GIN index for Full-Text Search
        modelBuilder.Entity<SearchableDocument>()
            .HasIndex(d => d.SearchVector)
            .HasMethod("GIN");
    }
}
