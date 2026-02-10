
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGPipeline.Advanced.Data
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public class Message
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty; // User, Assistant
        public string Content { get; set; } = string.Empty;
        
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        // Many-to-Many via explicit join entity
        public ICollection<MessageContext> MessageContexts { get; set; } = new List<MessageContext>();
    }

    public class ContextItem
    {
        public Guid Id { get; set; }
        public string SourceText { get; set; } = string.Empty;
        public string Metadata { get; set; } = string.Empty;

        // Navigation property back to join entity
        public ICollection<MessageContext> MessageContexts { get; set; } = new List<MessageContext>();
    }

    public class MessageContext
    {
        public Guid MessageId { get; set; }
        public Message Message { get; set; } = null!;

        public Guid ContextItemId { get; set; }
        public ContextItem ContextItem { get; set; } = null!;

        public double RelevanceScore { get; set; }
    }

    public class RAGSystemContext : DbContext
    {
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ContextItem> ContextItems { get; set; }
        public DbSet<MessageContext> MessageContexts { get; set; }

        public RAGSystemContext(DbContextOptions<RAGSystemContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Conversation 1-to-Many Message
            builder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade); // Delete Conversation -> Deletes Messages

            // 2. Message Many-to-Many ContextItem (Explicit Join)
            builder.Entity<MessageContext>(entity =>
            {
                entity.HasKey(mc => new { mc.MessageId, mc.ContextItemId });

                // Message -> MessageContext
                entity.HasOne(mc => mc.Message)
                    .WithMany(m => m.MessageContexts)
                    .HasForeignKey(mc => mc.MessageId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete Message -> Deletes Association

                // ContextItem -> MessageContext
                entity.HasOne(mc => mc.ContextItem)
                    .WithMany(ci => ci.MessageContexts)
                    .HasForeignKey(mc => mc.ContextItemId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting ContextItem if referenced
            });
        }
    }

    public class ConversationService
    {
        private readonly RAGSystemContext _context;

        public ConversationService(RAGSystemContext context)
        {
            _context = context;
        }

        // 4. Query to retrieve Conversation, Messages, and ContextItems
        public async Task<Conversation?> GetConversationWithContextAsync(Guid conversationId)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                    .ThenInclude(m => m.MessageContexts) // Include the join entity
                        .ThenInclude(mc => mc.ContextItem) // Include the actual ContextItem
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }
    }
}
