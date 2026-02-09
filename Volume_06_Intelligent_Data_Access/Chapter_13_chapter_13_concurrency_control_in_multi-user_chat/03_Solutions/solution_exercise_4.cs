
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

// Reference class from previous subsection (Refactored)
public class RAGQueryService
{
    private readonly ChatContext _context;
    private readonly IVectorSearchEngine _vectorEngine;

    public RAGQueryService(ChatContext context, IVectorSearchEngine vectorEngine)
    {
        _context = context;
        _vectorEngine = vectorEngine;
    }

    public async Task<RAGResponse> RetrieveContext(string query, Guid chatId)
    {
        // 2. Implement ReadCommitted isolation for vector similarity searches
        // Using an explicit transaction ensures the isolation level is respected for the entire scope.
        using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted))
        {
            try
            {
                // 3. Use AsNoTracking for read-heavy queries to minimize memory and locking
                var messages = await _context.Messages
                    .AsNoTracking() 
                    .Where(m => m.ChatId == chatId) // Assuming ChatId exists on ChatMessage
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Simulated vector search (assuming this runs external to EF or uses a specific provider)
                var vectors = await _vectorEngine.Search(query);

                await transaction.CommitAsync();

                return new RAGResponse { Context = messages, Vectors = vectors };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    // 4 & 5. Snapshot Isolation Pattern
    public async Task<RAGSnapshot> GetRAGSnapshotAsync(DateTime snapshotTime)
    {
        // Note: Snapshot isolation is specific to SQL Server. 
        // It prevents dirty reads and reads phantom rows by using row versioning.
        // In SQL Server, this requires enabling ALLOW_SNAPSHOT_ISOLATION on the database.
        
        using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Snapshot))
        {
            try
            {
                // Retrieve chat history as it existed at snapshotTime
                // This assumes a temporal table or manual versioning logic. 
                // For this exercise, we simulate it by filtering valid_from/valid_to if temporal tables are used,
                // or simply querying the current state within the snapshot transaction (which sees data as of the transaction start).
                
                // To truly get data "as of" a specific time in SQL Server, we would use:
                // SELECT * FROM Messages FOR SYSTEM_TIME AS OF @snapshotTime
                
                // Simulating the EF Core query for a temporal table (EF Core 6+ supports this):
                var historicalMessages = await _context.Messages
                    .TemporalAsOf(snapshotTime) 
                    .AsNoTracking()
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                // Retrieve vectors (assuming a similar temporal setup or just current state if vectors are immutable)
                var vectors = await _context.Vectors
                    .AsNoTracking()
                    .ToListAsync();

                await transaction.CommitAsync();

                return new RAGSnapshot
                {
                    AsOfTime = snapshotTime,
                    Messages = historicalMessages,
                    Vectors = vectors
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to retrieve RAG snapshot.", ex);
            }
        }
    }
}

// Supporting DTOs
public class RAGResponse
{
    public object Context { get; set; } // In reality, List<ChatMessage>
    public object Vectors { get; set; }
}

public class RAGSnapshot
{
    public DateTime AsOfTime { get; set; }
    public object Messages { get; set; }
    public object Vectors { get; set; }
}

public interface IVectorSearchEngine
{
    Task<object> Search(string query);
}
