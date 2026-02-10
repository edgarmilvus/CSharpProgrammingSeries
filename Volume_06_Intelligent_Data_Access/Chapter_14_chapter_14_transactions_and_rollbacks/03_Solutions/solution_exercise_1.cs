
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

// 1. Entity definition
public class Conversation
{
    public Guid Id { get; set; }
    public string UserQuery { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 2. EF Core Context
public class AppDbContext : DbContext
{
    public DbSet<Conversation> Conversations { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Optional: Override SaveChangesAsync to prevent direct usage outside UoW
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}

// 3. Vector Store Client Interface
public interface IVectorStoreClient
{
    Task UpsertAsync(Guid id, float[] vector, string metadata);
}

// 4. Unit of Work Interface
public interface IUnitOfWork
{
    AppDbContext Context { get; }
    IVectorStoreClient VectorStore { get; }
    Task<int> CommitAsync();
}

// 5. Concrete Implementation
public class ConversationUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private readonly IVectorStoreClient _vectorStore;
    private IDbContextTransaction _transaction;

    public AppDbContext Context => _context;
    public IVectorStoreClient VectorStore => _vectorStore;

    public ConversationUnitOfWork(AppDbContext context, IVectorStoreClient vectorStore)
    {
        _context = context;
        _vectorStore = vectorStore;
    }

    public async Task<int> CommitAsync()
    {
        // Start a relational transaction
        _transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Save changes to the relational database
            // Note: We do not commit the transaction yet, just stage the changes
            var affectedRows = await _context.SaveChangesAsync();

            // 2. Attempt to save to the vector store
            // In a real scenario, you would iterate over tracked entities here.
            // For this exercise, we simulate checking for pending Conversation entities.
            var pendingConversations = _context.ChangeTracker.Entries<Conversation>();
            
            foreach (var entry in pendingConversations)
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var entity = entry.Entity;
                    // Simulate vector generation (in reality, this might be complex logic)
                    float[] dummyVector = new float[] { 1.0f, 2.0f }; 
                    
                    // This is the critical point of potential failure
                    await _vectorStore.UpsertAsync(entity.Id, dummyVector, entity.UserQuery);
                }
            }

            // 3. If Vector Store succeeds, commit the relational transaction
            await _transaction.CommitAsync();
            return affectedRows;
        }
        catch (Exception ex)
        {
            // 4. Error Handling: Rollback if anything fails
            // Log the exception here (e.g., using ILogger)
            Console.WriteLine($"Error during commit: {ex.Message}");
            
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }

            // Re-throw to notify the caller of failure
            throw;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}

// --- Interactive Challenge: Cleanup Logic ---

/// <summary>
/// Service to handle orphaned records caused by race conditions or 
/// failures after the relational commit but before vector store completion.
/// </summary>
public class OrphanCleanupService
{
    private readonly AppDbContext _context;
    private readonly IVectorStoreClient _vectorStore;

    public OrphanCleanupService(AppDbContext context, IVectorStoreClient vectorStore)
    {
        _context = context;
        _vectorStore = vectorStore;
    }

    /// <summary>
    /// Pseudo-code for a background worker that scans for inconsistencies.
    /// </summary>
    public async Task ResolveOrphansAsync()
    {
        // 1. Identify potential orphans: Records in DB but missing in Vector Store
        // In production, this query would be optimized (e.g., batched) and might 
        // rely on a 'SyncStatus' flag rather than querying the vector store for every row.
        var conversations = await _context.Conversations
            .Where(c => c.CreatedAt >= DateTime.UtcNow.AddHours(-1)) // Recent records only
            .ToListAsync();

        foreach (var convo in conversations)
        {
            try
            {
                // 2. Check existence in Vector Store
                // (Assuming GetAsync returns null or throws if not found)
                var vectorData = await _vectorStore.GetAsync(convo.Id); 
                
                if (vectorData.vector == null)
                {
                    // 3. Compensating Action: Re-insert into Vector Store or Delete from DB
                    // Strategy: Retry insertion (Idempotent operation)
                    float[] vector = new float[] { 1.0f, 2.0f }; // Recalculate or retrieve cached vector
                    await _vectorStore.UpsertAsync(convo.Id, vector, convo.UserQuery);
                }
            }
            catch (Exception ex)
            {
                // Log failure to fix orphan
                Console.WriteLine($"Failed to resolve orphan {convo.Id}: {ex.Message}");
            }
        }
    }
}
