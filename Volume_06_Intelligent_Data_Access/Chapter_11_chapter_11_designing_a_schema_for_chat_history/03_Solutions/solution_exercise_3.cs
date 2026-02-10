
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

using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// 1. Schema Refactoring & Tagging Entities
public class Conversation
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    
    // JSON Metadata Column
    public string MetadataJson { get; set; } = "{}";
    
    [NotMapped]
    public Dictionary<string, string> Metadata
    {
        get => JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson) ?? new();
        set => MetadataJson = JsonSerializer.Serialize(value);
    }
    
    public ICollection<ChatMessage> Messages { get; set; }
}

// Tagging System Entities
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } // e.g., "InternalUseOnly", "Billing"
    public ICollection<MessageTag> MessageTags { get; set; }
}

public class MessageTag
{
    public Guid ChatMessageId { get; set; }
    public Guid TagId { get; set; }
    public ChatMessage ChatMessage { get; set; }
    public Tag Tag { get; set; }
}

// Update ChatMessage to include Tags
public class ChatMessage
{
    // ... existing properties ...
    public ICollection<MessageTag> MessageTags { get; set; } = new List<MessageTag>();
}

// 2. DbContext Configuration
public class OptimizedChatContext : DbContext
{
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Conversation Metadata Configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            // Map MetadataJson to a JSON column type
            // Note: In SQL Server, we use a string but treat it as JSON in migrations
            entity.Property(e => e.MetadataJson)
                  .HasColumnType("nvarchar(max)"); // Stores JSON text

            // Interactive Challenge: Computed Column Index
            // We create a computed column that extracts a specific key from the JSON.
            // This requires Raw SQL in the migration, but we define the model here.
            // SQL Server Syntax: CAST(JSON_VALUE(MetadataJson, '$.Department') AS nvarchar(100))
            
            // EF Core 8 allows defining computed columns via Fluent API:
            entity.Property(e => e.Department)
                  .HasComputedColumnSql("JSON_VALUE(MetadataJson, '$.Department')");
            
            // We then index this computed column
            entity.HasIndex(e => e.Department);
        });

        // Tagging Configuration (Many-to-Many)
        modelBuilder.Entity<MessageTag>()
            .HasKey(mt => new { mt.ChatMessageId, mt.TagId });

        modelBuilder.Entity<MessageTag>()
            .HasOne(mt => mt.ChatMessage)
            .WithMany(m => m.MessageTags)
            .HasForeignKey(mt => mt.ChatMessageId);

        modelBuilder.Entity<MessageTag>()
            .HasOne(mt => mt.Tag)
            .WithMany(t => t.MessageTags)
            .HasForeignKey(mt => mt.TagId);
    }
}

// 3. Query Implementation
public class MetadataRepository
{
    private readonly OptimizedChatContext _context;

    public MetadataRepository(OptimizedChatContext context) => _context = context;

    // Filter by Metadata Key-Value
    public async Task<List<Conversation>> GetConversationsByDepartmentAsync(string department)
    {
        // The JSON query is translated to SQL JSON_VALUE by EF Core
        return await _context.Conversations
            .Where(c => c.Metadata["Department"] == department)
            .ToListAsync();
    }

    // Interactive Challenge: Filtering by Tags
    public async Task<List<ChatMessage>> GetContextExcludingTagsAsync(Guid conversationId, List<string> excludedTags)
    {
        return await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            // Ensure the message does NOT have any tag matching the excluded list
            .Where(m => !m.MessageTags.Any(mt => excludedTags.Contains(mt.Tag.Name)))
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }
}
