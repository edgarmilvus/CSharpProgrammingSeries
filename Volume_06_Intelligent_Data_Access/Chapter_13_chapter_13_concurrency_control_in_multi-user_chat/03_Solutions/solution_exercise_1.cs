
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Modified Entity Structure
public class ChatMessage
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Concurrency Token Property
    [Timestamp]
    public byte[] RowVersion { get; set; }
}

// 2. DbContext Configuration
public class ChatContext : DbContext
{
    public DbSet<ChatMessage> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Concurrency Token using Fluent API
        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.RowVersion)
            .IsRowVersion();
    }
}

// DTO for Conflict Result
public class ConflictResult
{
    public bool IsDeleted { get; set; }
    public string CurrentContent { get; set; }
    public byte[] CurrentRowVersion { get; set; }
    public string Message { get; set; }
}

// 3. Service Method
public class MessageService
{
    private readonly ChatContext _context;

    public MessageService(ChatContext context)
    {
        _context = context;
    }

    public async Task<ConflictResult> UpdateMessageAsync(Guid messageId, string newContent, byte[] currentRowVersion)
    {
        try
        {
            // Locate the message to update
            var message = await _context.Messages.FindAsync(messageId);
            
            if (message == null)
            {
                return new ConflictResult 
                { 
                    IsDeleted = true, 
                    Message = "Message not found. It may have been deleted." 
                };
            }

            // Apply changes
            message.Content = newContent;
            message.ModifiedAt = DateTime.UtcNow;
            
            // EF Core will automatically include RowVersion in the WHERE clause of the UPDATE statement
            // because it is configured as a concurrency token.
            await _context.SaveChangesAsync();
            
            return new ConflictResult { Message = "Update successful." };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 4. Handle Concurrency Conflict
            // 5. Handle Edge Case: Deleted Entity
            // Fetch the current values from the database to present to the user
            var databaseEntry = ex.Entries.Single();
            
            // Attempt to reload the entity from the database to get the current state
            // If the entity was deleted, reloading will return null.
            await databaseEntry.ReloadAsync();

            if (databaseEntry.State == EntityState.Detached)
            {
                // The entity no longer exists in the database
                return new ConflictResult
                {
                    IsDeleted = true,
                    Message = "The message was deleted by another user."
                };
            }

            // Entity exists but has been modified by another user
            var currentValues = databaseEntry.CurrentValues;
            return new ConflictResult
            {
                IsDeleted = false,
                CurrentContent = currentValues.GetValue<string>(nameof(ChatMessage.Content)),
                CurrentRowVersion = currentValues.GetValue<byte[]>(nameof(ChatMessage.RowVersion)),
                Message = "The message was modified by another user."
            };
        }
    }
}
