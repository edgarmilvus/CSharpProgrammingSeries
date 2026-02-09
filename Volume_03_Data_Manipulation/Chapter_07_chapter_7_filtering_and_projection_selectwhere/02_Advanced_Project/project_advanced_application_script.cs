
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        // SCENARIO: Preparing a dataset for a Machine Learning embedding task.
        // We have raw user data. We need to clean it, normalize it, and split it.
        // This script demonstrates a PURE FUNCTIONAL pipeline using LINQ.

        // 1. RAW DATA SOURCE
        // A collection of anonymous objects representing user records.
        // Note: Some records are 'dirty' (nulls, negative ages, empty names).
        var rawUserRecords = new[]
        {
            new { UserId = 101, Name = "Alice",   Age = 29, Score = 8.5, IsActive = true },
            new { UserId = 102, Name = "Bob",     Age = -5, Score = 7.2, IsActive = false }, // Dirty: Negative Age
            new { UserId = 103, Name = null,     Age = 35, Score = 9.1, IsActive = true },  // Dirty: Null Name
            new { UserId = 104, Name = "David",   Age = 42, Score = 6.8, IsActive = true },
            new { UserId = 105, Name = "Eve",     Age = 24, Score = null, IsActive = true }, // Dirty: Null Score
            new { UserId = 106, Name = "Frank",   Age = 18, Score = 5.4, IsActive = false },
            new { UserId = 107, Name = "Grace",   Age = 29, Score = 8.9, IsActive = true },
            new { UserId = 108, Name = "Heidi",   Age = 31, Score = 7.0, IsActive = false },
            new { UserId = 109, Name = "Ivan",    Age = 29, Score = 9.5, IsActive = true },
            new { UserId = 110, Name = "Judy",    Age = 12, Score = 4.2, IsActive = true }  // Dirty: Too young
        };

        // ============================================================
        // PIPELINE STAGE 1: CLEANING (Filtering & Null Handling)
        // ============================================================
        // We define predicates to isolate valid data.
        // We use 'Where' to filter out records that break our assumptions.
        
        // Predicate: Valid Age is between 18 and 100.
        Func<bool> isAgeValid = () => true; // Placeholder logic, applied inside the query below
        
        // The Cleaned Pipeline.
        // Note: We use 'from x in ...' syntax for readability, equivalent to .Where().
        // We also handle nulls inside the lambda.
        var cleanedPipeline = from record in rawUserRecords
                              where record.Name != null                 // Filter 1: Remove null names
                              where record.Age >= 18 && record.Age <= 100 // Filter 2: Validate Age
                              where record.Score.HasValue              // Filter 3: Ensure Score exists
                              select record;                           // Projection: Keep the object as is for now.

        // ============================================================
        // CRITICAL CONCEPT: DEFERRED EXECUTION
        // ============================================================
        // 'cleanedPipeline' is NOT a list yet. It is a 'query definition'.
        // No iteration has happened. If we add more .Where() clauses, they are chained efficiently.
        // To verify this, we can materialize it.
        var materializedCleanList = cleanedPipeline.ToList();

        Console.WriteLine($"[Debug] Raw Count: {rawUserRecords.Length}");
        Console.WriteLine($"[Debug] Cleaned Count (Materialized): {materializedCleanList.Count}");
        Console.WriteLine("---------------------------------------------------");

        // ============================================================
        // PIPELINE STAGE 2: NORMALIZATION (Projection / Mapping)
        // ============================================================
        // We transform the data into a 'Vector-like' format.
        // Inputs: Age (int), Score (double).
        // Output: A 2D Vector [NormalizedAge, NormalizedScore].
        // Normalization formula: (Value - Min) / (Max - Min).
        
        // Let's define constants for normalization (usually calculated dynamically, but hardcoded here for clarity).
        const int MinAge = 18;
        const int MaxAge = 100;
        const double MinScore = 4.2;
        const double MaxScore = 9.5;

        // The Transformation Query
        var embeddingsPipeline = cleanedPipeline.Select(record => 
        {
            // MATH LOGIC: Pure functional transformation inside Select.
            // We calculate normalized values.
            double normAge = (double)(record.Age - MinAge) / (MaxAge - MinAge);
            double normScore = (record.Score.Value - MinScore) / (MaxScore - MinScore);

            // Return a new object representing the embedding vector.
            return new 
            { 
                OriginalId = record.UserId, 
                Vector = new[] { normAge, normScore } // Creating the "Embedding"
            };
        });

        // ============================================================
        // PIPELINE STAGE 3: SHUFFLING (Ordering)
        // ============================================================
        // Machine Learning training requires shuffled data to prevent bias.
        // We use OrderBy with a Random generator.
        // WARNING: Using Random inside OrderBy is technically a side effect, 
        // but it is the standard functional pattern for shuffling.
        
        var rng = new Random();
        var shuffledPipeline = embeddingsPipeline.OrderBy(x => rng.Next());

        // ============================================================
        // PIPELINE STAGE 4: BATCHING (Grouping)
        // ============================================================
        // Let's say we want to split the data into batches of size 3 for training.
        // We will GroupBy the index of the item.
        // Note: This requires materializing the shuffled list first to get indices.
        
        var finalShuffledList = shuffledPipeline.ToList();
        
        // GroupBy logic: Index / BatchSize
        var batchedGroups = finalShuffledList
            .Select((item, index) => new { Item = item, BatchIndex = index / 3 })
            .GroupBy(x => x.BatchIndex)
            .Select(g => new { BatchId = g.Key, Data = g.Select(x => x.Item).ToList() });

        // ============================================================
        // OUTPUT GENERATION
        // ============================================================
        // Visualizing the pipeline results.
        
        Console.WriteLine("FINAL PREPROCESSED EMBEDDINGS (Batched & Shuffled):");
        Console.WriteLine("===================================================");
        
        // We iterate here ONLY to display results. 
        // The calculation logic above contained NO loops.
        foreach (var batch in batchedGroups)
        {
            Console.WriteLine($"Batch #{batch.BatchId}:");
            foreach (var user in batch.Data)
            {
                // Formatting the vector for display
                string vectorStr = $"[{user.Vector[0]:F2}, {user.Vector[1]:F2}]";
                Console.WriteLine($"  - User {user.OriginalId}: Vector {vectorStr}");
            }
            Console.WriteLine();
        }
    }
}
