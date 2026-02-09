
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Problem: A customer support chatbot needs to remember the conversation history 
// across multiple interactions. Without a proper schema, chat history becomes 
// unstructured text blobs that are impossible to query, filter, or analyze.
// Solution: We design a relational schema using EF Core that stores messages 
// in threads with metadata, enabling efficient retrieval and context management.

// Define the core entity representing a single message in a conversation
public class ChatMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Foreign key to the conversation thread
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    // Distinguish between user and AI messages
    [Required]
    [MaxLength(10)]
    public string Role { get; set; } = "User"; // "User", "Assistant", "System"

    // The actual content of the message
    [Required]
    public string Content { get; set; } = string.Empty;

    // Timestamp for ordering and temporal queries
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Metadata for filtering (e.g., department, intent, sentiment)
    // Using JSON serialization for flexibility in a relational schema
    public string? MetadataJson { get; set; }

    // Optional: Vector embedding for semantic search (RAG integration)
    // Stored as a string for SQLite compatibility; use float[] for PostgreSQL/SQL Server
    public string? EmbeddingVector { get; set; }
}

// Define the conversation thread entity
public class Conversation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Optional: Link to a user session or customer ID
    public string? SessionId { get; set; }

    // Timestamp of the last activity for cleanup/retention policies
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    // Summary or title for quick reference
    public string? Summary { get; set; }
}

// Define the EF Core DbContext
public class ChatContext : DbContext
{
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Using SQLite for a self-contained, file-based example
        // In production, use PostgreSQL with pgvector or SQL Server with vector extensions
        optionsBuilder.UseSqlite("Data Source=chat_history.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure indexes for performance
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.ConversationId);

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.CreatedAt);

        // Composite index for querying by role and time
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => new { m.Role, m.CreatedAt });

        // Relationship configuration
        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data for demonstration
        var convId = Guid.NewGuid();
        modelBuilder.Entity<Conversation>().HasData(
            new Conversation { Id = convId, SessionId = "session_123", Summary = "Billing Inquiry" }
        );

        modelBuilder.Entity<ChatMessage>().HasData(
            new ChatMessage { 
                Id = Guid.NewGuid(), 
                ConversationId = convId, 
                Role = "User", 
                Content = "How do I upgrade my plan?", 
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                MetadataJson = "{\"department\":\"billing\",\"intent\":\"upgrade\"}"
            },
            new ChatMessage { 
                Id = Guid.NewGuid(), 
                ConversationId = convId, 
                Role = "Assistant", 
                Content = "You can upgrade via the account settings page.", 
                CreatedAt = DateTime.UtcNow.AddMinutes(-4),
                MetadataJson = "{\"department\":\"billing\"}"
            }
        );
    }
}

// Example usage
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup dependency injection (simulated for brevity)
        var services = new ServiceCollection();
        services.AddDbContext<ChatContext>();
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatContext>();

        // Ensure database is created and seeded
        await context.Database.EnsureCreatedAsync();

        // 1. Create a new conversation
        var newConversation = new Conversation 
        { 
            Id = Guid.NewGuid(), 
            SessionId = "session_456",
            Summary = "Technical Support"
        };
        context.Conversations.Add(newConversation);
        await context.SaveChangesAsync();

        // 2. Add messages to the conversation
        var messages = new[]
        {
            new ChatMessage 
            { 
                ConversationId = newConversation.Id, 
                Role = "User", 
                Content = "My app is crashing on startup.", 
                MetadataJson = "{\"priority\":\"high\",\"category\":\"bug\"}"
            },
            new ChatMessage 
            { 
                ConversationId = newConversation.Id, 
                Role = "Assistant", 
                Content = "Please provide the error logs.", 
                MetadataJson = "{\"category\":\"troubleshooting\"}"
            }
        };
        context.ChatMessages.AddRange(messages);
        await context.SaveChangesAsync();

        // 3. Retrieve conversation history with filtering
        // Real-world scenario: Fetch last 5 messages for context window
        var history = await context.ChatMessages
            .Where(m => m.ConversationId == newConversation.Id)
            .OrderBy(m => m.CreatedAt)
            .Take(5)
            .ToListAsync();

        Console.WriteLine($"Retrieved {history.Count} messages for conversation {newConversation.Id}:");
        foreach (var msg in history)
        {
            Console.WriteLine($"[{msg.CreatedAt:HH:mm:ss}] {msg.Role}: {msg.Content}");
        }

        // 4. Demonstrate metadata filtering (e.g., for analytics)
        // Real-world scenario: Find all high-priority issues
        var highPriorityMessages = await context.ChatMessages
            .Where(m => m.MetadataJson != null && m.MetadataJson.Contains("\"priority\":\"high\""))
            .ToListAsync();

        Console.WriteLine($"\nFound {highPriorityMessages.Count} high-priority messages.");
    }
}
