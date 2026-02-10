
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

// 1. Data Model
public class TechnicalArticle
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    // Simulating a vector extension column (e.g., pgvector's vector type)
    // In a real scenario, this would be mapped to a specific database type
    [Column(TypeName = "vector(384)")] 
    public float[]? Embedding { get; set; }
}

public class SearchContext : DbContext
{
    public DbSet<TechnicalArticle> Articles { get; set; }

    public SearchContext(DbContextOptions<SearchContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the vector index if supported by the provider
        // Example for PostgreSQL with pgvector:
        // modelBuilder.Entity<TechnicalArticle>()
        //     .HasIndex(a => a.Embedding)
        //     .HasMethod("ivfflat")
        //     .HasOperators("vector_l2_ops");
    }
}

// 2. Search Service
public class HybridSearchService
{
    private readonly SearchContext _context;
    private const double RrfK = 60.0; // Constant for RRF formula

    public HybridSearchService(SearchContext context)
    {
        _context = context;
    }

    // Mock function to generate query embedding (simulating an AI service)
    private float[] GenerateQueryEmbedding(string query)
    {
        // Deterministic "hash" based on query length for simulation
        // In reality, this calls an embedding model (e.g., Azure OpenAI)
        var seed = query.Length;
        var rng = new Random(seed);
        var embedding = new float[384];
        rng.NextBytes(embedding.SelectMany(BitConverter.GetBytes).ToArray());
        return embedding;
    }

    // Helper to calculate Cosine Similarity
    // Note: PostgreSQL pgvector handles this natively via <#> operator, 
    // but we simulate the logic here for the solution structure.
    private double CalculateCosineSimilarity(float[] v1, float[] v2)
    {
        if (v1 == null || v2 == null || v1.Length != v2.Length) return 0;
        
        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < v1.Length; i++)
        {
            dotProduct += v1[i] * v2[i];
            normA += v1[i] * v1[i];
            normB += v2[i] * v2[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    public async Task<List<SearchResult>> SearchAsync(string query, SearchMode mode, double alpha = 0.5)
    {
        var queryEmbedding = GenerateQueryEmbedding(query);

        // 1. Keyword Search (Simulating Full-Text Search)
        // In a real DB (PostgreSQL), we would use: 
        // .Where(a => EF.Functions.ToTsVector("english", a.Title + " " + a.Content).Matches(EF.Functions.ToTsQuery("english", query)))
        // Here we simulate ranking by simple word frequency for the demo.
        var keywordTask = _context.Articles
            .Where(a => a.Title.Contains(query) || a.Content.Contains(query))
            .Select(a => new 
            { 
                Article = a, 
                // Simulated rank: lower number is better (1 is best)
                Rank = 1 + _context.Articles
                    .Count(b => (b.Title.Contains(query) || b.Content.Contains(query)) && 
                                (b.Title.Length + b.Content.Length < a.Title.Length + a.Content.Length)) 
            })
            .Take(50)
            .ToListAsync();

        // 2. Vector Search (Simulating Cosine Similarity)
        // In a real DB (PostgreSQL), we would use:
        // .Where(a => a.Embedding != null)
        // .OrderByDescending(a => EF.Functions.CosineDistance(a.Embedding, queryEmbedding))
        // Since we are in memory for this simulation, we calculate manually.
        var vectorTask = _context.Articles
            .Where(a => a.Embedding != null)
            .Select(a => new 
            { 
                Article = a, 
                Similarity = CalculateCosineSimilarity(a.Embedding!, queryEmbedding) 
            })
            .OrderByDescending(x => x.Similarity)
            .Take(50)
            .ToListAsync();

        await Task.WhenAll(keywordTask, vectorTask);

        var keywordResults = await keywordTask;
        var vectorResults = await vectorTask;

        // 3. RRF Fusion
        var rrfScores = new Dictionary<int, double>();

        // Process Keyword Results
        foreach (var item in keywordResults)
        {
            // RRF Formula: 1 / (k + rank)
            double score = 1.0 / (RrfK + item.Rank);
            
            // Interactive Challenge: Adjust weight by Alpha (0.0 = pure keyword, 1.0 = pure vector)
            // We apply alpha to the RRF contribution.
            // If Alpha is 0, we rely heavily on keyword (1-alpha). If Alpha is 1, rely on vector.
            // However, RRF is rank-based, not score-based. 
            // To implement alpha here, we scale the RRF contribution.
            double weightedScore = score * (1.0 - alpha);

            if (!rrfScores.ContainsKey(item.Article.Id))
                rrfScores[item.Article.Id] = 0;
            
            rrfScores[item.Article.Id] += weightedScore;
        }

        // Process Vector Results
        foreach (var item in vectorResults)
        {
            // For vector, we don't have a "rank" in the traditional sense for RRF, 
            // but we can assign rank based on the sorted order (1 to 50).
            int rank = vectorResults.IndexOf(item) + 1;
            double score = 1.0 / (RrfK + rank);
            
            double weightedScore = score * alpha; // Apply alpha weighting

            if (!rrfScores.ContainsKey(item.Article.Id))
                rrfScores[item.Article.Id] = 0;
            
            rrfScores[item.Article.Id] += weightedScore;
        }

        // 4. Final Ranking and Output
        var finalResults = rrfScores
            .Select(kvp => new SearchResult 
            { 
                Id = kvp.Key, 
                Score = kvp.Value 
            })
            .OrderByDescending(r => r.Score)
            .Take(20)
            .ToList();

        // Hydrate titles for display (simulating a join)
        var ids = finalResults.Select(r => r.Id).ToList();
        var articles = await _context.Articles
            .Where(a => ids.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Title);

        foreach (var result in finalResults)
        {
            if (articles.TryGetValue(result.Id, out var title))
                result.Title = title;
        }

        return finalResults;
    }
}

public enum SearchMode { Keyword, Vector, Hybrid }

public class SearchResult
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Score { get; set; }
}
