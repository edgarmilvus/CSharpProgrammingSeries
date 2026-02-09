
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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// The Problem Context:
// In a multi-user chat application, imagine two users editing the same message simultaneously.
// User A clicks "Edit" on a message, and User B clicks "Edit" on the same message at the exact same time.
// Without concurrency control, User B's changes might overwrite User A's changes (the "Lost Update" problem).
// This example demonstrates how to use EF Core's optimistic concurrency to prevent this data corruption.

namespace ConcurrencyChatExample
{
    // 1. Define the Message Entity
    // We use a 'record' for immutability and concise syntax (modern C# feature).
    // The [ConcurrencyCheck] attribute or the 'rowversion' property tells EF Core to track changes.
    public class ChatMessage
    {
        public int Id { get; set; }
        public string User { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        // CRITICAL: This is the concurrency token.
        // In SQL Server, this maps to a 'rowversion' (timestamp) column that automatically updates on any write.
        // EF Core compares this value during SaveChanges(). If it differs, a concurrency exception is thrown.
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    // 2. Define the DbContext
    public class ChatContext : DbContext
    {
        public DbSet<ChatMessage> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQLite for a self-contained, portable example.
            // In a real app, this would be a connection string to SQL Server or PostgreSQL.
            optionsBuilder.UseSqlite("Data Source=chat.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly configure the RowVersion as a concurrency token.
            // This ensures EF Core checks this value against the database during updates.
            modelBuilder.Entity<ChatMessage>()
                .Property(m => m.RowVersion)
                .IsRowVersion(); 
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Ensure a clean database for the demo
            using var context = new ChatContext();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            Console.WriteLine("=== Concurrency Control Demo ===\n");

            // SCENARIO: Two users (or threads) try to update the same message.
            
            // 1. User A fetches the message to edit.
            int messageId = await CreateInitialMessageAsync();
            
            // We simulate a "Race Condition" by creating two separate contexts (User A and User B).
            // In a real web app, this represents two separate HTTP requests.
            
            // --- User A's Session ---
            using (var userAContext = new ChatContext())
            {
                var messageForA = await userAContext.Messages.FindAsync(messageId);
                Console.WriteLine($"User A read: '{messageForA.Content}' (RowVersion: {BitConverter.ToString(messageForA.RowVersion)})");

                // User A modifies the content locally (in memory).
                messageForA.Content = "User A's corrected version";
                
                // Delay to simulate network latency or user thinking time.
                // During this delay, User B acts...
                await Task.Delay(100); 
            }

            // --- User B's Session (Interleaved) ---
            using (var userBContext = new ChatContext())
            {
                // User B fetches the ORIGINAL message (because User A hasn't saved yet).
                var messageForB = await userBContext.Messages.FindAsync(messageId);
                Console.WriteLine($"User B read: '{messageForB.Content}' (RowVersion: {BitConverter.ToString(messageForB.RowVersion)})");

                // User B modifies the content.
                messageForB.Content = "User B's conflicting version";

                // User B saves FIRST.
                // EF Core sends the UPDATE command including the original RowVersion in the WHERE clause.
                // Since the DB hasn't changed yet, this succeeds.
                await userBContext.SaveChangesAsync();
                Console.WriteLine("User B saved successfully. Database now contains 'User B's conflicting version'.");
            }

            // --- User A Tries to Save (The Conflict) ---
            using (var userAContext = new ChatContext())
            {
                // We need to re-attach the entity User A was working on.
                // In a real app, User A's context might still be alive, or we re-fetch.
                // Here, we simulate re-attaching the disconnected entity.
                var messageForA = new ChatMessage 
                { 
                    Id = messageId, 
                    Content = "User A's corrected version",
                    // IMPORTANT: We must preserve the ORIGINAL RowVersion User A saw.
                    // If we fetch fresh now, we'd see User B's changes. 
                    // To trigger the conflict, we use the version User A originally loaded.
                    RowVersion = context.Messages.Find(messageId).RowVersion 
                };

                userAContext.Messages.Attach(messageForA);
                userAContext.Entry(messageForA).Property(m => m.Content).IsModified = true;

                try
                {
                    // EF Core tries to update: UPDATE Messages SET Content = ... WHERE Id = ... AND RowVersion = [Original A's Version]
                    // But the DB now has User B's NEWER RowVersion.
                    // The WHERE clause fails (0 rows affected).
                    await userAContext.SaveChangesAsync();
                    Console.WriteLine("User A saved successfully (Unexpected).");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("\n!!! CONCURRENCY EXCEPTION CAUGHT !!!");
                    Console.WriteLine($"Error: {ex.Message}");
                    
                    // RESOLUTION STRATEGY: Fetch current values from DB to resolve the conflict.
                    var entry = ex.Entries.Single();
                    var currentValues = await entry.GetDatabaseValuesAsync();
                    
                    Console.WriteLine("\n--- Conflict Resolution ---");
                    Console.WriteLine($"Database Current Content: {currentValues.GetValue<string>("Content")}");
                    Console.WriteLine($"User A Attempted Content: {entry.Property("Content").CurrentValue}");
                    
                    // Example Resolution: Merge or Notify User.
                    // Here, we simply accept the database value and append a note.
                    var resolvedMessage = (ChatMessage)currentValues.ToObject();
                    resolvedMessage.Content += " [Merged with User A's input]";
                    
                    // Update the context with the resolved values and retry.
                    entry.CurrentValues.SetValues(resolvedMessage);
                    
                    // Optional: Force save if business logic dictates.
                    // await userAContext.SaveChangesAsync(); 
                    Console.WriteLine($"Resolved Content: {resolvedMessage.Content}");
                }
            }
        }

        static async Task<int> CreateInitialMessageAsync()
        {
            using var context = new ChatContext();
            var msg = new ChatMessage { User = "System", Content = "Original Message" };
            context.Messages.Add(msg);
            await context.SaveChangesAsync();
            return msg.Id;
        }
    }
}
