
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

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Polly.Retry;
using System.ComponentModel.DataAnnotations;

public class MaintenanceDocument
{
    [Key]
    public Guid Id { get; set; }
    public float[] Embedding { get; set; }
    public bool IsDeleted { get; set; } // Soft delete flag
}

public class MaintenanceContext : DbContext
{
    public DbSet<MaintenanceDocument> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=maintenance_db;Username=postgres;Password=password");
    }
}

public class IndexMaintenanceService
{
    private readonly MaintenanceContext _context;
    // 4. Concurrency Handling: Polly Retry Policy
    private readonly AsyncRetryPolicy _retryPolicy;

    public IndexMaintenanceService(MaintenanceContext context)
    {
        _context = context;
        _retryPolicy = Policy
            .Handle<NpgsqlException>(ex => ex.IsTransient) // Handle transient DB errors
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                onRetry: (exception, timeSpan, retryCount, context) => 
                {
                    Console.WriteLine($"Retry {retryCount} due to {exception.Message}");
                });
    }

    // 1. Insertion Strategy
    public async Task AddDocumentAsync(float[] embedding)
    {
        var doc = new MaintenanceDocument 
        { 
            Id = Guid.NewGuid(), 
            Embedding = embedding,
            IsDeleted = false 
        };

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _context.Documents.AddAsync(doc);
            await _context.SaveChangesAsync();
        });
        
        // Impact: HNSW handles inserts efficiently by updating local graph links.
        // No global rebuild is needed (unlike IVFFlat, which might require re-clustering if lists are full).
    }

    // 2. Deletion Strategy (Soft Delete)
    public async Task DeleteDocumentAsync(Guid id)
    {
        // Hard deletes in HNSW are expensive because the graph links must be repaired.
        // Soft deletes are preferred.
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc != null)
            {
                doc.IsDeleted = true; // Mark as deleted
                await _context.SaveChangesAsync();
            }
        });
    }

    // 3. Index Rebuilding
    public async Task CheckAndRebuildIndexAsync()
    {
        // Simulate checking fragmentation (e.g., checking a system view or estimating)
        // In PostgreSQL, you might check `pg_stat_user_indexes` or simply schedule based on time/updates.
        bool isFragmented = await CheckFragmentationAsync();

        if (isFragmented)
        {
            Console.WriteLine("Index fragmented. Starting rebuild...");
            
            // Execute REINDEX CONCURRENTLY to avoid locking the table for writes
            await _context.Database.ExecuteSqlRawAsync("REINDEX INDEX CONCURRENTLY ix_document_embedding");
            
            Console.WriteLine("Rebuild complete.");
        }
    }

    private async Task<bool> CheckFragmentationAsync()
    {
        // Placeholder logic: Return true if > 10% of data changed
        // In reality, query pg_stat_user_indexes for idx_scan vs seq_scan ratio or dead tuples.
        return await Task.FromResult(true); 
    }
}

// 5. Visualization (Graphviz DOT representation)
/*
digraph IndexLifecycle {
    node [shape=rectangle];
    Start [label="Application Start"];
    Insert [label="Insert Vector (EF Core)"];
    HNSW_Update [label="HNSW Graph Update (Local)"];
    Delete [label="Soft Delete (Flag Only)"];
    Check [label="Cron Job: Check Health"];
    Reindex [label="REINDEX CONCURRENTLY"];
    Query [label="Query (Polly Retry)"];
    Error [label="Lock Timeout / Error"];

    Start -> Insert;
    Insert -> HNSW_Update;
    Start -> Delete;
    Delete -> Check;
    Check -> Reindex [label="If Fragmented"];
    Reindex -> Query;
    
    Query -> Error [label="Index Locked"];
    Error -> Query [label="Retry"];
}
*/
