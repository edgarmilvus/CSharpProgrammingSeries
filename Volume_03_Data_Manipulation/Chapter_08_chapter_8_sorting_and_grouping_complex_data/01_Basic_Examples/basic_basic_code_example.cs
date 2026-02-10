
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class BasicSortingAndGrouping
{
    public static void Main()
    {
        // --- DATA SOURCE ---
        // Simulating a raw dataset of user interactions.
        // In a real AI pipeline, this might come from a CSV, JSON API, or database.
        // We are modeling heterogeneous data: strings, numbers, and categories.
        List<UserInteraction> rawData = new List<UserInteraction>
        {
            new UserInteraction { UserId = "User_A", SessionId = 101, Action = "Click", DurationSeconds = 5, Score = 0.95 },
            new UserInteraction { UserId = "User_B", SessionId = 102, Action = "View",  DurationSeconds = 12, Score = 0.40 },
            new UserInteraction { UserId = "User_A", SessionId = 103, Action = "View",  DurationSeconds = 8,  Score = 0.60 },
            new UserInteraction { UserId = "User_C", SessionId = 104, Action = "Click", DurationSeconds = 2,  Score = 0.20 },
            new UserInteraction { UserId = "User_B", SessionId = 102, Action = "Scroll",DurationSeconds = 4,  Score = 0.80 },
            null // Handling dirty data
        };

        // --- PIPELINE EXECUTION ---
        // We define the query using functional composition.
        // Note: This is Deferred Execution. No processing happens yet.
        var preprocessingQuery = rawData
            // 1. CLEANING: Filter out nulls (Data Hygiene)
            .Where(interaction => interaction != null)

            // 2. FILTERING: Focus on high-value interactions (Signal vs Noise)
            .Where(interaction => interaction.Score > 0.5)

            // 3. SORTING: Primary by UserId, Secondary by Score (Descending)
            // This creates a deterministic order for the pipeline.
            .OrderBy(interaction => interaction.UserId)
            .ThenByDescending(interaction => interaction.Score)

            // 4. TRANSFORMATION: Project into a simplified structure (Normalization)
            .Select(interaction => new ProcessedRecord
            {
                NormalizedUser = interaction.UserId.ToUpper(),
                WeightedDuration = interaction.DurationSeconds * interaction.Score,
                Category = interaction.Action
            });

        // --- IMMEDIATE EXECUTION ---
        // We force execution here by materializing the results into a list.
        // In a real scenario, this might feed into a Vector Database or ML model.
        List<ProcessedRecord> cleanData = preprocessingQuery.ToList();

        // --- OUTPUT ---
        Console.WriteLine($"Original Count: {rawData.Count}");
        Console.WriteLine($"Cleaned Count:  {cleanData.Count}\n");

        Console.WriteLine("Sorted & Normalized Data:");
        Console.WriteLine("User     | Weighted | Action");
        Console.WriteLine("---------|----------|-------");
        
        // Using LINQ Aggregate instead of foreach to remain functional
        var outputString = cleanData.Aggregate("", (current, record) => 
            current + $"{record.NormalizedUser,-9}| {record.WeightedDuration,-9:F2}| {record.Category}\n");
        
        Console.WriteLine(outputString.Trim());

        // --- GROUPING EXAMPLE ---
        // Grouping is a separate, often terminal, operation in the pipeline.
        var groupedData = cleanData
            .GroupBy(record => record.Category)
            .Select(group => new 
            {
                Action = group.Key,
                Count = group.Count(),
                AverageWeightedDuration = group.Average(r => r.WeightedDuration)
            })
            .ToList();

        Console.WriteLine("\nAggregated Group Statistics:");
        groupedData.ForEach(g => 
            Console.WriteLine($"Action: {g.Action}, Count: {g.Count}, Avg Weighted Duration: {g.AverageWeightedDuration:F2}"));
    }
}

// --- DATA MODELS ---
// Pure data containers (POCOs) to represent the heterogeneous structure.
public class UserInteraction
{
    public string UserId { get; set; }
    public int SessionId { get; set; }
    public string Action { get; set; }
    public int DurationSeconds { get; set; }
    public double Score { get; set; }
}

public class ProcessedRecord
{
    public string NormalizedUser { get; set; }
    public double WeightedDuration { get; set; }
    public string Category { get; set; }
}
