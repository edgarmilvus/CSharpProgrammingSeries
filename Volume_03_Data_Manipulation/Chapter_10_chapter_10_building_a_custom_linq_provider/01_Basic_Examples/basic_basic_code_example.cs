
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// Define a simple class to represent a data point with an embedding vector
public class DataPoint
{
    public string Id { get; set; }
    public double[] Embedding { get; set; }
    public string Category { get; set; }
}

public class Program
{
    public static void Main()
    {
        // --- 1. Data Simulation (The "Real-World" Context) ---
        // Imagine a RAG (Retrieval-Augmented Generation) system where we have
        // thousands of document chunks represented as high-dimensional vectors.
        // We need to preprocess this data: filter out noise, normalize values, 
        // and prepare it for a vector similarity search.
        
        // Raw, unprocessed dataset
        IEnumerable<DataPoint> rawData = new List<DataPoint>
        {
            new DataPoint { Id = "A", Embedding = new[] { 1.0, 0.5, 0.0 }, Category = "Tech" },
            new DataPoint { Id = "B", Embedding = new[] { 1.1, 0.6, 0.1 }, Category = "Tech" }, // Similar to A
            new DataPoint { Id = "C", Embedding = new[] { 0.0, 0.0, 0.0 }, Category = "Noise" }, // Zero vector (bad data)
            new DataPoint { Id = "D", Embedding = new[] { 5.0, 8.0, 2.0 }, Category = "Finance" }
        };

        // --- 2. The Functional Pipeline (Declarative Style) ---
        // We define the transformation steps. Note: Nothing executes yet.
        // This is "Deferred Execution". We are building a recipe, not cooking the meal.
        
        var preprocessingQuery = 
            // Step A: Filter out "Noise" categories and zero vectors (Data Cleaning)
            rawData.Where(dp => dp.Category != "Noise" && dp.Embedding.Any(v => v > 0))
            
            // Step B: Normalize the vectors (Data Normalization)
            // We project the existing object into a NEW object with modified data.
            // CRITICAL: We do not modify the original 'rawData'. This is immutability.
            .Select(dp => 
            {
                double magnitude = Math.Sqrt(dp.Embedding.Sum(v => v * v));
                return new DataPoint 
                { 
                    Id = dp.Id, 
                    Category = dp.Category,
                    // Functional transformation: Map vector -> normalized vector
                    Embedding = dp.Embedding.Select(v => v / magnitude).ToArray() 
                };
            })
            
            // Step C: Group by Category (Aggregation)
            .GroupBy(dp => dp.Category);

        // --- 3. Triggering Execution ---
        // The query is still just a definition. 
        // We force execution by iterating or converting to a list.
        
        Console.WriteLine("--- Preprocessing Results ---");
        
        // Immediate Execution: .ToList() forces the pipeline to run now.
        var processedGroups = preprocessingQuery.ToList();

        foreach (var group in processedGroups)
        {
            Console.WriteLine($"Category: {group.Key}");
            foreach (var dp in group)
            {
                // Formatting the vector for display
                string vecStr = string.Join(", ", dp.Embedding.Select(v => v.ToString("F2")));
                Console.WriteLine($"  - ID: {dp.Id}, Vector: [{vecStr}]");
            }
        }
    }
}
