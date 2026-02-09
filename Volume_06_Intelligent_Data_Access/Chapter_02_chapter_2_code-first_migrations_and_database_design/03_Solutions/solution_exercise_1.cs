
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeRepository.Domain
{
    // 1. Define the Document entity
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long ContentSize { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    }

    // 2. Define the DocumentChunk entity
    public class DocumentChunk
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid DocumentId { get; set; } // Foreign Key property
        public Document Document { get; set; } = null!; // Navigation property

        public int Sequence { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        // Defined as byte[] to represent a binary vector blob
        public byte[]? Embedding { get; set; }

        public string? Metadata { get; set; }
    }

    // 3. Create the DbContext
    public class KnowledgeRepositoryContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }

        public KnowledgeRepositoryContext(DbContextOptions<KnowledgeRepositoryContext> options)
            : base(options)
        {
        }

        // 4. Configure the schema using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Document entity
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.LastModifiedDate).IsRequired();
                entity.Property(e => e.IsDeleted).IsRequired();

                // Global Query Filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure DocumentChunk entity
            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure Content as nvarchar(max)
                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasColumnType("nvarchar(max)");

                // Configure Embedding as varbinary(max)
                entity.Property(e => e.Embedding)
                      .HasColumnType("varbinary(max)");

                // Setup One-to-Many relationship
                entity.HasOne(d => d.Document)
                      .WithMany(doc => doc.Chunks)
                      .HasForeignKey(d => d.DocumentId)
                      .OnDelete(DeleteBehavior.Cascade); // Or Restrict based on requirements

                // Composite Index for optimization
                entity.HasIndex(e => new { e.DocumentId, e.Sequence });
            });
        }
    }

    // 5. Registration in Dependency Injection (Simulated in Program.cs)
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // Configuration setup (mocking IConfiguration)
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=KnowledgeRepoDb;Trusted_Connection=True;MultipleActiveResultSets=true" }
                })
                .Build();

            // Register DbContext
            services.AddDbContext<KnowledgeRepositoryContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Build the provider
            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("DbContext registered successfully.");
        }
    }
}
