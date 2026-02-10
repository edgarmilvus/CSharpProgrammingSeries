
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// Required Entity
public class ChatRoom
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime LastActivity { get; set; }
    public byte[] RowVersion { get; set; } // Concurrency token
}

// DbContext
public class ChatContext : DbContext
{
    public DbSet<ChatRoom> Rooms { get; set; }
    public DbSet<ChatMessage> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatRoom>()
            .Property(r => r.RowVersion)
            .IsRowVersion();
    }
}

public class RoomService
{
    private readonly ChatContext _context;

    public RoomService(ChatContext context)
    {
        _context = context;
    }

    // 4. Update Room Activity using ExecuteUpdateAsync (EF Core 7+)
    public async Task UpdateRoomActivityAsync(Guid roomId)
    {
        // ExecuteUpdateAsync performs a direct SQL UPDATE without loading the entity.
        // This bypasses the change tracker, avoiding locks on the entity in the context
        // and reducing the duration of the transaction.
        
        // Note: ExecuteUpdateAsync handles concurrency tokens automatically in EF Core 8+.
        // In EF Core 7, we manually construct the predicate.
        
        try
        {
            await _context.Rooms
                .Where(r => r.Id == roomId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.LastActivity, DateTime.UtcNow)
                    // EF Core 8+ syntax for concurrency token handling in ExecuteUpdate:
                    // .SetProperty(r => r.RowVersion, r => r.RowVersion + 1) 
                    // If using EF Core 7, we rely on the WHERE clause matching the current state 
                    // or handle the concurrency check manually if strict token validation is needed.
                    // However, for a simple timestamp update, ExecuteUpdate is ideal.
                );
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle conflict if strict optimistic concurrency is required for this update.
            // Since ExecuteUpdate bypasses tracking, the exception is thrown if 0 rows are affected.
            throw;
        }
    }

    // Aggregation Method (Simulated)
    public async Task<int> AggregateMessageCountAsync(Guid roomId, System.Data.IsolationLevel isolationLevel)
    {
        // 3. Use specific isolation level for aggregation to avoid blocking writes
        using (var transaction = await _context.Database.BeginTransactionAsync(isolationLevel))
        {
            try
            {
                // Locking the ChatRoom row here would cause deadlocks with UpdateRoomActivityAsync
                // if UpdateRoomActivityAsync used a transaction that locked the row.
                // By using ExecuteUpdateAsync (which commits immediately) and ReadCommitted here,
                // we minimize the locking window.
                
                var count = await _context.Messages
                    .Where(m => m.RoomId == roomId) // Assuming RoomId exists on ChatMessage
                    .CountAsync();

                await transaction.CommitAsync();
                return count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

// 1. Simulate Deadlock Scenario (Test Harness)
public class DeadlockSimulator
{
    private readonly ChatContext _context;
    private readonly RoomService _roomService;

    public DeadlockSimulator(ChatContext context, RoomService roomService)
    {
        _context = context;
        _roomService = roomService;
    }

    public async Task Simulate()
    {
        var roomId = Guid.NewGuid();
        
        // Task A: User Activity Update (Fast, direct SQL)
        var taskA = Task.Run(async () => 
        {
            await _roomService.UpdateRoomActivityAsync(roomId);
            Console.WriteLine("User activity updated.");
        });

        // Task B: Aggregator (Read Committed)
        var taskB = Task.Run(async () => 
        {
            // Using ReadCommitted ensures we don't read uncommitted data,
            // but we don't hold locks longer than necessary.
            var count = await _roomService.AggregateMessageCountAsync(roomId, System.Data.IsolationLevel.ReadCommitted);
            Console.WriteLine($"Aggregated count: {count}");
        });

        await Task.WhenAll(taskA, taskB);
    }
}
