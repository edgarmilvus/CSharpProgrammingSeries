
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

public class DataPreprocessingPipeline
{
    public static void Main()
    {
        // 1. IMMEREDIATE EXECUTION: 
        // The .ToList() call forces the query to execute immediately.
        // This creates a concrete List<T> in memory, capturing the state at this moment.
        // In a real scenario, this might be a database call or a file read.
        List<UserData> rawData = GenerateRawData().ToList();
        Console.WriteLine("--- Raw Data (Snapshot) ---");
        rawData.ForEach(d => Console.WriteLine(d));

        // 2. DEFERRED EXECUTION:
        // This query is NOT executed yet. It is merely a definition, a blueprint of operations.
        // No filtering or calculation happens until the result is enumerated.
        // This is the core of the "Functional Pipeline".
        var preprocessingPipeline = rawData
            .Where(d => d.IsValid()) // Step A: Cleanse
            .Select(d => d.Normalize()) // Step B: Normalize
            .Select(d => d.NoiseReduction()); // Step C: Filter Noise

        // 3. TRIGGERING EXECUTION:
        // We iterate over the pipeline. Only NOW do the lambda expressions fire.
        // The pipeline executes in a single pass (streaming), optimizing memory usage.
        Console.WriteLine("\n--- Processed Data (Functional Pipeline) ---");
        foreach (var processedItem in preprocessingPipeline)
        {
            Console.WriteLine(processedItem);
        }
    }

    // Simulating a raw data source (e.g., CSV, API)
    static IEnumerable<UserData> GenerateRawData()
    {
        yield return new UserData { Id = 1, Value = 10.5f, IsValidFlag = true };
        yield return new UserData { Id = 2, Value = -5.2f, IsValidFlag = false }; // Invalid
        yield return new UserData { Id = 3, Value = 0.0f, IsValidFlag = true };   // Noise
        yield return new UserData { Id = 4, Value = 25.0f, IsValidFlag = true };
    }
}

// Simple POCO (Plain Old CLR Object) to represent a data point
public record UserData
{
    public int Id { get; set; }
    public float Value { get; set; }
    public bool IsValidFlag { get; set; }

    // Pure function: No side effects
    public bool IsValid() => IsValidFlag;

    // Pure function: Normalizes the value (e.g., Min-Max scaling logic)
    public UserData Normalize() => this with { Value = Value / 100.0f };

    // Pure function: Removes low-value noise
    public UserData NoiseReduction() => this with { Value = Math.Abs(Value) > 0.01f ? Value : 0.0f };

    public override string ToString() => $"[ID: {Id}, Val: {Value:F4}, Valid: {IsValidFlag}]";
}
