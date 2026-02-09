
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGPipeline.Data
{
    // 1. Entity Definitions
    public class KnowledgeSource
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<Chunk> Chunks { get; set; } = new List<Chunk>();
    }

    public class Chunk
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; } = string.Empty;
        
        // Modeling embedding vector as string for exercise simplicity
        public string EmbeddingVector { get; set; } = string.Empty; 
        
        // Foreign Key
        public Guid SourceId { get; set; }
        
        // Navigation Property
        public KnowledgeSource Source { get; set; } = null!;

        // Self-referencing properties for Interactive Challenge
        public Guid? ParentChunkId { get; set; }
        public Chunk? ParentChunk { get; set; }
        public ICollection<Chunk> ChildChunks { get; set; } = new List<Chunk>();
    }

    public class RAGContext : DbContext
    {
        public DbSet<KnowledgeSource> KnowledgeSources { get; set; }
        public DbSet<Chunk> Chunks { get; set; }

        public RAGContext(DbContextOptions<RAGContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 2. Fluent API Configuration
            builder.Entity<KnowledgeSource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            });

            builder.Entity<Chunk>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Relationship: KnowledgeSource (1) -> Chunk (Many)
                // Cascade delete ensures chunks are deleted if source is deleted
                entity.HasOne(c => c.Source)
                      .WithMany(s => s.Chunks)
                      .HasForeignKey(c => c.SourceId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Interactive Challenge: Self-referencing One-to-Many
                // A chunk can have a parent chunk
                entity.HasOne(c => c.ParentChunk)
                      .WithMany(p => p.ChildChunks)
                      .HasForeignKey(c => c.ParentChunkId)
                      .OnDelete(DeleteBehavior.ClientCascade); // Prevent cycles in DB, handle in app
            });
        }
    }

    public class KnowledgeSourceRepository
    {
        private readonly RAGContext _context;

        public KnowledgeSourceRepository(RAGContext context)
        {
            _context = context;
        }

        // 3. Eager Loading Method
        public async Task<KnowledgeSource?> GetSourceWithChunksAsync(Guid sourceId)
        {
            // Using .Include() to eagerly load the Chunks collection
            return await _context.KnowledgeSources
                .Include(s => s.Chunks) 
                .FirstOrDefaultAsync(s => s.Id == sourceId);
        }

        // 4. LINQ Query for filtering
        public async Task<List<Chunk>> GetChunksContainingEmbeddingAsync()
        {
            // Case-insensitive search using EF.Functions.Like (or raw string comparison if provider specific)
            // Note: EF Core 8+ supports case-insensitive string comparison by default on many providers, 
            // but explicit handling is safer.
            return await _context.Chunks
                .Where(c => c.Content.Contains("embedding")) 
                .ToListAsync();
        }

        // Interactive Challenge: Query for hierarchy
        public async Task<Chunk?> GetChunkWithChildrenAsync(Guid chunkId)
        {
            return await _context.Chunks
                .Include(c => c.ChildChunks) // Load immediate children
                .FirstOrDefaultAsync(c => c.Id == chunkId);
        }
    }
}
