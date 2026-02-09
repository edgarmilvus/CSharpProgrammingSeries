
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum SearchMode { HighPrecision, BroadRecall }

public class AdvancedSearchService
{
    private readonly VectorContext _context;

    public AdvancedSearchService(VectorContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResultDto>> SearchWithThresholdAsync(
        List<float> queryVector, 
        SearchMode mode, 
        VectorContext context)
    {
        // 1. Determine Threshold
        double threshold = mode switch
        {
            SearchMode.HighPrecision => 0.5, // Strict
            SearchMode.BroadRecall => 1.5,   // Loose
            _ => 1.0
        };

        var queryVectorJson = System.Text.Json.JsonSerializer.Serialize(queryVector);

        // 2. Primary Vector Search (Server-side)
        // We construct a raw SQL query to ensure the WHERE clause uses the computed distance.
        // Using a local function to encapsulate the query construction.
        
        IQueryable<VectorRecord> VectorSearchQuery(double currentThreshold)
        {
            return _context.VectorRecords
                .FromSqlInterpolated($@"
                    SELECT * FROM VectorRecords 
                    WHERE dbo.CalculateEuclideanDistance(NormalizedEmbedding, {queryVectorJson}) < {currentThreshold}
                    ORDER BY dbo.CalculateEuclideanDistance(NormalizedEmbedding, {queryVectorJson}) ASC
                ");
        }

        var results = await VectorSearchQuery(threshold).ToListAsync();

        // 3. Handle Empty Results (Fallback)
        if (!results.Any())
        {
            // Fallback: Standard SQL LIKE operation on Content column
            // We extract keywords from the query vector context or use a generic approach.
            // Here, we simulate a fallback by searching for a generic term or re-running without threshold.
            
            // Note: In a real scenario, you might parse the original text query associated with the vector.
            // Since we only have the vector here, we perform a broad LIKE or remove the threshold.
            
            results = await _context.VectorRecords
                .Where(v => EF.Functions.Like(v.Content, "%general%")) // Placeholder for text extraction
                .OrderBy(v => v.Id) // Default sort
                .Take(5)
                .ToListAsync();
        }

        return results.Select(r => new SearchResultDto 
        { 
            Content = r.Content, 
            SimilarityScore = 0, // Calculate actual distance if needed
            CreatedAt = DateTime.Now 
        }).ToList();
    }
}
