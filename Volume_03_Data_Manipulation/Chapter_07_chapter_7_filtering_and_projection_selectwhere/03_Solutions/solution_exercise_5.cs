
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class SensorData { public string Type { get; set; } public double Value { get; set; } }

public class Exercise5
{
    public static void Run()
    {
        var sensors = new List<SensorData>
        {
            new SensorData { Type = "Temp", Value = 25.5 },
            new SensorData { Type = "Pressure", Value = 1013.25 },
            new SensorData { Type = "Temp", Value = 26.0 },
            new SensorData { Type = "Temp", Value = -10.0 },      // Invalid: Negative
            new SensorData { Type = "Pressure", Value = double.NaN }, // Invalid: NaN
            new SensorData { Type = "Humidity", Value = 45.0 }
        };

        // Modified Pipeline with PLINQ
        var stats = sensors
            .AsParallel() // 1. Parallelism: Distribute work across threads
            .Where(s => s.Value >= 0 && !double.IsNaN(s.Value)) // 2. Filter Invalid Data
            .GroupBy(s => s.Type) // 3. Group by Sensor Type
            .Select(g => new 
            { 
                SensorType = g.Key, 
                AverageValue = g.Average(s => s.Value) // 4. Aggregate
            })
            .OrderBy(r => r.SensorType) // 5. Deterministic Ordering (required for parallel results)
            .ToList(); // 6. Materialize

        Console.WriteLine("Sensor Statistics (Parallel Processing):");
        foreach (var stat in stats)
        {
            Console.WriteLine($"Type: {stat.SensorType}, Avg: {stat.AverageValue:F2}");
        }
    }
}
