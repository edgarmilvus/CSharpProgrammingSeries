
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

public class BoostedProductService
{
    private readonly AppDbContext _context;

    public BoostedProductService(AppDbContext context) => _context = context;

    public async Task<List<ProductDto>> SearchProductsWithBoost(string queryText, string category, decimal maxPrice, int minStock)
    {
        float[] queryVector = GenerateMockEmbedding(queryText, 384);

        // Base query with standard filters
        var query = _context.Products
            .Where(p => p.Price <= maxPrice && p.StockQuantity >= minStock);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        // --- IMPLEMENTING SCORE BOOSTING ---
        // We project the base similarity score first.
        // Then we calculate a BoostedScore in the Select projection.
        // We must OrderBy this BoostedScore.
        
        var boostedQuery = query
            .Select(p => new
            {
                Product = p,
                // 1. Calculate Base Similarity (1 - Distance)
                BaseSimilarity = 1 - EF.Functions.VectorCosineDistance(p.Embedding, new Vector(queryVector)),
            })
            .Select(x => new
            {
                x.Product,
                x.BaseSimilarity,
                // 2. Apply Boost Logic
                // Formula: Boosted = Base + (Category == "Sale" ? 0.1 : 0)
                // We use a CASE statement in SQL implicitly via the ternary operator
                BoostedScore = x.BaseSimilarity + (x.Product.Category == "Sale" ? 0.1m : 0m)
            })
            // 3. Dynamic Sorting by Boosted Score
            .OrderByDescending(x => x.BoostedScore)
            // 4. Edge Case - Ties: Add secondary sort
            .ThenBy(x => x.Product.Id) 
            .Select(x => new ProductDto
            {
                Name = x.Product.Name,
                Price = x.Product.Price,
                SimilarityScore = (double)x.BoostedScore // Return the boosted value
            });

        return await boostedQuery.Take(20).ToListAsync();
    }

    private float[] GenerateMockEmbedding(string text, int dimensions)
    {
        var rng = new Random(text.Length);
        var vector = new float[dimensions];
        for (int i = 0; i < dimensions; i++) vector[i] = (float)rng.NextDouble();
        return vector;
    }
}
