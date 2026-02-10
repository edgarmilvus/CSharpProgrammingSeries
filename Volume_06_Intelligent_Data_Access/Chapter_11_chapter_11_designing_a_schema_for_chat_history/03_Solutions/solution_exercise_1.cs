
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
using System.Text.Json;

// 1. Entity Definitions
public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // System, User, Assistant
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Threading Support (Interactive Challenge)
    public Guid? ParentMessageId { get; set; }
    public ChatMessage? ParentMessage { get; set; }
    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();

    // Vector Relationship
    public MessageEmbedding? Embedding { get; set; }
}

public class MessageEmbedding
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatMessageId { get; set; }
    
    // Stored as JSON string
    public string VectorJson { get; set; } = "[]";
    
    // Helper property for C# logic (not mapped to DB directly)
    [NotMapped]
    public float[] Vector 
    {
        get => JsonSerializer.Deserialize<float[]>(VectorJson) ?? Array.Empty<float>();
        set => VectorJson = JsonSerializer.Serialize(value);
    }
}

// 2. DbContext & Configuration
public class ChatContext : DbContext
{
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MessageEmbedding> MessageEmbeddings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Using SQL Server
        optionsBuilder.UseSqlServer("Server=.;Database=ChatDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Conversation Configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(c => c.Messages)
                  .WithOne()
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade); // Cascading delete
            
            // Index for temporal queries
            entity.HasIndex(e => e.CreatedAt);
        });

        // ChatMessage Configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Index for Timestamp
            entity.HasIndex(e => e.Timestamp);

            // Self-referencing relationship for threading (Interactive Challenge)
            entity.HasMany(m => m.Replies)
                  .WithOne(m => m.ParentMessage)
                  .HasForeignKey(m => m.ParentMessageId)
                  .OnDelete(DeleteBehavior.ClientCascade); // Prevent cycles in SQL, handled by app logic

            // One-to-one with Embedding
            entity.HasOne(m => m.Embedding)
                  .WithOne()
                  .HasForeignKey<MessageEmbedding>(e => e.ChatMessageId)
                  .OnDelete(DeleteBehavior.Cascade); // Ensure vector deleted if message deleted
        });

        // MessageEmbedding Configuration
        modelBuilder.Entity<MessageEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            // VectorJson is stored as a string, no special SQL config needed for JSON string
        });
    }
}

// 3. Repository for Hybrid Querying
public class ChatRepository
{
    private readonly ChatContext _context;

    public ChatRepository(ChatContext context) => _context = context;

    // Requirement 3: Retrieve last 5 messages
    public async Task<List<ChatMessage>> GetLastMessagesAsync(Guid conversationId)
    {
        return await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.Timestamp)
            .Take(5)
            .ToListAsync();
    }

    // Interactive Challenge: Recursive CTE Query
    // Note: EF Core 8+ supports raw SQL mapping to entities efficiently.
    public async Task<List<ChatMessage>> GetFullThreadAsync(Guid rootMessageId)
    {
        // SQL Server Recursive CTE to get the entire tree under a specific message
        var sql = @"
            WITH MessageThread AS (
                -- Anchor member: the root message
                SELECT Id, ConversationId, Role, Content, Timestamp, ParentMessageId
                FROM ChatMessages
                WHERE Id = @p0
                
                UNION ALL
                
                -- Recursive member: replies to the current message
                SELECT m.Id, m.Role, m.Content, m.Timestamp, m.ParentMessageId
                FROM ChatMessages m
                INNER JOIN MessageThread mt ON m.ParentMessageId = mt.Id
            )
            SELECT * FROM MessageThread
            ORDER BY Timestamp ASC;";

        // Map raw SQL results to the ChatMessage entity
        return await _context.ChatMessages
            .FromSqlRaw(sql, rootMessageId)
            .ToListAsync();
    }
}
