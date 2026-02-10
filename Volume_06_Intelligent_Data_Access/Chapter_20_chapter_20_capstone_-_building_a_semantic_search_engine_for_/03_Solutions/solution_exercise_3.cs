
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class HybridSearchService
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly ILogger<HybridSearchService> _logger;

    public HybridSearchService(AppDbContext context, IEmbeddingGenerator embeddingGenerator, ILogger<HybridSearchService> logger)
    {
        _context = context;
        _embeddingGenerator = embeddingGenerator;
        _logger = logger;
    }

    // 1. Vector Search (Simulated with Euclidean Distance in LINQ)
    private async Task<IEnumerable<(DocumentChunk Chunk, double Score)>> VectorSearchAsync(string query, int topK)
    {
        var queryVector = await _embeddingGenerator.GenerateEmbeddingAsync(query);
        
        // Note: In a real DB (e.g., PostgreSQL with pgvector), this would be a native SQL operation.
        // Here we simulate the calculation in memory after fetching candidates, 
        // or assume the DB supports a User Defined Function.
        // For this exercise, we will fetch all and calculate (inefficient for prod, ok for demo).
        
        var allChunks = await _context.DocumentChunks.ToListAsync();
        
        var results = allChunks
            .Select(c => 
            {
                // Euclidean Distance (lower is better, but we want higher score for closer)
                // Convert to similarity: 1 / (1 + distance)
                var distance = CalculateEuclideanDistance(c.Embedding, queryVector);
                return (Chunk: c, Score: 1.0 / (1.0 + distance));
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();

        return results;
    }

    // 2. Full-Text Search (Simulated with EF Core Like)
    private async Task<IEnumerable<(DocumentChunk Chunk, double Score)>> TextSearchAsync(string query, int topK)
    {
        // In a real scenario with SQL Server, we would use:
        // FROM DocumentChunks WHERE CONTAINS(Content, 'search_term')
        // Here we use EF Core Like for generic compatibility.
        
        var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Simple scoring: count of keyword occurrences
        var results = await _context.DocumentChunks
            .AsEnumerable() // Switch to client evaluation for this demo logic
            .Select(c => 
            {
                int count = keywords.Sum(k => CountOccurrences(c.Content, k));
                return (Chunk: c, Score: (double)count);
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToListAsync();

        return results;
    }

    // 3. Hybrid Search with RRF
    public async Task<IEnumerable<DocumentChunk>> HybridSearchAsync(string query, int topK)
    {
        var vectorTask = VectorSearchAsync(query, topK);
        var textTask = TextSearchAsync(query, topK);

        await Task.WhenAll(vectorTask, textTask);

        var vectorResults = vectorTask.Result.ToList();
        var textResults = textTask.Result.ToList();

        // RRF Formula: Score = 1 / (k + rank)
        const int k = 60;
        var rrfScores = new Dictionary<Guid, double>();

        // Process Vector Results
        for (int i = 0; i < vectorResults.Count; i++)
        {
            var id = vectorResults[i].Chunk.Id;
            if (!rrfScores.ContainsKey(id)) rrfScores[id] = 0;
            rrfScores[id] += 1.0 / (k + (i + 1));
        }

        // Process Text Results
        for (int i = 0; i < textResults.Count; i++)
        {
            var id = textResults[i].Chunk.Id;
            if (!rrfScores.ContainsKey(id)) rrfScores[id] = 0;
            rrfScores[id] += 1.0 / (k + (i + 1));
        }

        // Merge and Return
        return rrfScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topK)
            .Select(kvp => vectorResults.Concat(textResults).First(x => x.Chunk.Id == kvp.Key).Chunk)
            .ToList();
    }

    // 4. Dynamic Weighting Implementation
    public async Task<IEnumerable<DocumentChunk>> HybridSearchWeightedAsync(string query, int topK, double vectorWeight, double textWeight)
    {
        var vectorTask = VectorSearchAsync(query, topK);
        var textTask = TextSearchAsync(query, topK);

        await Task.WhenAll(vectorTask, textTask);

        var vectorResults = vectorTask.Result.ToList();
        var textResults = textTask.Result.ToList();

        // Normalize Weights to sum to 1.0 (optional but recommended)
        double totalWeight = vectorWeight + textWeight;
        double wV = vectorWeight / totalWeight;
        double wT = textWeight / totalWeight;

        // Normalize Scores (Min-Max Normalization)
        var vMax = vectorResults.Any() ? vectorResults.Max(x => x.Score) : 1;
        var vMin = vectorResults.Any() ? vectorResults.Min(x => x.Score) : 0;
        
        var tMax = textResults.Any() ? textResults.Max(x => x.Score) : 1;
        var tMin = textResults.Any() ? textResults.Min(x => x.Score) : 0;

        var combinedScores = new Dictionary<Guid, double>();

        // Add Vector Contributions
        foreach (var (chunk, score) in vectorResults)
        {
            double normalized = (vMax - vMin) == 0 ? 1 : (score - vMin) / (vMax - vMin);
            combinedScores[chunk.Id] = wV * normalized;
        }

        // Add Text Contributions
        foreach (var (chunk, score) in textResults)
        {
            double normalized = (tMax - tMin) == 0 ? 1 : (score - tMin) / (tMax - tMin);
            if (combinedScores.ContainsKey(chunk.Id))
                combinedScores[chunk.Id] += wT * normalized;
            else
                combinedScores[chunk.Id] = wT * normalized;
        }

        return combinedScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topK)
            .Select(kvp => 
                vectorResults.FirstOrDefault(x => x.Chunk.Id == kvp.Key).Chunk ?? 
                textResults.First(x => x.Chunk.Id == kvp.Key).Chunk)
            .ToList();
    }

    // Math Helpers
    private double CalculateEuclideanDistance(byte[] v1, float[] v2)
    {
        // Assuming byte[] is normalized 0-255 representation of float[] for storage
        // Convert byte[] back to float for calculation
        float[] v1Float = v1.Select(b => b / 255.0f).ToArray();
        
        double sum = 0;
        for (int i = 0; i < v1Float.Length && i < v2.Length; i++)
        {
            sum += Math.Pow(v1Float[i] - v2[i], 2);
        }
        return Math.Sqrt(sum);
    }

    private int CountOccurrences(string source, string substring)
    {
        if (string.IsNullOrEmpty(substring)) return 0;
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(substring, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }
}
