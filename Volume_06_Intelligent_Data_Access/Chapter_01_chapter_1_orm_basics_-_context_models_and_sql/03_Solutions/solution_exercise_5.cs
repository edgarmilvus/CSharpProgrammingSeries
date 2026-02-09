
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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class IntelligentContext : DbContext
{
    // Channel for batching vector updates
    private static readonly Channel<VectorUpdateJob> _updateChannel = Channel.CreateUnbounded<VectorUpdateJob>();
    private readonly List<VectorUpdateJob> _batchBuffer = new();

    public IntelligentContext(DbContextOptions options) : base(options) { }
    public DbSet<Document> Documents { get; set; }

    // 1. Override SaveChangesAsync
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 2. Detect changes using EntityEntry extensions
        var changes = ChangeTracker.Entries<Document>()
            .Where(e => e.State == EntityState.Modified && e.HasChangedContent())
            .Select(e => new VectorUpdateJob
            {
                DocumentId = e.Entity.Id,
                NewContent = e.Entity.Content,
                Priority = CalculatePriority(e)
            })
            .ToList();

        // 3. Batch Processing System
        // We don't update immediately. We buffer and flush to the channel.
        _batchBuffer.AddRange(changes);

        // Flush if batch size reached (e.g., 50)
        if (_batchBuffer.Count >= 50)
        {
            await FlushBatchAsync();
        }

        // Perform the standard relational save
        return await base.SaveChangesAsync(cancellationToken);
    }

    // 4. Queue-based system using System.Threading.Channels
    public async Task StartBackgroundProcessor(CancellationToken stoppingToken)
    {
        // This would typically run as a background service
        await foreach (var job in _updateChannel.Reader.ReadAllAsync(stoppingToken))
        {
            // Circuit Breaker & Retry Logic would wrap this call
            try
            {
                await ProcessVectorUpdateWithRetry(job);
            }
            catch (Exception ex)
            {
                // Log failure, maybe re-queue with delay
            }
        }
    }

    private async Task FlushBatchAsync()
    {
        foreach (var job in _batchBuffer)
        {
            await _updateChannel.Writer.WriteAsync(job);
        }
        _batchBuffer.Clear();
    }

    private async Task ProcessVectorUpdateWithRetry(VectorUpdateJob job)
    {
        // Circuit Breaker Pattern (simplified)
        if (IsCircuitOpen()) return; 

        int retries = 0;
        while (retries < 3)
        {
            try
            {
                // Simulate expensive embedding generation
                var embedding = await GenerateEmbeddingAsync(job.NewContent);
                
                // Update the database (using a fresh context to avoid tracking issues)
                using var scope = this.GetInfrastructure().CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<IntelligentContext>();
                
                var doc = await db.Documents.FindAsync(job.DocumentId);
                if (doc != null)
                {
                    doc.Vector = embedding;
                    doc.LastVectorUpdate = DateTime.UtcNow;
                    await db.SaveChangesAsync(); // Direct update
                }
                return; // Success
            }
            catch (Exception)
            {
                retries++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retries))); // Exponential backoff
            }
        }
        
        // Circuit Breaker logic here...
        TripCircuitBreaker();
    }

    // Helper to check content changes
    private async Task<float[]> GenerateEmbeddingAsync(string content) 
        => await Task.FromResult(new float[] { 0.1f }); // Mock

    private int CalculatePriority(EntityEntry<Document> entry) => 1;
    private bool IsCircuitOpen() => false; // Mock
    private void TripCircuitBreaker() { /* Mock */ }
}

// Extension Method for EntityEntry
public static class EntityEntryExtensions
{
    public static bool HasChangedContent(this EntityEntry<Document> entry)
    {
        // Check if the Content property is modified
        return entry.Property(d => d.Content).IsModified;
    }
}

public class VectorUpdateJob
{
    public int DocumentId { get; set; }
    public string NewContent { get; set; }
    public int Priority { get; set; }
}
