
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

public class BasicLinqExample
{
    public static void Main()
    {
        // 1. Source Data: A collection of raw sensor readings.
        // In a real scenario, this might come from a database or CSV file.
        var rawReadings = new List<SensorReading>
        {
            new SensorReading { Id = 1, Value = 10.5, IsValid = true },
            new SensorReading { Id = 2, Value = 25.0, IsValid = false }, // Invalid data
            new SensorReading { Id = 3, Value = 15.2, IsValid = true },
            new SensorReading { Id = 4, Value = 5.0,  IsValid = true },
            new SensorReading { Id = 5, Value = 30.0, IsValid = false }  // Invalid data
        };

        // 2. The Query: Define the pipeline (Filtering + Projection).
        // CRITICAL: This query is not executed yet. It is a definition of work (Deferred Execution).
        // We filter for valid readings and project (Select) only the normalized values.
        var processedPipeline = rawReadings
            .Where(reading => reading.IsValid)                  // Filter: Predicate logic
            .Select(reading => reading.Value * 1.1);            // Projection: Mapping to new shape/type

        // 3. Execution: Triggering the pipeline.
        // We materialize the results into a concrete list. 
        // This is where the logic actually runs over the data.
        List<double> results = processedPipeline.ToList();

        // 4. Output
        Console.WriteLine("Processed Sensor Values (Normalized):");
        foreach (var val in results)
        {
            Console.WriteLine(val);
        }
    }
}

// Simple DTO (Data Transfer Object) to represent our data structure
public class SensorReading
{
    public int Id { get; set; }
    public double Value { get; set; }
    public bool IsValid { get; set; }
}
