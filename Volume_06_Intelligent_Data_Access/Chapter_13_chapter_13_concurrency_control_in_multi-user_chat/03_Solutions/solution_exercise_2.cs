
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// Mock Vector Entity
public class VectorEmbedding
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public float[] EmbeddingData { get; set; }
    public DateTime GeneratedAt { get; set; }
}

// DbContext Definition
public class ChatContext : DbContext
{
    public DbSet<ChatMessage> Messages { get; set; }
    public DbSet<VectorEmbedding> Vectors { get; set; }
}

public class RagService
{
    private readonly ChatContext _context;

    public RagService(ChatContext context)
    {
        _context = context;
    }

    // 4. Retry mechanism constants
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 100;

    public async Task SendMessageWithRAGAsync(string content, Guid userId)
    {
        int retryCount = 0;

        while (true)
        {
            // 4. Begin Transaction
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Insert Chat Message
                var message = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    Content = content,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Messages.AddAsync(message);
                
                // Save to generate the ID if needed, but we are doing explicit transaction handling
                // so we save changes here to ensure the message is committed with the transaction.
                await _context.SaveChangesAsync();

                // 2. Simulate Vector Generation
                // Ensure connection is open (handled by BeginTransactionAsync keeping the connection alive)
                await Task.Delay(50); // Simulate processing time
                
                var vector = new VectorEmbedding
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    EmbeddingData = new float[] { 0.1f, 0.5f, 0.9f }, // Mock data
                    GeneratedAt = DateTime.UtcNow
                };

                await _context.Vectors.AddAsync(vector);
                
                // 3. Save Changes within transaction
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();
                
                // Success, break the loop
                break;
            }
            catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is InvalidOperationException)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();

                retryCount++;
                if (retryCount >= MaxRetries)
                {
                    throw new Exception("Max retries reached. Operation failed.", ex);
                }

                // Exponential Backoff
                int delay = InitialDelayMs * (int)Math.Pow(2, retryCount - 1);
                await Task.Delay(delay);
            }
            finally
            {
                // 5. Ensure connection management
                // Explicit transactions usually keep the connection open until commit/rollback.
                // The 'using' pattern or finally block ensures resources are released if manual disposal is needed.
                await transaction.DisposeAsync();
            }
        }
    }
}
