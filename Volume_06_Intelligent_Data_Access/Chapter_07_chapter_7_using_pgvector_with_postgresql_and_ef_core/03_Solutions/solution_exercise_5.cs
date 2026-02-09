
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Linq;

// --- SCHEMA DESIGN ---
// Approach A: Single Table
public class ProductMultiModal
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float[] TextEmbedding { get; set; }
    public float[] ImageEmbedding { get; set; }
}
// Pros: Easier data integrity, single join. Cons: Table can be wide, indexes are separate.

// Approach B: Table Per Modality
public class ProductText { public int ProductId { get; set; } public float[] Embedding { get; set; } }
public class ProductImage { public int ProductId { get; set; } public float[] Embedding { get; set; } }
// Pros: Decoupled, cleaner separation. Cons: Requires joins to unify.

// --- IMPLEMENTATION (Using Approach A for simplicity in code) ---

public class MultiModalService
{
    private readonly AppDbContext _context;

    public MultiModalService(AppDbContext context) => _context = context;

    public async Task<List<UnifiedProductDto>> SearchMultiModal(string queryText, byte[] imageBytes)
    {
        // 1. Generate Embeddings
        float[] textVector = GenerateMockEmbedding(queryText, 384);
        float[] imageVector = GenerateMockImageEmbedding(imageBytes, 512); // Image models often have different dims

        // 2. Parallel Retrieval (Simulated via two separate queries)
        // In a real high-perf system, you might use UNION ALL or separate async tasks.
        
        // Search Text
        var textResults = await _context.Products
            .Select(p => new 
            { 
                p.Id, 
                p.Name, 
                TextScore = 1 - EF.Functions.VectorCosineDistance(p.TextEmbedding, new Vector(textVector)) 
            })
            .OrderByDescending(x => x.TextScore)
            .Take(50)
            .ToListAsync();

        // Search Image
        var imageResults = await _context.Products
            .Select(p => new 
            { 
                p.Id, 
                ImageScore = 1 - EF.Functions.VectorCosineDistance(p.ImageEmbedding, new Vector(imageVector)) 
            })
            .OrderByDescending(x => x.ImageScore)
            .Take(50)
            .ToListAsync();

        // 3. Result Merging (Rank Fusion)
        // We cannot compare raw distances directly (different vector spaces).
        // We normalize by Rank.
        
        // Create a lookup for text ranks
        var textRanks = textResults
            .Select((x, index) => new { x.Id, Rank = index + 1 })
            .ToDictionary(x => x.Id, x => x.Rank);

        // Create a lookup for image ranks
        var imageRanks = imageResults
            .Select((x, index) => new { x.Id, Rank = index + 1 })
            .ToDictionary(x => x.Id, x => x.Rank);

        // Get all unique IDs from both results
        var allIds = textRanks.Keys.Union(imageRanks.Keys).Distinct();

        // Calculate Weighted Rank Score
        // Weights: 60% Text, 40% Image
        const double weightText = 0.6;
        const double weightImage = 0.4;

        var merged = allIds.Select(id =>
        {
            // Default rank is high (bad) if missing in one set
            int tRank = textRanks.ContainsKey(id) ? textRanks[id] : 100; 
            int iRank = imageRanks.ContainsKey(id) ? imageRanks[id] : 100;

            // Calculate score (Lower rank is better, so we invert or minimize the weighted sum)
            // A common formula is: 1 / (weighted_rank_sum) or just minimizing the weighted sum.
            // Let's use a simple weighted average of Ranks (lower is better).
            double weightedRank = (tRank * weightText) + (iRank * weightImage);

            // Fetch name (could be optimized by including in the initial queries)
            // For this exercise, we assume we have access to the name or fetch it separately.
            // Here we mock the name retrieval for the final DTO.
            string name = $"Product {id}"; 

            return new UnifiedProductDto
            {
                Id = id,
                Name = name,
                CombinedScore = 1.0 / weightedRank // Convert to a relevance score (higher is better)
            };
        })
        .OrderByDescending(x => x.CombinedScore)
        .Take(10)
        .ToList();

        return merged;
    }

    // Mock helpers
    private float[] GenerateMockEmbedding(string text, int dim) { /* ... */ return new float[dim]; }
    private float[] GenerateMockImageEmbedding(byte[] bytes, int dim) { /* ... */ return new float[dim]; }
}

public class UnifiedProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double CombinedScore { get; set; }
}
