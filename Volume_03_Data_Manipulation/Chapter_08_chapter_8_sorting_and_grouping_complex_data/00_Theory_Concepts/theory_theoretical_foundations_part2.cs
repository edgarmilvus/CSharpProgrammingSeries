
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class AiDataPipeline
{
    // A simple record to represent a raw data row
    public record RawData(string Id, string Text, double? Label);

    public static void ProcessData(IEnumerable<RawData> rawData)
    {
        // DEFERRED EXECUTION: The entire pipeline is defined here.
        // No data is processed yet.
        var processedPipeline = rawData
            // 1. Filtering: Keep only rows with valid labels and non-empty text
            .Where(row => row.Label.HasValue && !string.IsNullOrWhiteSpace(row.Text))
            
            // 2. Normalization & Transformation: Project to a new shape
            .Select(row => new
            {
                Id = row.Id,
                // Simple normalization: lowercase and remove punctuation (conceptual)
                NormalizedText = row.Text.ToLowerInvariant().Replace(".", ""),
                Label = row.Label.Value
            })
            
            // 3. Partitioning: Split into training and validation sets (e.g., 80/20)
            // This is still deferred. We are just defining the logic.
            .Select((item, index) => new { Item = item, Index = index })
            .GroupBy(x => x.Index < (rawData.Count() * 0.8) ? "Train" : "Validation")
            .SelectMany(g => g.Select(x => new { Set = g.Key, x.Item }));

        // ... At this point, memory usage is minimal. We haven't loaded or processed any text.

        // IMMEDIATE EXECUTION: We decide when to materialize the results.
        // For example, we might want to write the training set to a file.
        var trainingSet = processedPipeline
            .Where(x => x.Set == "Train")
            .Select(x => x.Item)
            .ToList(); // Forces execution for the training set

        // Now we can process the training set
        foreach (var item in trainingSet)
        {
            // Console.WriteLine($"Training Item: {item.Id}, Label: {item.Label}");
            // In a real scenario, this is where you would feed the data to a model.
        }
    }
}
