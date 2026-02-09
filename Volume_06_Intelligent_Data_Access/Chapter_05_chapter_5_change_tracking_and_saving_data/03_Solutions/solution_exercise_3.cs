
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

public class LogEntry
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    // 1. Concurrency Token
    [Timestamp] 
    public byte[]? Version { get; set; }
}

public class IngestionService
{
    // 2. Method IngestLogsAsync
    public async Task IngestLogsAsync(List<LogEntry> logs)
    {
        // 4. Wrap in TransactionScope (Distributed Transaction)
        // Note: Requires MSDTC enabled for SQL Server if spanning resources, 
        // but fine for single DB in this example.
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            using var context = new LogContext();
            
            // 3. Batching Mechanism (Loop every 500 records)
            const int batchSize = 500;
            for (int i = 0; i < logs.Count; i += batchSize)
            {
                var batch = logs.Skip(i).Take(batchSize).ToList();
                
                foreach (var log in batch)
                {
                    context.LogEntries.Add(log);
                }

                try
                {
                    // 6. Simulate Race Condition
                    // In a real app, this happens externally. Here we spawn a task.
                    if (i == 0) // Trigger once
                    {
                        _ = Task.Run(async () => 
                        {
                            using var raceContext = new LogContext();
                            var firstLog = await raceContext.LogEntries.FirstAsync();
                            firstLog.Message += " [Modified by Race]";
                            await raceContext.SaveChangesAsync();
                            Console.WriteLine("Race condition simulated: DB updated.");
                        }).Wait(100); // Small delay to let it run
                    }

                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // 7. Retry Logic
                    Console.WriteLine("Concurrency conflict detected. Retrying...");
                    await HandleConcurrencyConflict(ex, context);
                    // After handling, we might want to retry the save for this batch
                    // For simplicity, we assume HandleConcurrencyConflict fixes the state
                    await context.SaveChangesAsync();
                }
            }

            scope.Complete();
        }
    }

    private async Task HandleConcurrencyConflict(DbUpdateConcurrencyException ex, DbContext context)
    {
        // 7. Inspect entries
        foreach (var entry in ex.Entries)
        {
            // Get values from DB (the conflicting values)
            var databaseValues = await entry.GetDatabaseValuesAsync();
            
            // Get values from User A (the attempted save)
            var clientValues = entry.CurrentValues;
            
            // Strategy: Merge (Append conflict note)
            if (entry.Entity is LogEntry log)
            {
                // We decide to keep the client's message but append a warning
                // Note: We need to refresh the entity to avoid tracking issues
                var dbLog = databaseValues.ToObject() as LogEntry;
                
                // Merge logic
                log.Message = $"{clientValues.GetValue<string>("Message")} (Merged with DB version)";
                
                // Update the concurrency token to the DB value to allow the save
                entry.OriginalValues.SetValues(databaseValues);
            }
        }
    }
}

public class LogContext : DbContext
{
    public DbSet<LogEntry> LogEntries { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("YourConnectionStringHere");
}
