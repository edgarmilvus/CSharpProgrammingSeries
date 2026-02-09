
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
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Diagnostics;

// 1. Migration Strategy (Raw SQL)
/*
   -- Ideally, you add this to your EF Core migration's Up() method using migrationBuilder.Sql()
   -- Note: HNSW requires the 'vector' extension to be enabled first.
   
   CREATE INDEX idx_product_embedding_hnsw 
   ON products 
   USING hnsw (embedding vector_cosine_ops)
   WITH (m = 16, ef_construction = 64);
   
   -- Parameters:
   -- m: The number of connections per layer (16 is a good default, higher = better recall but slower build).
   -- ef_construction: The size of the dynamic candidate list for construction (64 is standard, higher = better quality but slower build).
*/

// 2. Query Refactoring
public class PerformanceOptimizer
{
    private readonly AppDbContext _context;

    public PerformanceOptimizer(AppDbContext context) => _context = context;

    public async Task<List<ProductDto>> SearchWithIndex(string queryText)
    {
        // The query structure remains the same as Exercise 1.
        // pgvector automatically uses the HNSW index if the query involves an ORDER BY with a distance function.
        // It does NOT use the index if you just do a WHERE distance < threshold without ORDER BY.
        
        float[] queryVector = GenerateMockEmbedding(queryText, 384);

        // Ensure the query is ordered by distance for index usage
        return await _context.Products
            .OrderBy(p => EF.Functions.VectorCosineDistance(p.Embedding, new Vector(queryVector)))
            .Select(p => new ProductDto
            {
                Name = p.Name,
                Price = p.Price,
                SimilarityScore = 1 - EF.Functions.VectorCosineDistance(p.Embedding, new Vector(queryVector))
            })
            .Take(10)
            .ToListAsync();
    }

    // 3. Performance Analysis
    public async Task ComparePerformance()
    {
        // NOTE: To accurately test this, you would programmatically drop/create the index 
        // or have two identical tables (one indexed, one not).
        
        Console.WriteLine("Starting Performance Benchmark...");
        var iterations = 100;
        var sw = new Stopwatch();

        // Scenario A: With Index (Assuming index exists)
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            await SearchWithIndex("test query");
        }
        sw.Stop();
        Console.WriteLine($"Avg Time with HNSW Index: {sw.ElapsedMilliseconds / (double)iterations:F2} ms");

        // Scenario B: Without Index (Simulated by forcing a sequential scan hint or dropping index)
        // In PostgreSQL, you can use 'SET enable_seqscan = off;' to force index usage, 
        // or drop the index to force sequential scan.
        
        /* 
        // Example of forcing a sequential scan (for testing purposes only):
        await _context.Database.ExecuteSqlRawAsync("SET enable_seqscan = OFF;"); 
        // Run test...
        await _context.Database.ExecuteSqlRawAsync("SET enable_seqscan = ON;");
        */
    }

    // 4. Architectural Implication: Setting ef_search
    public async Task SetSearchEf(int efSearchValue)
    {
        // This sets the parameter for the current session/connection.
        // It controls the size of the dynamic candidate list during search.
        // Higher = more accurate but slower. Lower = faster but potentially less accurate.
        await _context.Database.ExecuteSqlRawAsync($"SET hnsw.ef_search = {efSearchValue};");
    }

    private float[] GenerateMockEmbedding(string text, int dimensions)
    {
        var rng = new Random(text.Length);
        var vector = new float[dimensions];
        for (int i = 0; i < dimensions; i++) vector[i] = (float)rng.NextDouble();
        return vector;
    }
}
