
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum Role { User, Assistant }

public class ConversationMemory
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Role Role { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public List<float> Embedding { get; set; }
}

public class MemoryContext : DbContext
{
    public DbSet<ConversationMemory> ConversationMemories { get; set; }
    // ... config
}

public class MemoryService
{
    private readonly MemoryContext _context;

    public MemoryService(MemoryContext context)
    {
        _context = context;
    }

    public async Task<List<ConversationMemory>> RetrieveRelevantContextAsync(
        Guid sessionId, 
        List<float> currentQueryEmbedding, 
        Guid? currentMessageId = null) // Exclusion ID
    {
        var queryVectorJson = System.Text.Json.JsonSerializer.Serialize(currentQueryEmbedding);

        // 1. Base Query: Filter by Session
        var query = _context.ConversationMemories
            .Where(cm => cm.SessionId == sessionId);

        // 2. Exclusion: Remove the current message if it exists in DB
        if (currentMessageId.HasValue)
        {
            query = query.Where(cm => cm.Id != currentMessageId.Value);
        }

        // 3. Semantic Ranking: Order by Vector Distance
        // We use Euclidean distance on normalized embeddings.
        // Note: We assume a helper method or UDF for distance calculation.
        
        var relevantHistory = await query
            .OrderBy(cm => CalculateDistance(cm.Embedding, queryVectorJson))
            .Take(3) // Top 3 relevant exchanges
            .ToListAsync();

        return relevantHistory;
    }

    private double CalculateDistance(List<float> dbVector, string queryVectorJson)
    {
        // In a real EF Core implementation, this logic is translated to SQL.
        // For this example, we simulate the SQL translation.
        // Actual SQL: ORDER BY VECTOR_DISTANCE('euclidean', NormalizedEmbedding, @query)
        throw new NotImplementedException("Map to DB Function");
    }
}
