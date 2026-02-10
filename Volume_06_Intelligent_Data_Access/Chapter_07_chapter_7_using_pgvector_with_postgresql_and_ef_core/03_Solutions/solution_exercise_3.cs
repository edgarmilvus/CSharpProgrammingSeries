
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[] Embedding { get; set; }
}

public class RagService
{
    private readonly AppDbContext _context;

    public RagService(AppDbContext context) => _context = context;

    public async Task<string> GetRelevantContext(string question, int topK, int maxTokens)
    {
        // 1. Generate embedding for the question
        float[] questionVector = GenerateMockEmbedding(question, 384);

        // 2. Retrieve a larger pool initially (e.g., top 50) to allow for token filtering
        // We order by similarity (distance) to ensure we get the best candidates first.
        var candidates = await _context.DocumentChunks
            .OrderBy(d => EF.Functions.VectorCosineDistance(d.Embedding, new Vector(questionVector)))
            .Take(50) 
            .Select(d => new { d.Content, d.DocumentId }) // Only fetch what we need
            .ToListAsync();

        // 3. Token Counting & Filtering (Client-side processing)
        // Heuristic: 1 token ≈ 4 characters
        int currentTokenCount = 0;
        var selectedChunks = new List<string>();
        int chunkCounter = 1;

        foreach (var candidate in candidates)
        {
            // Calculate estimated tokens for this chunk
            int chunkTokens = (int)Math.Ceiling(candidate.Content.Length / 4.0);

            if (currentTokenCount + chunkTokens > maxTokens)
            {
                // Stop if adding this chunk exceeds the limit
                break;
            }

            // 4. Prompt Formatting
            selectedChunks.Add($"[{chunkCounter}] {candidate.Content}");
            currentTokenCount += chunkTokens;
            chunkCounter++;
        }

        if (!selectedChunks.Any())
        {
            return $"Question: {question}\nContext: No relevant context found within token limit.";
        }

        var formattedContext = new StringBuilder();
        formattedContext.AppendLine("Context:");
        formattedContext.AppendLine(string.Join("\n", selectedChunks));
        formattedContext.AppendLine($"Question: {question}");

        return formattedContext.ToString();
    }

    // 5. Hybrid Metadata Filtering (Optional Extension)
    public async Task<string> GetContextWithSourceFilter(string question, string sourceKeyword, int maxTokens)
    {
        float[] questionVector = GenerateMockEmbedding(question, 384);

        // Dynamic Where clause based on content analysis
        // Note: In a real scenario, you might have a separate 'Source' column.
        // Here we simulate checking the DocumentId or Content.
        var query = _context.DocumentChunks
             .Where(d => d.DocumentId.Contains(sourceKeyword) || d.Content.Contains(sourceKeyword)) // Dynamic filter
             .OrderBy(d => EF.Functions.VectorCosineDistance(d.Embedding, new Vector(questionVector)))
             .Take(20);

        // ... (Rest of logic same as above)
        return await query.AnyAsync() ? "Found context with filter." : "No matches.";
    }

    private float[] GenerateMockEmbedding(string text, int dimensions)
    {
        var rng = new Random(text.Length);
        var vector = new float[dimensions];
        for (int i = 0; i < dimensions; i++) vector[i] = (float)rng.NextDouble();
        return vector;
    }
}
