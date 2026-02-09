
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

// Project: VectorStore.Services
// File: ProductSearchService.cs

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VectorEmbedding.Core;
using VectorStore.Data;
using VectorMath; // Dependency on Exercise 3

namespace VectorStore.Services
{
    public class ProductSearchService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ProductContext _dbContext;

        public ProductSearchService(IEmbeddingService embeddingService, ProductContext dbContext)
        {
            _embeddingService = embeddingService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Single query search with pre-filtering and in-memory vector calculation.
        /// </summary>
        public async Task<List<(Product Product, float Score)>> SearchProductsAsync(
            string query, 
            decimal minPrice, 
            int topK = 10)
        {
            // 1. Generate Embedding for the query
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(query);

            // 2. Pre-filtering using EF Core (Executes SQL WHERE clause)
            // This drastically reduces the dataset loaded into memory.
            var candidates = await _dbContext.Products
                .Where(p => p.Price >= minPrice)
                .ToListAsync(); // Materialize the filtered set

            // 3. In-Memory Vector Similarity Calculation
            var results = candidates
                .Select(p => new
                {
                    Product = p,
                    Score = p.Embedding.CosineSimilarity(queryVector)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();

            return results.Select(r => (r.Product, r.Score)).ToList();
        }

        /// <summary>
        /// Batch processing for multiple queries.
        /// </summary>
        public async Task<List<List<(Product Product, float Score)>>> SearchProductsBatchAsync(
            List<string> queries, 
            decimal minPrice, 
            int topK = 10)
        {
            // 1. Generate embeddings for all queries in parallel
            var queryTasks = queries.Select(q => _embeddingService.GenerateEmbeddingAsync(q)).ToList();
            var queryVectors = await Task.WhenAll(queryTasks);

            // 2. Fetch pre-filtered data ONCE
            var candidates = await _dbContext.Products
                .Where(p => p.Price >= minPrice)
                .ToListAsync();

            // 3. Process each query against the candidates
            var batchResults = new List<List<(Product Product, float Score)>>();

            foreach (var queryVector in queryVectors)
            {
                var results = candidates
                    .Select(p => (Product: p, Score: p.Embedding.CosineSimilarity(queryVector)))
                    .OrderByDescending(x => x.Score)
                    .Take(topK)
                    .ToList();
                
                batchResults.Add(results);
            }

            return batchResults;
        }
    }
}

// Project: VectorStore.Services
// File: SequenceDiagram.txt (Graphviz DOT)

/*
digraph SequenceDiagram {
    rankdir=LR;
    node [shape=box];

    Client [label="Client"];
    SearchService [label="ProductSearchService"];
    EmbeddingService [label="IEmbeddingService\n(External AI)"];
    DbContext [label="EF Core DbContext\n(Database)"];

    Client -> SearchService : SearchProductsBatchAsync(queries, minPrice)
    
    // Parallel Execution
    subgraph cluster_parallel {
        label = "Parallel Execution";
        SearchService -> EmbeddingService : GenerateEmbeddingAsync (Query 1)
        SearchService -> EmbeddingService : GenerateEmbeddingAsync (Query 2)
        SearchService -> EmbeddingService : GenerateEmbeddingAsync (Query N)
    }
    
    EmbeddingService -> SearchService : Return Vectors

    SearchService -> DbContext : Where(p => p.Price >= minPrice)
    DbContext -> SearchService : Return Filtered Products

    SearchService -> SearchService : In-Memory Cosine Similarity
    SearchService -> Client : Return Top K Results
}
*/
