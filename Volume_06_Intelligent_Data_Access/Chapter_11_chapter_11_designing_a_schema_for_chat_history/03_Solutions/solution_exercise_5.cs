
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
using Microsoft.EntityFrameworkCore.ChangeTracking;

// 1. Entity Updates for Concurrency and Soft Deletes
public class ChatMessage
{
    // ... existing properties ...
    
    // Concurrency Token
    [ConcurrencyCheck]
    public uint RowVersion { get; set; } // SQL Server uses 'rowversion' (binary) mapped to uint in EF

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class MessageEmbedding
{
    // ... existing properties ...
    
    // Soft Delete (Optional, but good for consistency)
    public bool IsDeleted { get; set; }
}

// 2. DbContext Configuration
public class IntegrityChatContext : DbContext
{
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MessageEmbedding> MessageEmbeddings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Concurrency Token Configuration
        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.RowVersion)
            .IsRowVersion(); // Maps to SQL Server 'rowversion' type automatically

        // Cascading Deletes (Database Level)
        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Embedding)
            .WithOne()
            .HasForeignKey<MessageEmbedding>(e => e.ChatMessageId)
            .OnDelete(DeleteBehavior.Cascade); // SQL Server will enforce this

        // Global Query Filters for Soft Deletes
        modelBuilder.Entity<ChatMessage>()
            .HasQueryFilter(m => !m.IsDeleted);
        
        modelBuilder.Entity<MessageEmbedding>()
            .HasQueryFilter(e => !e.IsDeleted);
    }

    // Override SaveChangesAsync to handle Soft Deletes automatically
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Intercept entities marked for deletion
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted && 
                       (e.Entity is ChatMessage || e.Entity is MessageEmbedding));

        foreach (var entry in entries)
        {
            // Switch to Soft Delete instead of physical delete
            entry.State = EntityState.Modified;
            
            if (entry.Entity is ChatMessage msg)
            {
                msg.IsDeleted = true;
                msg.DeletedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MessageEmbedding emb)
            {
                emb.IsDeleted = true;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

// 3. Repository Implementation
public class IntegrityRepository
{
    private readonly IntegrityChatContext _context;

    public IntegrityRepository(IntegrityChatContext context) => _context = context;

    // Update with Concurrency Handling
    public async Task UpdateMessageWithEmbeddingAsync(Guid messageId, string newContent, float[] newVector)
    {
        bool retry = false;
        do
        {
            try
            {
                var message = await _context.ChatMessages
                    .Include(m => m.Embedding)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null) return;

                // Update content
                message.Content = newContent;
                
                // Update vector
                if (message.Embedding != null)
                {
                    message.Embedding.Vector = newVector; // Uses the setter to update JSON
                }

                await _context.SaveChangesAsync();
                retry = false;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log conflict
                Console.WriteLine($"Concurrency conflict on message {messageId}. Retrying...");
                
                // Optimistic Concurrency: Reload values and retry
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync();
                }
                retry = true;
            }
        } while (retry);
    }

    // Hard Delete for Soft-Deleted items
    public async Task HardDeleteOldMessagesAsync(DateTime cutoffDate)
    {
        // We must temporarily disable the query filter to find soft-deleted items
        var messages = await _context.ChatMessages
            .IgnoreQueryFilters() 
            .Where(m => m.IsDeleted && m.DeletedAt < cutoffDate)
            .ToListAsync();

        var embeddings = await _context.MessageEmbeddings
            .IgnoreQueryFilters()
            .Where(e => e.IsDeleted && _context.ChatMessages
                .Any(m => m.Id == e.ChatMessageId && m.DeletedAt < cutoffDate))
            .ToListAsync();

        // Perform physical deletion
        _context.MessageEmbeddings.RemoveRange(embeddings);
        _context.ChatMessages.RemoveRange(messages);

        await _context.SaveChangesAsync();
    }
}
