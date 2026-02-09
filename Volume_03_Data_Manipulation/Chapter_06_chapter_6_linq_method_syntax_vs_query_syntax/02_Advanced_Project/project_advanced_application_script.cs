
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
using System.Threading.Tasks;

namespace DataPreprocessingPipeline
{
    // 1. Domain Model: Represents a raw sensor reading.
    // We avoid arrays to demonstrate working with IEnumerable<T>.
    public class SensorReading
    {
        public int SensorId { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsValid { get; set; }
    }

    // 2. Configuration: Holds parameters for normalization.
    public class ProcessingConfig
    {
        public double MinThreshold { get; set; }
        public double MaxThreshold { get; set; }
        public int BatchSize { get; set; }
    }

    public class DataPipeline
    {
        public static void Run()
        {
            Console.WriteLine("--- Starting Functional Data Pipeline ---\n");

            // 3. Input Simulation: Generating raw, unclean data.
            // Note: We are not using arrays or loops; we rely on LINQ generation.
            var rawDataSource = GenerateMockData();

            // 4. Configuration Setup
            var config = new ProcessingConfig { MinThreshold = 10.0, MaxThreshold = 90.0, BatchSize = 5 };

            // ---------------------------------------------------------
            // PHASE 1: CLEANING (Method Syntax)
            // ---------------------------------------------------------
            // Concept: Deferred Execution.
            // 'cleanedData' is not executed yet. It is a definition of a query.
            // No side effects: The lambda only reads data; it does not modify external state.
            var cleanedData = rawDataSource.Where(reading => 
                reading.IsValid && 
                reading.Value >= config.MinThreshold && 
                reading.Value <= config.MaxThreshold
            );

            // ---------------------------------------------------------
            // PHASE 2: NORMALIZATION (Method Syntax)
            // ---------------------------------------------------------
            // We need global statistics (Min/Max) to normalize individual points.
            // To get these, we must trigger Immediate Execution on the cleaned set.
            // If we didn't do this, we would have to traverse the data multiple times.
            
            // Immediate Execution: .ToList() forces the query to run and store results.
            var materializedCleanData = cleanedData.ToList(); 
            
            // Calculate bounds immediately
            double minVal = materializedCleanData.Min(r => r.Value);
            double maxVal = materializedCleanData.Max(r => r.Value);
            double range = maxVal - minVal;

            Console.WriteLine($"[Stats] Cleaned Count: {materializedCleanData.Count}");
            Console.WriteLine($"[Stats] Normalization Range: {minVal:F2} - {maxVal:F2}\n");

            // Now, create the normalized projection. 
            // This is deferred again until we iterate or materialize.
            var normalizedData = materializedCleanData.Select(r => new 
            {
                // Anonymous type for the transformed data
                SensorId = r.SensorId,
                // Z-Score Normalization simulation
                NormalizedValue = (r.Value - minVal) / range, 
                Timestamp = r.Timestamp
            });

            // ---------------------------------------------------------
            // PHASE 3: SHUFFLING & BATCHING (Hybrid Syntax)
            // ---------------------------------------------------------
            // Pure functional shuffling is non-trivial without side effects or external libraries.
            // We simulate a deterministic shuffle using a hash of the timestamp to randomize order.
            // We use Query Syntax here to demonstrate the 'let' keyword for intermediate calculations.
            
            var shuffledQuery = 
                from n in normalizedData
                // 'let' creates a calculated variable within the query scope
                let shuffleKey = n.Timestamp.Ticks ^ n.SensorId 
                orderby shuffleKey // Sort by the pseudo-random key
                select n;

            // Execute the shuffle and batch immediately.
            // We convert to a list to allow grouping, which is often easier in Query Syntax.
            var shuffledList = shuffledQuery.ToList();

            // ---------------------------------------------------------
            // PHASE 4: PARALLEL PROCESSING (PLINQ)
            // ---------------------------------------------------------
            // Now that data is clean and shuffled, we process batches in parallel.
            // We use .AsParallel() to enable PLINQ.
            // We use .GroupBy to create batches of size 'BatchSize'.
            
            var processingBatches = shuffledList
                .Select((item, index) => new { item, index }) // Pair item with index
                .GroupBy(x => x.index / config.BatchSize)     // Group by batch index
                .Select(g => g.Select(x => x.item).ToList())  // Convert groups to lists
                .AsParallel()                                 // Enable parallel execution
                .WithDegreeOfParallelism(4);                  // Simulate 4 cores

            // ---------------------------------------------------------
            // PHASE 5: OUTPUT (Consumption)
            // ---------------------------------------------------------
            // We iterate over the parallel query. 
            // Note: Order is not guaranteed here due to parallelism.
            Console.WriteLine("--- Processing Batches in Parallel ---");

            processingBatches.ForAll(batch =>
            {
                // Calculate batch statistics (Average) purely functionally
                double avg = batch.Average(b => b.NormalizedValue);
                
                // We use a lock here only for console output to prevent garbled text.
                // This is a side effect of UI, not of the data logic.
                lock (typeof(Console))
                {
                    Console.WriteLine($"[Batch] Processed {batch.Count} items. Avg Norm Value: {avg:F4}");
                }
            });
        }

        // Helper to generate data without loops (using LINQ Repeat and Select)
        private static IEnumerable<SensorReading> GenerateMockData()
        {
            var random = new Random();
            
            // Generate 20 readings
            return Enumerable.Range(1, 20)
                .Select(i => new SensorReading
                {
                    SensorId = (i % 3) + 1, // 3 Sensors
                    // Simulate noise: occasionally generate invalid data
                    Value = (i % 5 == 0) ? 150.0 : random.NextDouble() * 100, 
                    Timestamp = DateTime.Now.AddSeconds(i),
                    IsValid = (i % 7 != 0) // Every 7th reading is invalid
                });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DataPipeline.Run();
        }
    }
}
