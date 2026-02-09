
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
using System.ComponentModel.DataAnnotations;

public class Listing
{
    [Key]
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int SquareFootage { get; set; }
    
    // Assuming a vector extension is available
    [Column(TypeName = "vector(384)")]
    public float[]? DescriptionEmbedding { get; set; }
}

public class RealEstateService
{
    private readonly DbContext _context;

    public RealEstateService(DbContext context)
    {
        _context = context;
    }

    // Mock vector generation for the query
    private float[] GenerateQueryVector(string query)
    {
        return new float[384]; // Simplified for brevity
    }

    // Standard Filtering + Vector Search
    public async Task<List<ListingResult>> SearchWithFiltersAsync(
        string query, 
        decimal maxPrice, 
        int minBedrooms)
    {
        var queryVector = GenerateQueryVector(query);
        var listings = _context.Set<Listing>().AsQueryable();

        // STEP 1: Filtering (Strict SQL WHERE)
        // This executes first, reducing the dataset size.
        // EF Core translates this to: WHERE Price < X AND Bedrooms >= Y
        var filteredListings = listings
            .Where(l => l.Price < maxPrice && l.Bedrooms >= minBedrooms && l.DescriptionEmbedding != null);

        // Check for empty result set to prevent unnecessary vector calculation
        var count = await filteredListings.CountAsync();
        if (count == 0) return new List<ListingResult>();

        // STEP 2: Vector Search (On Filtered Subset)
        // We project the similarity score. 
        // Note: In a real DB (PostgreSQL), we would use:
        // .OrderByDescending(l => 1 - EF.Functions.CosineDistance(l.DescriptionEmbedding, queryVector))
        // Since we are simulating, we assume the DB handles the vector math efficiently on the subset.
        
        var results = await filteredListings
            .Select(l => new ListingResult
            {
                Id = l.Id,
                Description = l.Description,
                Price = l.Price,
                // Simulating the score calculation
                SimilarityScore = 0.95 // Placeholder for DB calculation
            })
            .OrderByDescending(r => r.SimilarityScore)
            .Take(20)
            .ToListAsync();

        return results;
    }

    // Interactive Challenge: Must Match vs Should Match
    public async Task<List<ListingResult>> SearchWithBoostAsync(
        string query,
        decimal maxPrice, // Must Match
        int minBedrooms,  // Must Match
        decimal? targetPriceRange = null) // Should Match (Boost)
    {
        var queryVector = GenerateQueryVector(query);
        var listings = _context.Set<Listing>().AsQueryable();

        // 1. Apply Strict Filters (Must Match)
        var filteredListings = listings
            .Where(l => l.Price < maxPrice && l.Bedrooms >= minBedrooms && l.DescriptionEmbedding != null);

        // 2. Calculate Base Similarity
        // In a real scenario, we might use a SQL scalar function for the boost logic.
        // Here, we simulate the logic in the projection.
        
        var results = await filteredListings
            .Select(l => new 
            { 
                Listing = l, 
                BaseSimilarity = 0.95, // Simulated DB vector similarity
                // Calculate a boost score based on "Should" criteria
                // Example: If targetPriceRange is provided, boost listings close to that price
                PriceBoost = (targetPriceRange.HasValue && Math.Abs(l.Price - targetPriceRange.Value) < 10000) 
                             ? 0.1 : 0 
            })
            .ToListAsync(); // Pulls filtered subset to memory for complex scoring if DB doesn't support custom functions

        // 3. Apply Weighted Ranking
        var rankedResults = results
            .Select(x => new ListingResult
            {
                Id = x.Listing.Id,
                Description = x.Listing.Description,
                Price = x.Listing.Price,
                // Weighted Sum: Base Similarity + Boost
                SimilarityScore = x.BaseSimilarity + x.PriceBoost
            })
            .OrderByDescending(r => r.SimilarityScore)
            .Take(20)
            .ToList();

        return rankedResults;
    }
}

public class ListingResult
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double SimilarityScore { get; set; }
}
