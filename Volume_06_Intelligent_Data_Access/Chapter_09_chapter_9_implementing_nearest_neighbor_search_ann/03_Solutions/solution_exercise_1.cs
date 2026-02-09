
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chapter9.Exercise1
{
    // 1. Domain Entity
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        
        // The vector data. 
        // Note: We use a standard array. For high-performance scenarios, Memory<float> is preferred, 
        // but ValueConverter support is more established with arrays for now.
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    // 2. DbContext Configuration
    public class DocumentContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using InMemory for runnable code, but configuration applies to any provider.
            optionsBuilder.UseInMemoryDatabase("DocumentDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure the Embedding property
                entity.Property(e => e.Embedding)
                    .HasConversion(new FloatArrayToPgVectorConverter())
                    .HasMaxLength(1536); // Conceptual length check
            });
        }
    }

    // Custom Value Converter for Vector Serialization
    // Converts float[] to a string format "1.0,2.0,3.0" compatible with pgvector text input.
    public class FloatArrayToPgVectorConverter : ValueConverter<float[], string>
    {
        public FloatArrayToPgVectorConverter() 
            : base(
                v => string.Join(",", v), // Convert to DB
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries) // Convert from DB
                       .Select(float.Parse)
                       .ToArray()) 
        { }
    }

    // 3. Repository Pattern & Interactive Challenge
    public interface IVectorDistanceStrategy
    {
        double Calculate(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB);
    }

    public class CosineSimilarityStrategy : IVectorDistanceStrategy
    {
        public double Calculate(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0; // Handle zero vectors

            // Cosine Similarity = dot / (magA * magB)
            // We return 1 - similarity to treat it as a distance (lower is better) for sorting
            // Or return similarity directly and sort Descending. Let's return similarity.
            return dotProduct / (magnitudeA * magnitudeB);
        }
    }

    public class EuclideanDistanceStrategy : IVectorDistanceStrategy
    {
        public double Calculate(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            double sumSqDiff = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                double diff = vectorA[i] - vectorB[i];
                sumSqDiff += diff * diff;
            }

            // Euclidean Distance (L2)
            return Math.Sqrt(sumSqDiff);
        }
    }

    public interface IVectorRepository<T> where T : class
    {
        Task<List<T>> SearchAsync(float[] queryVector, int topK, IVectorDistanceStrategy? strategy = null);
    }

    public class DocumentVectorRepository : IVectorRepository<Document>
    {
        private readonly DocumentContext _context;
        private readonly IVectorDistanceStrategy _defaultStrategy;

        public DocumentVectorRepository(DocumentContext context)
        {
            _context = context;
            _defaultStrategy = new CosineSimilarityStrategy();
        }

        public async Task<List<Document>> SearchAsync(float[] queryVector, int topK, IVectorDistanceStrategy? strategy = null)
        {
            if (queryVector == null || queryVector.Length == 0)
                throw new ArgumentNullException(nameof(queryVector), "Query vector cannot be null or empty.");

            var activeStrategy = strategy ?? _defaultStrategy;
            var documents = await _context.Documents.ToListAsync(); // Simulate DB pull
            
            // Simulate database-side computation logic in C#
            var scoredDocs = documents
                .Where(d => d.Embedding != null && d.Embedding.Length == queryVector.Length)
                .Select(d => new 
                {
                    Document = d,
                    Score = activeStrategy.Calculate(new ReadOnlySpan<float>(d.Embedding), new ReadOnlySpan<float>(queryVector))
                });

            // If using Cosine Similarity, we want highest score first. 
            // If using Euclidean, we want lowest distance first.
            // To handle both generically, we check the strategy type or assume the caller knows.
            // For this implementation, we'll assume Cosine returns similarity (High is good) 
            // and Euclidean returns distance (Low is good).
            
            IEnumerable<Document> results;
            if (activeStrategy is CosineSimilarityStrategy)
            {
                results = scoredDocs.OrderByDescending(x => x.Score).Select(x => x.Document);
            }
            else
            {
                results = scoredDocs.OrderBy(x => x.Score).Select(x => x.Document);
            }

            return results.Take(topK).ToList();
        }
    }
}
