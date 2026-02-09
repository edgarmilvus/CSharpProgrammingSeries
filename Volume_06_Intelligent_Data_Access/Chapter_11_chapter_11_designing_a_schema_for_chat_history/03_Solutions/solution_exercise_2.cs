
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
using System.Numerics;

// 1. Interface Definition
public interface IChatMemoryProvider
{
    Task<ChatMessage> GetMessageByIdAsync(Guid id);
    Task<IEnumerable<ChatMessage>> SearchBySemanticSimilarityAsync(float[] queryVector, int topK);
    Task<Conversation> GetConversationContextAsync(Guid conversationId, int lookbackMessages = 10);
}

// 2. Implementation
public class EfCoreChatMemoryProvider : IChatMemoryProvider
{
    private readonly ChatContext _context;

    public EfCoreChatMemoryProvider(ChatContext context) => _context = context;

    public async Task<ChatMessage> GetMessageByIdAsync(Guid id)
    {
        return await _context.ChatMessages
            .Include(m => m.Embedding) // Eager load vector data
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // Requirement 3: Simulated Vector Search
    public async Task<IEnumerable<ChatMessage>> SearchBySemanticSimilarityAsync(float[] queryVector, int topK)
    {
        // 1. Fetch candidates (e.g., last 100 messages) using relational filters
        // We filter by time or other criteria to reduce the search space.
        var candidates = await _context.ChatMessages
            .Include(m => m.Embedding)
            .Where(m => m.Embedding != null) // Ensure we have vectors
            .OrderByDescending(m => m.Timestamp)
            .Take(100) // Fetch a manageable set
            .ToListAsync();

        // 2. Calculate Cosine Similarity in Memory
        // Using System.Numerics.Vector for potential SIMD optimization (if supported by hardware)
        // or a simple loop for portability.
        var scoredMessages = candidates.Select(m => 
        {
            var targetVector = m.Embedding!.Vector;
            float score = CalculateCosineSimilarity(queryVector, targetVector);
            return new { Message = m, Score = score };
        });

        // 3. Return top K ordered by score
        return scoredMessages
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Message);
    }

    // Helper for Cosine Similarity
    private float CalculateCosineSimilarity(float[] vecA, float[] vecB)
    {
        if (vecA.Length != vecB.Length) return 0;

        // Using System.Numerics.Vector for SIMD if dimensions align, otherwise fallback
        // For simplicity and robustness in this exercise, we use a standard loop.
        float dotProduct = 0;
        float normA = 0;
        float normB = 0;

        for (int i = 0; i < vecA.Length; i++)
        {
            dotProduct += vecA[i] * vecB[i];
            normA += vecA[i] * vecA[i];
            normB += vecB[i] * vecB[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dotProduct / (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    public async Task<Conversation> GetConversationContextAsync(Guid conversationId, int lookbackMessages = 10)
    {
        // Retrieve conversation with recent messages
        var conversation = await _context.Conversations
            .Include(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(lookbackMessages))
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return null;

        // Interactive Challenge: Inject System Context via Semantic Search
        // Find the last user message to use as a query vector
        var lastUserMessage = conversation.Messages
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefault(m => m.Role == "User");

        if (lastUserMessage != null && lastUserMessage.Embedding != null)
        {
            // Retrieve relevant global memory (simulated by searching all messages)
            var relevantContext = await SearchBySemanticSimilarityAsync(
                lastUserMessage.Embedding.Vector, 
                3 // Top 3 relevant messages globally
            );

            // In a real scenario, we might attach these to a "SystemContext" property 
            // or append them to the prompt context manually.
            // For this exercise, we return the conversation object, 
            // but the caller would use 'relevantContext' to augment the prompt.
        }

        return conversation;
    }
}
