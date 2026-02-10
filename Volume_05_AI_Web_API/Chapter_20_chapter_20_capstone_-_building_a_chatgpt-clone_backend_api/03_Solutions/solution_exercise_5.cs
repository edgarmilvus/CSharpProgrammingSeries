
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
using System.Text.Json;

// 1. Entity & Configuration
public class ChatSession
{
    public int Id { get; set; }
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    
    // Store messages as a JSON string (EF Core will handle conversion)
    // We use a ValueComparer to ensure EF Core detects changes within the list
    public List<ChatMessage> Messages { get; set; } = new();

    // 4. Concurrency Control
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ChatContext : DbContext
{
    public DbSet<ChatSession> ChatSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatSession>(entity =>
        {
            // 1. JSONB Column Strategy
            entity.Property(e => e.Messages)
                .HasConversion(
                    // Convert List<ChatMessage> to JSON string for storage
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    // Convert JSON string back to List<ChatMessage>
                    v => JsonSerializer.Deserialize<List<ChatMessage>>(v, (JsonSerializerOptions)null),
                    // Value Comparer: Essential for EF Core to detect changes in the list
                    new ValueComparer<List<ChatMessage>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => new List<ChatMessage>(c)
                    )
                )
                .HasColumnType("jsonb"); // Explicitly map to PostgreSQL jsonb type

            // 4. Optimistic Concurrency
            entity.Property(e => e.RowVersion)
                .IsRowVersion(); // Maps to PostgreSQL xmin system column or a timestamp column

            // 3. Indexing
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.UpdatedAt); // Assuming UpdatedAt property exists
            // Note: GIN index is usually created via raw SQL migration for JSONB
        });
    }
}

// 2. Partial Retrieval Repository
public class ChatRepository
{
    private readonly ChatContext _context;

    public ChatRepository(ChatContext context) => _context = context;

    public async Task<List<ChatMessage>> GetRecentMessagesAsync(string sessionId, int count, CancellationToken cancellationToken)
    {
        // Using EF Core 7+ JSON querying capabilities or raw SQL for complex JSONB extraction
        // This example uses raw SQL for precise JSONB array slicing (PostgreSQL specific)
        
        var sql = @"
            SELECT ""Messages"" 
            FROM ""ChatSessions"" 
            WHERE ""SessionId"" = @p0
            LIMIT 1";

        // Note: To get the *last N* messages efficiently, we usually rely on client-side processing 
        // or PostgreSQL JSONB operators if the structure is predictable.
        // However, EF Core 8+ supports translating LINQ to JSON queries.
        
        // Example using EF Core 8 JSON Querying (if supported by provider):
        var session = await _context.ChatSessions
            .Where(s => s.SessionId == sessionId)
            .Select(s => s.Messages)
            .FirstOrDefaultAsync(cancellationToken);

        if (session == null) return new List<ChatMessage>();

        // Return the last N messages
        return session.Skip(Math.Max(0, session.Count - count)).ToList();
    }

    public async Task UpdateSessionAsync(ChatSession session, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle Optimistic Concurrency failure
            // e.g., Log conflict, reload entity, merge changes, and retry
            throw new InvalidOperationException("The session was modified by another user. Please refresh.", ex);
        }
    }
}

// Migration Snippet (Raw SQL for GIN Index)
/*
CREATE INDEX idx_chat_sessions_messages_gin ON "ChatSessions" USING GIN ("Messages");
*/
