
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Chapter9.Exercise5
{
    public enum Role { User, Assistant, System }

    // 1. Memory Entity
    public class MemoryEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public Role Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }

    public class MemoryContext : DbContext
    {
        public DbSet<MemoryEntry> MemoryEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MemoryDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 5. EF Core Configuration: Index on SessionId
            modelBuilder.Entity<MemoryEntry>()
                .HasIndex(m => m.SessionId);
            
            modelBuilder.Entity<MemoryEntry>()
                .HasIndex(m => m.Timestamp);
        }
    }

    public interface IMemoryStore
    {
        Task SaveMessageAsync(Guid sessionId, string role, string content, float[] embedding);
        Task<List<MemoryEntry>> RetrieveContextAsync(Guid sessionId, float[] queryEmbedding, int limit);
    }

    public class MemoryStore : IMemoryStore
    {
        private readonly MemoryContext _context;

        public MemoryStore(MemoryContext context) => _context = context;

        public async Task SaveMessageAsync(Guid sessionId, string role, string content, float[] embedding)
        {
            var entry = new MemoryEntry
            {
                SessionId = sessionId,
                Role = Enum.Parse<Role>(role, true),
                Content = content,
                Embedding = embedding
            };
            _context.MemoryEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MemoryEntry>> RetrieveContextAsync(Guid sessionId, float[] queryEmbedding, int limit)
        {
            // Basic retrieval logic (implementation details depend on DB provider)
            // For this exercise, we'll rely on the ContextBuilder to handle the complex logic.
            return await _context.MemoryEntries
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .Take(limit * 2) // Fetch more to allow filtering
                .ToListAsync();
        }
    }

    // 3. Context Builder
    public class ContextBuilder
    {
        private readonly IMemoryStore _store;
        private const int TokenLimit = 4000;
        private const int TokensPerMessage = 100; // Rough estimate

        public ContextBuilder(IMemoryStore store) => _store = store;

        public async Task<string> BuildContextAsync(Guid sessionId, float[] queryEmbedding)
        {
            // 1. Retrieve Chronological History (Last 5)
            var allMessages = await _store.RetrieveContextAsync(sessionId, queryEmbedding, 50); // Fetch buffer
            var chronological = allMessages
                .OrderBy(m => m.Timestamp)
                .TakeLast(5)
                .ToList();

            // 2. Retrieve Semantic Context (Top 3 via Vector Search)
            // In a real DB, this would be a SQL query. Here we simulate the calculation.
            var semantic = allMessages
                .Select(m => new { Entry = m, Score = CalculateCosine(m.Embedding, queryEmbedding) })
                .OrderByDescending(x => x.Score)
                .Take(3)
                .Select(x => x.Entry)
                .ToList();

            // 3. Merge and Deduplicate
            var merged = chronological.Union(semantic).OrderBy(m => m.Timestamp).ToList();

            // 4. Token Limit Handling
            int currentTokens = merged.Count * TokensPerMessage;
            
            if (currentTokens > TokenLimit)
            {
                // Truncate oldest messages first, but prioritize keeping semantic ones.
                // We mark semantic messages as "protected".
                var semanticIds = semantic.Select(s => s.Id).ToHashSet();
                
                // Sort by timestamp (oldest first)
                var toTruncate = merged.OrderBy(m => m.Timestamp).ToList();
                
                foreach (var msg in toTruncate)
                {
                    if (currentTokens <= TokenLimit) break;
                    
                    // If it's not a semantic match, remove it.
                    if (!semanticIds.Contains(msg.Id))
                    {
                        merged.Remove(msg);
                        currentTokens -= TokensPerMessage;
                    }
                }
            }

            // 5. Format for LLM
            return string.Join("\n", merged.Select(m => $"[{m.Role}]: {m.Content}"));
        }

        // Interactive Challenge: Multi-Query Expansion
        public async Task<string> BuildContextWithExpansionAsync(Guid sessionId, string rawQuery, float[] originalEmbedding)
        {
            // 1. Generate Variations (Mocked)
            var variations = new List<string>
            {
                rawQuery,
                $"Summary of: {rawQuery}",
                $"Key points regarding: {rawQuery}"
            };

            // 2. Embed Variations (Mocked)
            // In reality, you'd call an embedding service 3 times.
            // Here we assume the embeddings are slightly different or identical for simulation.
            var embeddings = new List<float[]> { originalEmbedding, originalEmbedding, originalEmbedding };

            // 3. Perform Search for all
            var allCandidates = new List<MemoryEntry>();
            foreach (var emb in embeddings)
            {
                var results = await _store.RetrieveContextAsync(sessionId, emb, 5);
                allCandidates.AddRange(results);
            }

            // 4. Deduplicate by Id
            var uniqueCandidates = allCandidates.DistinctBy(x => x.Id).ToList();

            // 5. Re-rank based on similarity to the ORIGINAL query (or average)
            var ranked = uniqueCandidates
                .Select(m => new { Entry = m, Score = CalculateCosine(m.Embedding, originalEmbedding) })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Entry)
                .OrderBy(m => m.Timestamp) // Final output chronological
                .ToList();

            return string.Join("\n", ranked.Select(m => $"[{m.Role}]: {m.Content}"));
        }

        private double CalculateCosine(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0;
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }
    }
}
