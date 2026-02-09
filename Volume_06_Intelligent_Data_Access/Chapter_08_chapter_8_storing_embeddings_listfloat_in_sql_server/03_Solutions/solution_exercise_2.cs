
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// DTOs
public class SearchRequest
{
    public List<float> QueryVector { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Category { get; set; }
}

public class SearchResultDto
{
    public string Content { get; set; }
    public double SimilarityScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Extended Entity for Metadata
public class VectorRecordWithContext : VectorRecord
{
    public DateTime CreatedAt { get; set; }
    public string Category { get; set; }
}

public class RagService
{
    private readonly VectorContext _context;

    public RagService(VectorContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResultDto>> SearchWithContextAsync(SearchRequest request)
    {
        // 1. Prepare the query vector (normalize or serialize)
        var queryVectorJson = System.Text.Json.JsonSerializer.Serialize(request.QueryVector);

        // 2. Construct the Composable Query
        // We use IQueryable to allow further composition (Skip/Take) by the caller.
        // Note: We assume a SQL User Defined Function 'VectorDistance' is mapped.
        
        var query = _context.Set<VectorRecordWithContext>()
            .Where(v => v.CreatedAt >= request.FromDate && v.CreatedAt <= request.ToDate)
            .Where(v => v.Category == request.Category)
            .Select(v => new SearchResultDto
            {
                Content = v.Content,
                CreatedAt = v.CreatedAt,
                // Calculate distance on the fly. Lower distance = higher similarity for Euclidean.
                // We map this to a "SimilarityScore" (inverted or scaled if needed).
                SimilarityScore = CalculateDistance(v.NormalizedEmbedding, queryVectorJson)
            })
            .Where(x => x.SimilarityScore < 1.5) // Optional: Pre-filtering logic
            .OrderBy(x => x.SimilarityScore);

        // 3. Execution (Pagination handled externally or here)
        return await query.Take(5).ToListAsync();
    }

    // Helper to simulate the SQL Vector Distance function in LINQ-to-Entities
    // In a real scenario, this would be mapped via EF Core's DbFunction.
    private double CalculateDistance(byte[] vectorA, string vectorBJson)
    {
        // This is a placeholder. In EF Core, you would define a static method 
        // and map it in OnModelCreating to a SQL function.
        throw new NotImplementedException("Map this to a DbFunction in DbContext");
    }
}
