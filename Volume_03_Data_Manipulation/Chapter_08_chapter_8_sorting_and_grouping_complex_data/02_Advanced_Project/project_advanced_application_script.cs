
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Book3_Chapter8_AdvancedApplication
{
    // REAL-WORLD CONTEXT:
    // Imagine you are a Data Engineer at a streaming service (e.g., Netflix/Spotify).
    // You have received a raw batch of user interaction logs. The data is messy:
    // 1. It contains "null" or invalid entries (corrupted logs).
    // 2. It contains duplicates (users clicking "like" twice rapidly).
    // 3. It contains complex nested objects (User profiles, Content metadata).
    // 4. It contains vector embeddings (numerical representations of content).
    //
    // OBJECTIVE:
    // Build a functional data preprocessing pipeline using LINQ to:
    // 1. Clean and normalize the data.
    // 2. Group interactions by user to calculate aggregate statistics.
    // 3. Sort users by their "affinity" (calculated score).
    // 4. Perform vectorized operations (simulated) to find content similarity.
    //
    // CONSTRAINTS:
    // - No imperative loops (foreach/for).
    // - No side effects in queries.
    // - Strict use of Deferred vs Immediate execution.

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("--- Starting Data Preprocessing Pipeline ---");

            // 1. GENERATE RAW DATA
            // In a real scenario, this comes from a database or file stream.
            var rawLogs = GenerateMockData();

            // ==========================================
            // STAGE 1: CLEANING & FILTERING (WHERE)
            // ==========================================
            // DEFERRED EXECUTION DEMONSTRATION:
            // The query 'cleanedDataQuery' is defined here but NOT executed yet.
            // This allows us to chain multiple operations without intermediate allocations.
            var cleanedDataQuery = rawLogs
                .Where(log => log != null) // Filter out null entries
                .Where(log => log.UserId != Guid.Empty) // Filter out invalid users
                .Where(log => log.InteractionScore >= 0.0f); // Filter out negative scores

            // ==========================================
            // STAGE 2: DEDUPLICATION (DISTINCT)
            // ==========================================
            // We define a custom comparer for InteractionLog to handle uniqueness
            // based on User + Content + Timestamp, rather than object reference.
            var distinctLogsQuery = cleanedDataQuery.Distinct(new InteractionLogComparer());

            // ==========================================
            // STAGE 3: IMMEDIATE EXECUTION
            // ==========================================
            // We trigger execution here to materialize the data.
            // This is necessary because we need a concrete list for the next steps
            // or if we want to avoid re-querying the source (e.g., a network stream).
            List<InteractionLog> processedData = distinctLogsQuery.ToList();

            Console.WriteLine($"Data reduced from {rawLogs.Count} raw items to {processedData.Count} distinct items.");

            // ==========================================
            // STAGE 4: GROUPING & AGGREGATION (GROUPBY)
            // ==========================================
            // We group by UserId to analyze behavior per user.
            // We project into a new anonymous type containing calculated stats.
            var userProfilesQuery = processedData
                .GroupBy(log => log.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    // Aggregate: Sum of scores (Total Affinity)
                    TotalAffinity = group.Sum(log => log.InteractionScore),
                    // Aggregate: Count distinct content viewed
                    UniqueContentViews = group.Select(log => log.ContentId).Distinct().Count(),
                    // Aggregate: Average duration (if available)
                    AvgDuration = group.Average(log => log.DurationSeconds),
                    // Capture the raw logs for this user for further processing
                    RawLogs = group.ToList() 
                });

            // ==========================================
            // STAGE 5: SORTING (ORDERBY / THENBY)
            // ==========================================
            // Sorting by a calculated metric (TotalAffinity) descending.
            // ThenBy is used for secondary sorting (UserId) to ensure stable ordering.
            var sortedUsersQuery = userProfilesQuery
                .OrderByDescending(u => u.TotalAffinity)
                .ThenBy(u => u.UserId);

            // Execute immediately to display results
            var topUsers = sortedUsersQuery.ToList();

            // ==========================================
            // STAGE 6: VECTOR OPERATIONS (SIMULATED)
            // ==========================================
            // Real-world AI Context: Content Embeddings.
            // We have a "Content Vector Database". We want to find similar content
            // for our top users based on their interaction history.
            // Since we cannot use external libraries like NumPy/PyTorch in this pure C# script,
            // we simulate vector operations using LINQ's functional projection.

            Console.WriteLine("\n--- Calculating Content Similarity (Vector Projection) ---");

            // Define a static "Vector Database" (In reality, this is a billion-row matrix)
            var contentVectorDb = GetMockContentVectors();

            // For the top user, find content they interacted with, and find similar content
            // by calculating Euclidean Distance (simulated via LINQ Select/Sum).
            if (topUsers.Any())
            {
                var topUser = topUsers.First();
                
                // Get vectors of content the user interacted with
                var userContentVectors = topUser.RawLogs
                    .Select(log => contentVectorDb.FirstOrDefault(c => c.ContentId == log.ContentId))
                    .Where(vec => vec != null) // Filter out content not in DB
                    .ToList();

                // Find similar content by comparing vectors
                // This is a "Nearest Neighbor" lookup simulation.
                var recommendedContentQuery = contentVectorDb
                    // Exclude content the user has already seen
                    .Where(dbItem => !userContentVectors.Any(uv => uv.ContentId == dbItem.ContentId))
                    // Calculate "Distance" (Similarity Score) using Euclidean distance formula
                    // Euclidean Distance = Sqrt(Sum((a_i - b_i)^2))
                    .Select(dbItem => new
                    {
                        Content = dbItem,
                        // We project the vector calculation into the result
                        Distance = Math.Sqrt(
                            userContentVectors
                                .SelectMany(uv => uv.Vector) // Flatten user vectors to compare against this one
                                .Select((dimension, index) => 
                                {
                                    // This is a complex projection. We take the dimension from the DB item
                                    // and compare it to the average dimension of user's history.
                                    // Note: In a real tensor library, this is a matrix multiplication.
                                    // Here we simulate it by averaging the user's dimensions first.
                                    var avgUserDim = userContentVectors.Average(uv => uv.Vector[index]);
                                    var dbDim = dbItem.Vector[index];
                                    return Math.Pow(dbDim - avgUserDim, 2);
                                })
                                .Sum()
                        )
                    })
                    .OrderBy(r => r.Distance) // Sort by smallest distance (most similar)
                    .Take(5); // Top 5 recommendations

                // Execute
                var recommendations = recommendedContentQuery.ToList();

                Console.WriteLine($"\nTop User: {topUser.UserId}");
                Console.WriteLine($"Recommendations based on Vector Similarity:");
                foreach (var rec in recommendations)
                {
                    Console.WriteLine($" - Content: {rec.Content.ContentId}, Similarity Score: {rec.Distance:F4}");
                }
            }

            // ==========================================
            // STAGE 7: PARTITIONING (SKIP / TAKE)
            // ==========================================
            // Pagination logic using LINQ.
            int pageSize = 3;
            int pageNumber = 1;
            
            var paginatedUsers = sortedUsersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Console.WriteLine($"\n--- Page {pageNumber} of Top Users (Pagination) ---");
            foreach (var user in paginatedUsers)
            {
                Console.WriteLine($"User: {user.UserId} | Affinity: {user.TotalAffinity:F2}");
            }
        }

        // ==========================================
        // MOCK DATA GENERATORS
        // ==========================================
        static List<InteractionLog> GenerateMockData()
        {
            var random = new Random();
            var users = Enumerable.Range(0, 10).Select(i => Guid.NewGuid()).ToList();
            var contents = Enumerable.Range(1, 20).Select(i => i).ToList();

            // Generate 50 logs, including some nulls and duplicates
            var logs = new List<InteractionLog>();
            for (int i = 0; i < 50; i++)
            {
                // Introduce random nulls
                if (i % 10 == 0) 
                {
                    logs.Add(null); 
                    continue;
                }

                // Introduce duplicates (repeat same user/content)
                int userIndex = i % 5; 
                int contentIndex = random.Next(0, 20);

                logs.Add(new InteractionLog
                {
                    UserId = users[userIndex],
                    ContentId = contents[contentIndex],
                    InteractionScore = (float)random.NextDouble() * 100,
                    DurationSeconds = random.Next(10, 600),
                    Timestamp = DateTime.Now.AddMinutes(-i)
                });
            }
            return logs;
        }

        static List<ContentVector> GetMockContentVectors()
        {
            // Simulating a vector database (e.g., 3-dimensional embeddings)
            return new List<ContentVector>
            {
                new ContentVector { ContentId = 1, Vector = new float[] { 0.1f, 0.2f, 0.3f } },
                new ContentVector { ContentId = 2, Vector = new float[] { 0.9f, 0.8f, 0.7f } },
                new ContentVector { ContentId = 3, Vector = new float[] { 0.4f, 0.4f, 0.4f } },
                new ContentVector { ContentId = 4, Vector = new float[] { 0.15f, 0.25f, 0.35f } }, // Similar to 1
                new ContentVector { ContentId = 5, Vector = new float[] { 0.5f, 0.1f, 0.9f } }
            };
        }
    }

    // ==========================================
    // DATA MODELS
    // ==========================================
    public class InteractionLog
    {
        public Guid UserId { get; set; }
        public int ContentId { get; set; }
        public float InteractionScore { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ContentVector
    {
        public int ContentId { get; set; }
        public float[] Vector { get; set; } // Simulating an embedding
    }

    // ==========================================
    // CUSTOM COMPARER FOR DEDUPLICATION
    // ==========================================
    // Allows Distinct() to work on complex objects based on business logic
    // rather than memory reference equality.
    public class InteractionLogComparer : IEqualityComparer<InteractionLog>
    {
        public bool Equals(InteractionLog x, InteractionLog y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            // Define uniqueness: Same User, Same Content, within 1 minute window
            return x.UserId == y.UserId && 
                   x.ContentId == y.ContentId && 
                   Math.Abs((x.Timestamp - y.Timestamp).TotalMinutes) < 1;
        }

        public int GetHashCode(InteractionLog obj)
        {
            // Combine hash codes of the properties used in Equals
            return HashCode.Combine(obj.UserId, obj.ContentId);
        }
    }
}
