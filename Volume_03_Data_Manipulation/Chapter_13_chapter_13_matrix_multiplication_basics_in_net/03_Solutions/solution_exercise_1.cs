
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public class SensorReading
{
    public int Id { get; set; }
    public double? Value { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DataCleaningExercise
{
    public static void Run()
    {
        // 1. Create Data Source
        // Using a List to allow modification (adding items) after query definition.
        var source = new List<SensorReading>();
        var rand = new Random();
        
        for (int i = 0; i < 10; i++)
        {
            // Simulate dirty data: values between -50 and 150, with 20% chance of null
            double? val = rand.NextDouble() * 200 - 50; 
            if (rand.NextDouble() < 0.2) val = null; 
            
            source.Add(new SensorReading { Id = i, Value = val, Timestamp = DateTime.Now });
        }

        // 2. Build the LINQ Pipeline (Deferred Execution)
        // The query is defined here but not executed.
        var cleanQuery = source
            .Where(s => s.Value.HasValue) // Filter nulls
            .Where(s => s.Value >= -100 && s.Value <= 100) // Filter outliers
            .Select(s => new { s.Id, NormalizedValue = s.Value.Value / 100.0 }); // Project

        // 3. Analyze Execution
        // Calling Count() triggers execution over the current 'source'.
        Console.WriteLine($"Initial Count (Query Definition): {cleanQuery.Count()}");
        
        // Modify source AFTER the query is defined.
        // This record has a valid value (50.0) and should pass the filters.
        source.Add(new SensorReading { Id = 999, Value = 50.0, Timestamp = DateTime.Now });

        // Calling Count() again re-evaluates the query over the MODIFIED source.
        Console.WriteLine($"Count after adding record (Deferred): {cleanQuery.Count()}");

        // 4. Materialize
        // ToList() forces immediate execution and captures the result in memory.
        var materializedList = cleanQuery.ToList();
        Console.WriteLine($"Materialized List Count: {materializedList.Count}");
        
        // Further modifications to 'source' will NOT affect 'materializedList'
        // because it is a separate collection in memory.
        source.Add(new SensorReading { Id = 1000, Value = 20.0, Timestamp = DateTime.Now });
        Console.WriteLine($"Count after adding record (Materialized): {materializedList.Count}");
    }
}
