
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

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// 1. Updated Entity with Concurrency Token
public class Conversation
{
    [Key]
    public Guid Id { get; set; }
    public string Content { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}

public interface IVectorStoreClient
{
    Task UpsertAsync(Guid id, float[] vector, string metadata);
    Task<(float[] vector, string metadata)> GetAsync(Guid id);
}

// 2. Hybrid Update Service
public class ConversationUpdateService
{
    private readonly AppDbContext _context;
    private readonly IVectorStoreClient _vectorStore;

    public ConversationUpdateService(AppDbContext context, IVectorStoreClient vectorStore)
    {
        _context = context;
        _vectorStore = vectorStore;
    }

    public async Task UpdateConversationAsync(Guid id, string newContent, float[] newVector)
    {
        // Fetch the existing entity to get the current RowVersion
        var conversation = await _context.Conversations.FindAsync(id);
        if (conversation == null) throw new ArgumentException("Conversation not found");

        // Update the content
        conversation.Content = newContent;

        try
        {
            // 1. Attempt Relational Update
            // EF Core will include the RowVersion in the WHERE clause of the UPDATE statement.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 2. Handle Relational Conflict
            // This occurs if another user modified the record between our read and write.
            var entry = ex.Entries.Single();
            var currentValues = entry.GetDatabaseValues();
            
            // Implement "Last-Write-Wins" (Overwrite DB with current values) or throw custom exception
            throw new ConcurrencyConflictException(
                $"Conflict detected for Conversation {id}. " +
                $"Current DB Value: {currentValues.GetValue<string>("Content")}"
            );
        }

        // 3. Conditional Vector Update (Only executed if Relational Update succeeded)
        try
        {
            // Retrieve current vector metadata to check version
            var (existingVector, metadata) = await _vectorStore.GetAsync(id);
            
            // Parse metadata to check version (Simulated logic)
            // In reality, metadata might be a JSON string containing { "RowVersion": "Base64String" }
            string currentDbVersionBase64 = Convert.ToBase64String(conversation.RowVersion);
            
            // Check if the vector store has the same version as the DB *after* the update
            // Note: This is a check against the state we just wrote to DB, 
            // or we could check if the vector store was updated by another process concurrently.
            if (metadata != null && metadata.Contains(currentDbVersionBase64))
            {
                Console.WriteLine("Vector store is up to date.");
                return;
            }

            // Update Vector Store with the NEW RowVersion as metadata
            string newMetadata = $"RowVersion:{currentDbVersionBase64}";
            await _vectorStore.UpsertAsync(id, newVector, newMetadata);
        }
        catch (Exception ex)
        {
            // 4. Vector Store Failure Handling
            // Since the SQL DB is already committed, we cannot rollback automatically.
            // We must rely on a compensating action or alert the user.
            Console.WriteLine($"Warning: SQL updated, but Vector Store update failed: {ex.Message}");
            // In a real system, queue a background job to retry the vector update.
            throw;
        }
    }
}

// Custom Exception for Conflict Resolution
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message) { }
}
