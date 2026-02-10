
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

public class ProductInteraction
{
    public double Price { get; set; }
    public double TimeSpent { get; set; } // in seconds
    public string UserId { get; set; }
}

public class NormalizationExample
{
    public static void Run()
    {
        // 1. Raw Data Source (Simulating a database query or CSV load)
        var rawData = new List<ProductInteraction>
        {
            new ProductInteraction { UserId = "User1", Price = 10.0, TimeSpent = 5.0 },
            new ProductInteraction { UserId = "User2", Price = 500.0, TimeSpent = 120.0 },
            new ProductInteraction { UserId = "User3", Price = 25.0, TimeSpent = 15.0 },
            new ProductInteraction { UserId = "User4", Price = 1000.0, TimeSpent = 300.0 }
        };

        // 2. Analyze Data Characteristics (Immediate Execution)
        // We must calculate Min/Max values BEFORE transforming the data.
        // .Max() and .Min() trigger immediate execution on the source.
        double minPrice = rawData.Min(p => p.Price);
        double maxPrice = rawData.Max(p => p.Price);
        double minTime = rawData.Min(p => p.TimeSpent);
        double maxTime = rawData.Max(p => p.TimeSpent);

        Console.WriteLine($"Price Range: [{minPrice}, {maxPrice}]");
        Console.WriteLine($"Time Range: [{minTime}, {maxTime}]");

        // 3. Define Normalization Logic (Pure Functions)
        // Helper function to scale a value to [0, 1] range.
        // Formula: (x - min) / (max - min)
        Func<double, double, double, double> minMaxScale = (val, min, max) => 
            (val - min) / (max - min);

        // 4. Construct the LINQ Transformation Pipeline (Deferred Execution)
        // Note: This query is not executed yet. It simply defines the steps.
        var normalizedQuery = rawData
            .Select(p => new 
            {
                // Preserve original ID for reference
                UserId = p.UserId,
                // Apply scaling using the captured closure variables
                ScaledPrice = minMaxScale(p.Price, minPrice, maxPrice),
                ScaledTime = minMaxScale(p.TimeSpent, minTime, maxTime)
            });

        // 5. Materialize the Results (Immediate Execution)
        // .ToList() forces the query to execute and store results in memory.
        var normalizedData = normalizedQuery.ToList();

        // 6. Output Results
        Console.WriteLine("\n--- Normalized Data ---");
        foreach (var item in normalizedData)
        {
            Console.WriteLine($"User: {item.UserId} | Price: {item.ScaledPrice:F4} | Time: {item.ScaledTime:F4}");
        }
    }
}
