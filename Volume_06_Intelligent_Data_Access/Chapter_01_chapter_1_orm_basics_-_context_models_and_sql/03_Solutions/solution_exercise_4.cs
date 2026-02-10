
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
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
*/

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// 1. Vector Database Abstraction
public interface IVectorDatabaseClient
{
    Task UpsertAsync(string collection, VectorRecord record, CancellationToken token = default);
    Task DeleteAsync(string collection, string id, CancellationToken token = default);
}

public class VectorRecord 
{
    public string Id { get; set; }
    public float[] Embedding { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

// 2. Custom Execution Strategy for Vector DB
public class VectorRetryExecutionStrategy : IExecutionStrategy
{
    private readonly IExecutionStrategy _innerStrategy;
    private readonly int _maxRetries;

    public VectorRetryExecutionStrategy(IExecutionStrategy innerStrategy, int maxRetries = 3)
    {
        _innerStrategy = innerStrategy;
        _maxRetries = maxRetries;
    }

    public bool RetriesOnFailure => true; // Enable retries

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (attempt < _maxRetries && IsTransientVectorError(ex))
            {
                attempt++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
            }
        }
    }

    private bool IsTransientVectorError(Exception ex) 
        => ex is TimeoutException || ex is ConnectionException; // Mock types

    // Synchronous version omitted for brevity
    public T Execute<T>(Func<CancellationToken, T> operation, CancellationToken cancellationToken) 
        => throw new NotImplementedException();
}

// 3. Hybrid DbContext
public class HybridDbContext : DbContext
{
    private readonly IVectorDatabaseClient _vectorDb;
    private readonly List<VectorRecord> _pendingVectorOperations = new();

    public DbSet<Document> Documents { get; set; }

    public HybridDbContext(DbContextOptions options, IVectorDatabaseClient vectorDb) 
        : base(options)
    {
        _vectorDb = vectorDb;
    }

    // 4. Change Tracker for Vector Updates
    public override int SaveChanges()
    {
        // Pre-process: Detect text changes to queue vector updates
        TrackVectorChanges();
        
        // Execute SQL Transaction
        var result = base.SaveChanges();

        // Post-process: Execute Vector Operations
        // Note: In a true distributed transaction, we would use a 2PC or Saga pattern here.
        // For this exercise, we attempt the vector update after the relational commit.
        ProcessVectorUpdates().GetAwaiter().GetResult();

        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TrackVectorChanges();
        
        // Use the custom execution strategy for the relational part
        var strategy = new VectorRetryExecutionStrategy(new ExecutionStrategy(this));
        
        // We wrap the whole operation to ensure atomicity logic
        return await strategy.ExecuteAsync(async (ct) =>
        {
            var relationalResult = await base.SaveChangesAsync(ct);
            
            // Asynchronous Vector Indexing
            // We fire and forget but log to a recovery table if we want strict consistency
            await ProcessVectorUpdates(ct);
            
            return relationalResult;
        }, cancellationToken);
    }

    private void TrackVectorChanges()
    {
        foreach (var entry in ChangeTracker.Entries<Document>())
        {
            if (entry.State == EntityState.Added || 
                (entry.State == EntityState.Modified && entry.Property(d => d.Content).IsModified))
            {
                // Queue for vector update
                var entity = entry.Entity;
                _pendingVectorOperations.Add(new VectorRecord
                {
                    Id = entity.Id.ToString(),
                    Embedding = entity.Vector, // Assume this is calculated elsewhere or injected
                    Metadata = new Dictionary<string, object> { { "Title", entity.Title } }
                });
            }
        }
    }

    private async Task ProcessVectorUpdates(CancellationToken ct = default)
    {
        foreach (var op in _pendingVectorOperations)
        {
            // This calls the vector DB with retry logic
            await _vectorDb.UpsertAsync("documents", op, ct);
        }
        _pendingVectorOperations.Clear();
    }
}

// Mocks
public class ConnectionException : Exception { }
