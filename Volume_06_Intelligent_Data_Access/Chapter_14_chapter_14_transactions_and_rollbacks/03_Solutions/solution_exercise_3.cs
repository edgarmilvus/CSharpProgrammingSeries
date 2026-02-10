
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain Models
public class ConversationLog { public Guid Id { get; set; } public string Data { get; set; } }
public class MemoryContext { public Guid Id { get; set; } public string Content { get; set; } public string Status { get; set; } } // Status: Pending, Committed

// Repository Interfaces
public interface IRelationalRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task<bool> ExistsAsync(Guid id);
    Task SaveChangesAsync();
}

public interface IMemoryStoreRepository<T> where T : class
{
    Task AddAsync(T entity); // Simulates "Pending" state
    Task UpdateAsync(T entity); // Simulates status update
    Task<T> GetAsync(Guid id);
    Task DeleteAsync(Guid id);
}

// Concrete Implementation of the Service
public class MemoryPersistenceService
{
    private readonly IRelationalRepository<ConversationLog> _relationalRepo;
    private readonly IMemoryStoreRepository<MemoryContext> _memoryRepo;

    public MemoryPersistenceService(
        IRelationalRepository<ConversationLog> relationalRepo, 
        IMemoryStoreRepository<MemoryContext> memoryRepo)
    {
        _relationalRepo = relationalRepo;
        _memoryRepo = memoryRepo;
    }

    public async Task SaveMemoryAsync(ConversationLog log, MemoryContext context)
    {
        // Phase 1: Prepare (Save to Memory Store as Pending)
        try
        {
            context.Status = "Pending";
            await _memoryRepo.AddAsync(context);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Phase 1 failed: Could not prepare memory store.", ex);
        }

        try
        {
            // Phase 2: Commit (Save to Relational DB)
            // Map domain object to relational entity
            var relationalLog = new ConversationLog { Id = log.Id, Data = log.Data };
            await _relationalRepo.AddAsync(relationalLog);
            await _relationalRepo.SaveChangesAsync();

            // Phase 3: Finalize (Update Memory Store status to Committed)
            context.Status = "Committed";
            await _memoryRepo.UpdateAsync(context);
        }
        catch (Exception ex)
        {
            // Phase 2 Failed: Rollback Phase 1
            Console.WriteLine($"Phase 2 failed: {ex.Message}. Rolling back Phase 1...");
            await RollbackPendingAsync(context.Id);
            throw;
        }
    }

    private async Task RollbackPendingAsync(Guid contextId)
    {
        try
        {
            await _memoryRepo.DeleteAsync(contextId);
        }
        catch (Exception ex)
        {
            // Log critical failure: We have a dangling "Pending" record
            Console.WriteLine($"CRITICAL: Failed to rollback memory store for ID {contextId}. Error: {ex.Message}");
            // In production, alert monitoring system
        }
    }

    // --- Recovery Logic ---

    /// <summary>
    /// Scans for "Pending" entries and resolves inconsistencies.
    /// </summary>
    public async Task RecoverPendingStates()
    {
        // 1. In a real scenario, query the Memory Store for all items where Status == "Pending"
        // For this simulation, we assume we have a method to retrieve them.
        // var pendingItems = await _memoryRepo.GetPendingAsync(); 
        
        // Mocking a pending item for demonstration
        var pendingItem = new MemoryContext { Id = Guid.NewGuid(), Status = "Pending" }; 

        // 2. Verify existence in Relational DB
        bool existsInRelational = await _relationalRepo.ExistsAsync(pendingItem.Id);

        if (existsInRelational)
        {
            // 3. Resolve: If exists in DB, it means Phase 3 failed. Finalize now.
            Console.WriteLine($"Recovery: Found pending item {pendingItem.Id} that exists in DB. Finalizing...");
            pendingItem.Status = "Committed";
            await _memoryRepo.UpdateAsync(pendingItem);
        }
        else
        {
            // 4. Resolve: If not in DB, Phase 2 failed. Cleanup.
            Console.WriteLine($"Recovery: Found pending item {pendingItem.Id} missing from DB. Cleaning up...");
            await _memoryRepo.DeleteAsync(pendingItem.Id);
        }
    }
}
