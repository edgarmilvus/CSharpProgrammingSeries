
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Concurrent;
using System.Text.Json;

// 1. Domain Entities
public record Guideline(string Id, string Content, List<float> Embedding);
public record Conversation(string Id, string UserId, string Message, DateTime Timestamp);

// 2. Vector Store Simulation (Mocking a Vector DB like Pinecone or Qdrant)
// In a real scenario, this would be an external HTTP client.
public class VectorStore
{
    private readonly ConcurrentDictionary<string, List<float>> _vectors = new();

    public Task UpsertAsync(Guideline guideline)
    {
        // Simulate network latency
        Thread.Sleep(50); 
        _vectors[guideline.Id] = guideline.Embedding;
        Console.WriteLine($"[Vector Store] Vector upserted for ID: {guideline.Id}");
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        Thread.Sleep(50);
        _vectors.TryRemove(id, out _);
        Console.WriteLine($"[Vector Store] Vector deleted for ID: {id}");
        return Task.CompletedTask;
    }

    // Simulates a check to see if the vector exists (for rollback verification)
    public bool Exists(string id) => _vectors.ContainsKey(id);
}

// 3. Relational Context (EF Core)
public class AppDbContext : DbContext
{
    public DbSet<Conversation> Conversations => Set<Conversation>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseInMemoryDatabase("ChatDb");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>().HasKey(c => c.Id);
    }
}

// 4. Unit of Work Implementation
// This coordinates the changes across the relational DB and Vector Store.
public class ChatUnitOfWork : IDisposable
{
    private readonly AppDbContext _relationalContext;
    private readonly VectorStore _vectorStore;
    
    // Transaction Log: Tracks operations performed during this UoW session
    // to enable rollback (compensating transactions).
    private readonly List<Func<Task>> _compensationActions = new();

    public ChatUnitOfWork(AppDbContext relationalContext, VectorStore vectorStore)
    {
        _relationalContext = relationalContext;
        _vectorStore = vectorStore;
    }

    public async Task<Guid> AddChatEntryAsync(string userId, string message, List<float> embedding)
    {
        var transactionId = Guid.NewGuid();
        var guidelineId = $"guideline-{transactionId}";

        try
        {
            // Step A: Relational Operation
            var conversation = new Conversation(
                Id: transactionId.ToString(),
                UserId: userId,
                Message: message,
                Timestamp: DateTime.UtcNow
            );

            await _relationalContext.Conversations.AddAsync(conversation);
            Console.WriteLine($"[Relational DB] Queued insert for Conversation: {conversation.Id}");

            // Register compensation: Delete the conversation if later steps fail
            _compensationActions.Add(async () => {
                var existing = await _relationalContext.Conversations.FindAsync(transactionId.ToString());
                if (existing != null)
                {
                    _relationalContext.Conversations.Remove(existing);
                    await _relationalContext.SaveChangesAsync(); // Save immediately for compensation
                    Console.WriteLine($"[Compensation] Rolled back Relational Insert: {transactionId}");
                }
            });

            // Step B: Vector Store Operation
            var guideline = new Guideline(guidelineId, message, embedding);
            await _vectorStore.UpsertAsync(guideline);

            // Register compensation: Delete the vector if later steps fail
            _compensationActions.Add(async () => {
                if (_vectorStore.Exists(guidelineId))
                {
                    await _vectorStore.DeleteAsync(guidelineId);
                    Console.WriteLine($"[Compensation] Rolled back Vector Upsert: {guidelineId}");
                }
            });

            // Step C: Commit Phase
            // In a real distributed system, this is where the 2PC (Two-Phase Commit) 
            // would attempt to lock resources. Here, we simulate the commit.
            await _relationalContext.SaveChangesAsync();
            
            Console.WriteLine(">>> TRANSACTION COMMITTED SUCCESSFULLY <<<");
            return transactionId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! ERROR DETECTED: {ex.Message}. Initiating Rollback... !!!");
            await RollbackAsync();
            throw; // Re-throw to notify the caller of failure
        }
    }

    private async Task RollbackAsync()
    {
        // Execute compensation actions in Reverse Order (LIFO)
        // This ensures we undo operations in the opposite order they were created.
        for (int i = _compensationActions.Count - 1; i >= 0; i--)
        {
            try
            {
                await _compensationActions[i].Invoke();
            }
            catch (Exception ex)
            {
                // Log critical error: Manual intervention might be required
                Console.WriteLine($"!!! CRITICAL: Compensation action {i} failed: {ex.Message} !!!");
            }
        }
        _compensationActions.Clear();
    }

    public void Dispose()
    {
        _relationalContext.Dispose();
    }
}

// 5. Main Execution
public class Program
{
    public static async Task Main()
    {
        // Setup
        using var relationalContext = new AppDbContext();
        await relationalContext.Database.EnsureCreatedAsync();
        
        var vectorStore = new VectorStore();
        var uow = new ChatUnitOfWork(relationalContext, vectorStore);

        // Scenario 1: Happy Path
        Console.WriteLine("--- SCENARIO 1: SUCCESSFUL TRANSACTION ---");
        var embedding1 = new List<float> { 0.1f, 0.9f, 0.5f }; // Mock vector
        await uow.AddChatEntryAsync("user-123", "Hello, AI!", embedding1);

        // Scenario 2: Simulated Failure (Vector Store throws exception)
        // We create a new UoW to isolate the failure test
        using var uowFail = new ChatUnitOfWork(new AppDbContext(), vectorStore);
        Console.WriteLine("\n--- SCENARIO 2: FAILED TRANSACTION (Simulated Vector DB Crash) ---");
        
        // Let's hack the vector store to throw an exception on the next call
        // (In a real app, this would be a network timeout or constraint violation)
        var originalUpsert = vectorStore.UpsertAsync;
        vectorStore.UpsertAsync = (g) => throw new InvalidOperationException("Vector Store Connection Lost");

        try
        {
            var embedding2 = new List<float> { 0.2f, 0.8f, 0.4f };
            await uowFail.AddChatEntryAsync("user-456", "This will fail", embedding2);
        }
        catch (Exception)
        {
            Console.WriteLine("Main caught the exception as expected.");
        }

        // Verification
        Console.WriteLine("\n--- VERIFICATION ---");
        Console.WriteLine($"Total Conversations in DB: {await relationalContext.Conversations.CountAsync()}");
        Console.WriteLine($"Total Vectors in Store: {vectorStore.Exists("guideline-" + Guid.Empty) ? 1 : 0} (Check specific IDs for accuracy)");
    }
}
