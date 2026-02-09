
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiUserChatConcurrency
{
    // ==========================================
    // 1. Domain Models
    // ==========================================
    // Represents a single chat message in the system.
    // We use a class to simulate a database entity.
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // CRITICAL: Concurrency Token (Row Version)
        // In a real EF Core scenario, this would be marked with [Timestamp] or IsRowVersion().
        // It acts as a fingerprint of the row's state at the time of retrieval.
        public byte[] RowVersion { get; set; }

        public ChatMessage(string content)
        {
            Content = content;
            CreatedAt = DateTime.Now;
            // Initialize version (simulating a new DB record)
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }; 
        }
    }

    // ==========================================
    // 2. In-Memory Database Simulation
    // ==========================================
    // Simulates the database context and the concurrency mechanism.
    // In a real app, this logic is handled by EF Core's ChangeTracker.
    public class ChatDbContext
    {
        // Simulating a database table with a list.
        private static List<ChatMessage> _database = new List<ChatMessage>();
        private static readonly object _dbLock = new object();

        // Seed initial data
        static ChatDbContext()
        {
            _database.Add(new ChatMessage("Hello everyone! Welcome to the chat.") { Id = 1 });
        }

        // Simulates EF Core's FindAsync method
        public ChatMessage GetMessage(int id)
        {
            lock (_dbLock)
            {
                return _database.Find(m => m.Id == id);
            }
        }

        // Simulates EF Core's SaveChangesAsync with Optimistic Concurrency check
        public void UpdateMessage(ChatMessage updatedMessage)
        {
            lock (_dbLock)
            {
                // 1. Retrieve the current state from the "database"
                var existingMessage = _database.Find(m => m.Id == updatedMessage.Id);

                if (existingMessage == null)
                {
                    throw new InvalidOperationException("Message not found.");
                }

                // 2. THE CONCURRENCY CHECK
                // Compare the RowVersion of the incoming update with the current DB version.
                // If they don't match, someone else updated the record in between.
                if (!AreByteArraysEqual(existingMessage.RowVersion, updatedMessage.RowVersion))
                {
                    // Throw the specific exception EF Core throws in this scenario
                    throw new DBConcurrencyException(
                        "The record you attempted to update was modified by another user after you loaded it.");
                }

                // 3. Apply updates and BUMP the version
                existingMessage.Content = updatedMessage.Content;
                
                // Increment the version (simulating EF Core behavior)
                // In reality, this is a hash or timestamp increment.
                existingMessage.RowVersion[7]++; 
                
                Console.WriteLine($"[DB] Successfully updated Message {existingMessage.Id}. New Version: {existingMessage.RowVersion[7]}");
            }
        }

        // Helper to simulate byte array comparison
        private bool AreByteArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length) return false;
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i]) return false;
            }
            return true;
        }
    }

    // ==========================================
    // 3. The Concurrency Service (Logic Layer)
    // ==========================================
    public class ChatService
    {
        private readonly ChatDbContext _context = new ChatDbContext();

        // Simulates a user attempting to edit a message
        // Returns true if successful, false if conflict occurred.
        public bool AttemptMessageEdit(int messageId, string newContent, string userName)
        {
            Console.WriteLine($"\n[{userName}] Attempting to edit Message {messageId}...");

            try
            {
                // STEP 1: Load the entity (Simulating AsNoTracking or standard Fetch)
                // We capture the current RowVersion here.
                var messageToUpdate = _context.GetMessage(messageId);
                
                if (messageToUpdate == null)
                {
                    Console.WriteLine($"[{userName}] Error: Message not found.");
                    return false;
                }

                // Simulate network latency or processing time
                // This increases the window for a concurrency conflict
                Thread.Sleep(1000); 

                // STEP 2: Modify the entity locally
                // We create a copy to simulate the client sending changes back to server
                var updatedMessage = new ChatMessage(newContent)
                {
                    Id = messageToUpdate.Id,
                    RowVersion = messageToUpdate.RowVersion, // We carry the OLD version token
                    CreatedAt = messageToUpdate.CreatedAt
                };

                // STEP 3: Attempt to save (Persist)
                _context.UpdateMessage(updatedMessage);

                Console.WriteLine($"[{userName}] Edit successful!");
                return true;
            }
            catch (DBConcurrencyException ex)
            {
                // STEP 4: Handle the Conflict
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{userName}] CONFLICT DETECTED! {ex.Message}");
                Console.ResetColor();
                
                // In a real app, we would trigger the "Conflict Resolution" logic here
                HandleConflictResolution(messageId, newContent, userName);
                
                return false;
            }
        }

        // Conflict Resolution Logic (The "Advanced" part)
        private void HandleConflictResolution(int messageId, string attemptedChange, string userName)
        {
            Console.WriteLine($"[{userName}] Initiating Automatic Merge Strategy...");
            
            // Fetch the latest version from DB to see what changed
            var latestMessage = _context.GetMessage(messageId);
            
            Console.WriteLine($"[{userName}] Latest DB Content: '{latestMessage.Content}'");
            Console.WriteLine($"[{userName}] Your Attempted Content: '{attemptedChange}'");
            
            // Simple Resolution: Append changes (Operational Transformation concept)
            string mergedContent = $"{latestMessage.Content} | [Edit by {userName}: {attemptedChange}]";
            
            Console.WriteLine($"[{userName}] Merged Content Proposed: '{mergedContent}'");
            
            // Retry the edit with the new merged content
            // Note: In a real system, this might require user intervention or complex merge algorithms.
            AttemptMessageEdit(messageId, mergedContent, userName + "_Merge");
        }
    }

    // ==========================================
    // 4. Main Execution (Simulation)
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Multi-User Chat Concurrency Simulation ===\n");

            var chatService = new ChatService();

            // SCENARIO: Two users (Alice and Bob) try to edit the same message simultaneously.
            
            // User 1: Alice
            Thread aliceThread = new Thread(() => {
                chatService.AttemptMessageEdit(1, "Hello everyone! (Edited by Alice)", "Alice");
            });

            // User 2: Bob
            Thread bobThread = new Thread(() => {
                // Bob starts slightly after Alice to ensure they load the same initial version
                Thread.Sleep(200); 
                chatService.AttemptMessageEdit(1, "Hello everyone! (Edited by Bob)", "Bob");
            });

            // Start the concurrent operations
            aliceThread.Start();
            bobThread.Start();

            // Wait for threads to finish
            aliceThread.Join();
            bobThread.Join();

            Console.WriteLine("\n=== Simulation Complete ===");
            Console.WriteLine("Notice how Bob's update failed initially due to Alice's commit.");
            Console.WriteLine("The system automatically detected the conflict and merged the changes.");
        }
    }
}
