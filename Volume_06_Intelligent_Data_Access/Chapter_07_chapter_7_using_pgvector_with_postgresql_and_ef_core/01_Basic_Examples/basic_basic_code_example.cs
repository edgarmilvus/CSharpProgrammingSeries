
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

// 1. Define the Entity
// This represents a book in our library. We store the title and the vector embedding.
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    // The vector column. We use float[] for flexibility, but pgvector supports vector(n).
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

// 2. Define the DbContext
// This manages the connection and mapping to the database.
public class LibraryContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // CRITICAL: We must register the vector type mapping.
        // Without this, EF Core won't know how to translate float[] to PostgreSQL vector.
        var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost;Database=library_db;Username=postgres;Password=your_password");
        
        // This line enables the vector extension for EF Core.
        dataSourceBuilder.EnableVector();
        
        optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map the entity to the table
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            
            // Explicitly configure the column as a vector.
            // We define the dimension here (e.g., 384 for a small embedding model).
            entity.Property(e => e.Embedding)
                  .HasColumnType("vector(384)");
        });
    }
}

// 3. The "Hello World" Execution Logic
public class Program
{
    public static async Task Main()
    {
        // Initialize the database and seed dummy data
        await InitializeDatabaseAsync();

        // Define a search query (e.g., user input)
        string userQuery = "A story about a wizard in a magical forest";
        
        // In a real app, you would generate the embedding for this text using an AI model.
        // For this example, we simulate a vector representing the query.
        float[] queryEmbedding = GenerateDummyEmbedding(384, 100); 

        // Perform the Semantic Search
        using (var context = new LibraryContext())
        {
            Console.WriteLine($"Searching for: '{userQuery}'");
            Console.WriteLine("--------------------------------------------------");

            // THE CORE QUERY: Calculate Cosine Similarity
            // We use L2 Euclidean distance (Vector.L2Distance) which pgvector optimizes.
            // To get Cosine Similarity (1 - cosine_distance), we calculate: 
            // 1 - (||x|| * ||y||) - usually handled by the distance function directly.
            // pgvector's '<->' operator maps to L2 distance. 
            // For Cosine similarity, we often use the `<=>` operator, but EF Core L2Distance maps to L2.
            // However, for this example, we stick to L2 distance (Euclidean) as it's the standard 
            // distance metric for vector search in pgvector by default.
            
            var similarBooks = await context.Books
                .OrderBy(b => EF.Functions.L2Distance(b.Embedding, queryEmbedding))
                .Take(3)
                .ToListAsync();

            foreach (var book in similarBooks)
            {
                // Calculate the actual similarity score for display (Cosine Similarity)
                // Cosine Similarity = (A . B) / (||A|| * ||B||)
                float dotProduct = Vector3.Dot(
                    new Vector3(book.Embedding[0..3]), 
                    new Vector3(queryEmbedding[0..3])
                ); // Simplified for demo; real usage handles full vectors
                
                // Note: In a real scenario, you might calculate the distance in the DB or project it.
                // Here we just print the result.
                Console.WriteLine($"ID: {book.Id} | Title: {book.Title}");
            }
        }
    }

    // Helper to seed data (Simulating a pre-populated database)
    private static async Task InitializeDatabaseAsync()
    {
        using var context = new LibraryContext();
        // Ensure database is created
        await context.Database.EnsureDeletedAsync(); // Clean slate for demo
        await context.Database.EnsureCreatedAsync();

        if (!await context.Books.AnyAsync())
        {
            // Add dummy books with dummy embeddings
            var books = new List<Book>
            {
                new() { Title = "The Hobbit", Embedding = GenerateDummyEmbedding(384, 10) },
                new() { Title = "Advanced Calculus", Embedding = GenerateDummyEmbedding(384, 50) },
                new() { Title = "Harry Potter and the Sorcerer's Stone", Embedding = GenerateDummyEmbedding(384, 12) }, // Close to query
                new() { Title = "Cooking for Beginners", Embedding = GenerateDummyEmbedding(384, 90) }
            };
            
            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }

    // Helper to generate a random vector (simulating an AI model output)
    private static float[] GenerateDummyEmbedding(int dimensions, int seed)
    {
        var random = new Random(seed);
        var embedding = new float[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            embedding[i] = (float)random.NextDouble();
        }
        return embedding;
    }
}
