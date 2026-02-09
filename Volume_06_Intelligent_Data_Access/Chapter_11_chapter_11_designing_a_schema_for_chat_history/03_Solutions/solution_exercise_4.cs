
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// 1. State & Branching Entities
public enum ConversationStateEnum { Active, Paused, Completed, Error }

public class ConversationState
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public ConversationStateEnum CurrentState { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ChatMessage
{
    // ... existing properties ...
    public Guid? BranchId { get; set; } // Nullable: null implies main branch
}

public class Conversation
{
    // ... existing properties ...
    public Guid? ActiveBranchId { get; set; } // Tracks which branch is currently "active"
}

// 2. Repository Pattern with Transactions
public class ConversationRepository
{
    private readonly ChatContext _context;

    public ConversationRepository(ChatContext context) => _context = context;

    // Interactive Challenge: Start a new branch
    public async Task<Guid> StartNewBranchAsync(Guid conversationId, Guid rootMessageId)
    {
        // Generate a new Branch ID
        var newBranchId = Guid.NewGuid();

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 1. Find the root message to branch from
                var rootMessage = await _context.ChatMessages.FindAsync(rootMessageId);
                if (rootMessage == null) throw new Exception("Root message not found");

                // 2. Create a "Branch Point" message (optional, but good practice)
                // For this exercise, we simply update the conversation's active branch
                var conversation = await _context.Conversations.FindAsync(conversationId);
                if (conversation == null) throw new Exception("Conversation not found");

                conversation.ActiveBranchId = newBranchId;

                // 3. In a real scenario, we might copy the root message or just 
                // mark subsequent messages with the new BranchId.
                // Here we just update the state.
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return newBranchId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    // Switch Branch
    public async Task SwitchBranchAsync(Guid conversationId, Guid branchId)
    {
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.ActiveBranchId = branchId;
            await _context.SaveChangesAsync();
        }
    }

    // Add Message to Branch (Transaction Safety)
    public async Task AddMessageToBranchAsync(Guid conversationId, Guid? branchId, string role, string content)
    {
        // Using explicit transaction as requested
        using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 1. Update Conversation State to Active
                var state = await _context.Set<ConversationState>()
                    .FirstOrDefaultAsync(s => s.ConversationId == conversationId);
                
                if (state == null)
                {
                    state = new ConversationState 
                    { 
                        Id = Guid.NewGuid(), 
                        ConversationId = conversationId, 
                        CurrentState = ConversationStateEnum.Active,
                        LastUpdated = DateTime.UtcNow 
                    };
                    _context.Set<ConversationState>().Add(state);
                }
                else
                {
                    state.CurrentState = ConversationStateEnum.Active;
                    state.LastUpdated = DateTime.UtcNow;
                }

                // 2. Add the message
                var message = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    BranchId = branchId,
                    Role = role,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };
                _context.ChatMessages.Add(message);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
