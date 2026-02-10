
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

// Source File: solution_exercise_7.cs
// Description: Solution for Exercise 7
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Vector Operation Log (Outbox Pattern)
public class VectorOperationLog
{
    public Guid Id { get; set; }
    public string OperationType { get; set; } // "Upsert", "Delete"
    public string Collection { get; set; }
    public string Payload { get; set; } // JSON serialized VectorRecord
    public bool IsProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SagaDbContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<VectorOperationLog> VectorOperationLogs { get; set; }

    public SagaDbContext(DbContextOptions options) : base(options) { }

    // 2. Saga Orchestrator Logic
    public async Task<Guid> CommitWithSagaAsync(Document doc)
    {
        // Phase 1: Start Transaction for Relational DB
        using var transaction = await Database.BeginTransactionAsync();
        try
        {
            // Add/Update Document
            Documents.Add(doc);
            await SaveChangesAsync();

            // Phase 2: Log Vector Operation (Prepare Phase)
            // Instead of calling Vector DB directly, we log it.
            var logEntry = new VectorOperationLog
            {
                Id = Guid.NewGuid(),
                OperationType = "Upsert",
                Collection = "Documents",
                Payload = System.Text.Json.JsonSerializer.Serialize(doc),
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };
            
            VectorOperationLogs.Add(logEntry);
            await SaveChangesAsync();

            // Commit Relational Transaction
            await transaction.CommitAsync();

            // Phase 3: Attempt Vector Commit (Post-Commit)
            // We return the ID so a background worker can pick it up, 
            // OR we try it immediately with a timeout.
            return logEntry.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // 3. Compensating Transaction
    public async Task<bool> CompensateAsync(Guid logId)
    {
        // This runs if the Vector DB commit failed after SQL commit
        var log = await VectorOperationLogs.FindAsync(logId);
        if (log == null || log.IsProcessed) return false;

        // Logic to revert the SQL state if possible, 
        // OR mark the log for manual review/retry.
        // For this exercise, we will assume we delete the document 
        // if the vector part failed (strict consistency).
        
        // Deserialize to find the ID
        var doc = System.Text.Json.JsonSerializer.Deserialize<Document>(log.Payload);
        
        var entity = await Documents.FindAsync(doc.Id);
        if (entity != null)
        {
            Documents.Remove(entity);
            await SaveChangesAsync();
        }

        // Mark log as compensated
        log.IsProcessed = true; 
        await SaveChangesAsync();

        return true;
    }
}

// 4. Background Worker for Vector Commit
public class VectorCommitWorker
{
    private readonly SagaDbContext _context;
    private readonly IVectorDatabaseClient _vectorDb;

    public async Task ProcessPendingOperations()
    {
        var pendingLogs = await _context.VectorOperationLogs
            .Where(l => !l.IsProcessed)
            .ToListAsync();

        foreach (var log in pendingLogs)
        {
            try
            {
                // 5. TransactionScope (Simulated)
                // Ideally, we would use TransactionScope with a custom resource manager,
                // but since Vector DBs often don't support 2PC, we do best-effort.
                
                var record = System.Text.Json.JsonSerializer.Deserialize<VectorRecord>(log.Payload);
                await _vectorDb.UpsertAsync(log.Collection, record);

                // Success: Mark processed
                log.IsProcessed = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Failure: Trigger Compensating Transaction
                // In a real system, we might alert ops, or retry N times first.
                await _context.CompensateAsync(log.Id);
            }
        }
    }
}

// Mocks
public class VectorRecord { }
public interface IVectorDatabaseClient { Task UpsertAsync(string collection, VectorRecord record); }
