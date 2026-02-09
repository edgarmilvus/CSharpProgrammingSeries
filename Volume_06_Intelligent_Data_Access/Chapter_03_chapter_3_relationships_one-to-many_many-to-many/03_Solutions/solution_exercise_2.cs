
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

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAGPipeline.Data
{
    // 1. Entity Definitions
    public class Chunk
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; } = string.Empty;

        // Navigation Property for Many-to-Many
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        
        // Explicit Join Entity Navigation (for refactored scenario)
        public ICollection<ChunkTopic> ChunkTopics { get; set; } = new List<ChunkTopic>();
    }

    public class Topic
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        // Navigation Property for Many-to-Many
        public ICollection<Chunk> Chunks { get; set; } = new List<Chunk>();

        // Explicit Join Entity Navigation (for refactored scenario)
        public ICollection<ChunkTopic> ChunkTopics { get; set; } = new List<ChunkTopic>();
    }

    // Explicit Join Entity for Interactive Challenge
    public class ChunkTopic
    {
        public Guid ChunkId { get; set; }
        public Guid TopicId { get; set; }
        
        // Metadata
        public DateTime AssociationDate { get; set; }
        public double RelevanceScore { get; set; }

        // Navigation Properties
        public Chunk Chunk { get; set; } = null!;
        public Topic Topic { get; set; } = null!;
    }

    public class ManyToManyContext : DbContext
    {
        public DbSet<Chunk> Chunks { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<ChunkTopic> ChunkTopics { get; set; }

        public ManyToManyContext(DbContextOptions<ManyToManyContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 3. Configuration for Implicit Join Table (Standard EF Core 5+)
            // Note: This is commented out to switch to the explicit version required for the challenge,
            // but this is how the implicit version is configured:
            /*
            builder.Entity<Chunk>()
                .HasMany(c => c.Topics)
                .WithMany(t => t.Chunks)
                .UsingEntity("ChunkTopicAssociations"); 
            */

            // Interactive Challenge: Configuration for Explicit Join Entity
            builder.Entity<ChunkTopic>(entity =>
            {
                entity.HasKey(ct => new { ct.ChunkId, ct.TopicId });

                entity.HasOne(ct => ct.Chunk)
                    .WithMany(c => c.ChunkTopics)
                    .HasForeignKey(ct => ct.ChunkId);

                entity.HasOne(ct => ct.Topic)
                    .WithMany(t => t.ChunkTopics)
                    .HasForeignKey(ct => ct.TopicId);
            });
        }
    }

    public class ChunkRepository
    {
        private readonly ManyToManyContext _context;

        public ChunkRepository(ManyToManyContext context)
        {
            _context = context;
        }

        // 4. LINQ Query
        public async Task<object> GetTopicsForChunkAsync(Guid chunkId)
        {
            // Retrieving Topics for a specific Chunk
            // Using projection to avoid circular references if serialized
            return await _context.Chunks
                .Where(c => c.Id == chunkId)
                .Select(c => new 
                { 
                    c.Id, 
                    c.Content, 
                    Topics = c.Topics.Select(t => new { t.Id, t.Name }).ToList() 
                })
                .FirstOrDefaultAsync();
        }

        // Interactive Challenge: Query with Explicit Join Entity
        public async Task<object> GetChunkWithTopicMetadataAsync(Guid chunkId)
        {
            return await _context.ChunkTopics
                .Where(ct => ct.ChunkId == chunkId)
                .Select(ct => new 
                { 
                    TopicName = ct.Topic.Name, 
                    ct.AssociationDate, 
                    ct.RelevanceScore 
                })
                .ToListAsync();
        }
    }
}
