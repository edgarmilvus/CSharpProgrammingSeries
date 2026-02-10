
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent; // For Priority Queue simulation

namespace Chapter9.Exercise4
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        // In Memory, we might store as string to mimic DB serialization, 
        // or just keep as float[] for ease of testing. 
        // Let's keep float[] for logic testing.
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    public class InMemoryVectorDbContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("VectorTestDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // We can't use the pgvector converter here directly if we want to query it easily in memory,
            // but we can simulate the storage format if needed. 
            // For unit testing logic, keeping it as a raw array is usually sufficient 
            // to test the distance algorithms.
        }
    }

    public class VectorSearchService
    {
        private readonly InMemoryVectorDbContext _context;

        public VectorSearchService(InMemoryVectorDbContext context)
        {
            _context = context;
        }

        // 4. Brute Force Search (Standard LINQ)
        public async Task<List<Document>> SearchBruteForceAsync(float[] queryVector, int topK)
        {
            var docs = await _context.Documents.ToListAsync();
            
            // O(N log N) due to sorting
            return docs
                .Select(d => new { Doc = d, Dist = EuclideanDistance(d.Embedding, queryVector) })
                .OrderBy(x => x.Dist)
                .Take(topK)
                .Select(x => x.Doc)
                .ToList();
        }

        // Interactive Challenge: Optimized Search with Priority Queue
        // O(N log K) complexity
        public async Task<List<Document>> SearchOptimizedAsync(float[] queryVector, int topK)
        {
            var docs = await _context.Documents.ToListAsync();
            
            // Simulating a Min-Heap (Priority Queue) behavior using a SortedSet or custom logic.
            // In .NET 6+, we would use PriorityQueue<TElement, TPriority>.
            // Here we implement a manual logic to demonstrate the concept.
            
            // We want to keep the TOP K *smallest* distances.
            // A Max-Heap of size K is best for this (store the worst of the best).
            // Since we don't have a built-in MaxHeap, we'll use a SortedList to simulate.
            
            var topCandidates = new SortedList<double, Document>(); // Key: Distance, Value: Doc
            
            foreach (var doc in docs)
            {
                double dist = EuclideanDistance(doc.Embedding, queryVector);
                
                if (topCandidates.Count < topK)
                {
                    topCandidates.Add(dist, doc);
                }
                else 
                {
                    // If the list is full, check if this doc is closer than the furthest one in our list
                    var maxDist = topCandidates.Keys[topCandidates.Count - 1];
                    if (dist < maxDist)
                    {
                        topCandidates.RemoveAt(topCandidates.Count - 1);
                        topCandidates.Add(dist, doc);
                    }
                }
            }

            return topCandidates.Values.ToList();
        }

        private double EuclideanDistance(float[] a, float[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++) sum += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(sum);
        }

        // 3. Test Data Seeding
        public async Task SeedDataAsync(int count, int dimensions)
        {
            var random = new Random(42); // Seeded for determinism
            var docs = new List<Document>();
            
            for (int i = 0; i < count; i++)
            {
                var embedding = new float[dimensions];
                for (int j = 0; j < dimensions; j++)
                {
                    embedding[j] = (float)random.NextDouble();
                }
                
                docs.Add(new Document 
                { 
                    Id = Guid.NewGuid(), 
                    Title = $"Doc {i}", 
                    Embedding = embedding 
                });
            }

            _context.Documents.AddRange(docs);
            await _context.SaveChangesAsync();
        }
    }
}
