
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

public class DataPreprocessing
{
    // Represents a raw user interaction event
    public record UserInteraction(string UserId, int ItemId, double RawValue, DateTime Timestamp);

    public static void Main()
    {
        // 1. Simulate Raw Data (Dirty, Unsorted)
        var rawData = new List<UserInteraction>
        {
            new("UserA", 101, 500.0, DateTime.Now.AddDays(-10)),
            new("UserB", 102, 0.0,   DateTime.Now.AddDays(-5)),  // Invalid: RawValue is 0
            new("UserA", 103, 250.0, DateTime.Now.AddDays(-2)),
            new("UserC", 101, 100.0, DateTime.Now.AddDays(-1)),
            new("UserB", 104, 750.0, DateTime.Now.AddDays(-8)),  // Invalid: RawValue > 700 (outlier)
            new("UserA", 101, 300.0, DateTime.Now.AddDays(-3))
        };

        Console.WriteLine("--- Step 1: Defining the Query (Deferred Execution) ---");
        
        // 2. Define the Functional Pipeline
        // This query is NOT executed yet. It is a blueprint of instructions.
        var preprocessingPipeline = rawData
            .Where(interaction => interaction.RawValue > 0 && interaction.RawValue <= 700) // Clean: Filter outliers/zeros
            .Select(interaction => interaction with 
            { 
                // Normalize: Scale RawValue to a 0.0-1.0 range (assuming max is 700)
                RawValue = interaction.RawValue / 700.0 
            })
            .OrderBy(interaction => Guid.NewGuid()); // Shuffle: Randomize order

        Console.WriteLine("Query defined. No processing has occurred yet.");
        Console.WriteLine($"Type of 'preprocessingPipeline': {preprocessingPipeline.GetType().Name}\n");

        // 3. Immediate Execution (Materialization)
        // The pipeline executes here. We iterate over the results to force execution.
        // .ToList() creates a concrete list in memory.
        var processedData = preprocessingPipeline.ToList();

        Console.WriteLine("--- Step 2: Execution Results ---");
        Console.WriteLine($"Original Count: {rawData.Count}");
        Console.WriteLine($"Processed Count: {processedData.Count} (2 invalid items removed)\n");

        // 4. Displaying the processed data
        // We use a functional projection to format the output string
        var outputLines = processedData
            .Select((item, index) => $"{index + 1}. User: {item.UserId}, Item: {item.ItemId}, Normalized Value: {item.RawValue:F4}")
            .ToList();

        outputLines.ForEach(Console.WriteLine);
    }
}
