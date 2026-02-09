
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

namespace DataPreprocessingPipeline
{
    // Represents a raw data point from a fitness tracking application.
    // Features: [HeartRate, CaloriesBurned, Steps]
    public class RawFitnessRecord
    {
        public int HeartRate { get; set; }
        public double CaloriesBurned { get; set; }
        public int Steps { get; set; }
        public string Label { get; set; } // e.g., "Walking", "Running", "Resting"
    }

    // Represents a processed vector ready for embedding or model training.
    public class ProcessedVector
    {
        public double[] Features { get; set; }
        public string Label { get; set; }
    }

    public class PreprocessingEngine
    {
        public static void Main()
        {
            // 1. DATA GENERATION
            // Simulating a dataset with varying magnitudes and some missing/invalid data.
            // In a real scenario, this would be loaded from a CSV or database.
            var rawData = new List<RawFitnessRecord>
            {
                new RawFitnessRecord { HeartRate = 72, CaloriesBurned = 200.5, Steps = 1000, Label = "Walking" },
                new RawFitnessRecord { HeartRate = 85, CaloriesBurned = 450.2, Steps = 3500, Label = "Running" },
                new RawFitnessRecord { HeartRate = 68, CaloriesBurned = 150.0, Steps = 800, Label = "Walking" },
                new RawFitnessRecord { HeartRate = 60, CaloriesBurned = 0.0, Steps = 0, Label = "Resting" },
                new RawFitnessRecord { HeartRate = 150, CaloriesBurned = 800.0, Steps = 7000, Label = "Running" },
                new RawFitnessRecord { HeartRate = 90, CaloriesBurned = 600.0, Steps = 5000, Label = "Running" },
                new RawFitnessRecord { HeartRate = 120, CaloriesBurned = 0, Steps = 0, Label = "Error" }, // Invalid data
                new RawFitnessRecord { HeartRate = 75, CaloriesBurned = 220.0, Steps = 1200, Label = "Walking" }
            };

            Console.WriteLine("--- Step 1: Raw Data Loaded ---");
            rawData.ForEach(d => Console.WriteLine($"HR: {d.HeartRate}, Cal: {d.CaloriesBurned}, Steps: {d.Steps}, Label: {d.Label}"));
            Console.WriteLine();

            // 2. FUNCTIONAL DATA CLEANING PIPELINE
            // We use LINQ to filter out invalid records.
            // Constraint Check: No side effects. We are projecting a new filtered sequence.
            // Deferred Execution: This query is not executed until iterated over.
            var validDataQuery = rawData
                .Where(record => record.HeartRate > 0 && record.CaloriesBurned >= 0 && record.Steps >= 0)
                .Where(record => record.Label != "Error");

            // Immediate Execution: .ToList() forces evaluation of the query.
            var validData = validDataQuery.ToList();

            Console.WriteLine("--- Step 2: Cleaned Data (Immediate Execution) ---");
            Console.WriteLine($"Records before cleaning: {rawData.Count}");
            Console.WriteLine($"Records after cleaning: {validData.Count}");
            Console.WriteLine();

            // 3. STATISTICAL ANALYSIS (Z-Score Normalization Prep)
            // We need Mean and Standard Deviation for each feature.
            // We calculate these using LINQ Aggregate or Average/StdDev formulas.
            
            // Helper to calculate Standard Deviation using a pure functional approach
            Func<IEnumerable<int>, double> calculateStdDevInt = (values) =>
            {
                var mean = values.Average();
                var sumSquares = values.Sum(v => Math.Pow(v - mean, 2));
                return Math.Sqrt(sumSquares / values.Count());
            };

            Func<IEnumerable<double>, double> calculateStdDevDouble = (values) =>
            {
                var mean = values.Average();
                var sumSquares = values.Sum(v => Math.Pow(v - mean, 2));
                return Math.Sqrt(sumSquares / values.Count());
            };

            // Extract columns using Select
            var heartRates = validData.Select(d => d.HeartRate).ToList();
            var calories = validData.Select(d => d.CaloriesBurned).ToList();
            var steps = validData.Select(d => d.Steps).ToList();

            // Calculate Stats
            var hrMean = heartRates.Average();
            var hrStd = calculateStdDevInt(heartRates);

            var calMean = calories.Average();
            var calStd = calculateStdDevDouble(calories);

            var stepMean = steps.Average();
            var stepStd = calculateStdDevInt(steps);

            Console.WriteLine("--- Step 3: Statistics Calculated ---");
            Console.WriteLine($"HR - Mean: {hrMean:F2}, StdDev: {hrStd:F2}");
            Console.WriteLine($"Cal - Mean: {calMean:F2}, StdDev: {calStd:F2}");
            Console.WriteLine($"Step - Mean: {stepMean:F2}, StdDev: {stepStd:F2}");
            Console.WriteLine();

            // 4. NORMALIZATION PIPELINE (Z-Score)
            // Transforming data to have Mean=0 and StdDev=1.
            // Formula: z = (x - mean) / stdDev
            // We use an anonymous type to hold intermediate values before final vector creation.
            var normalizedDataQuery = validData
                .Select(d => new
                {
                    // Calculate Z-scores immediately within the projection
                    Z_Hr = (d.HeartRate - hrMean) / hrStd,
                    Z_Cal = (d.CaloriesBurned - calMean) / calStd,
                    Z_Step = (d.Steps - stepMean) / stepStd,
                    Label = d.Label
                });

            // 5. VECTOR CONVERSION & MIN-MAX SCALING
            // Sometimes we want vectors in [0, 1] range instead of Z-scores.
            // We will demonstrate a hybrid pipeline: Z-score for one branch, MinMax for another.
            
            // First, calculate Min/Max for Min-Max scaling
            var minHr = heartRates.Min();
            var maxHr = heartRates.Max();
            var minCal = calories.Min();
            var maxCal = calories.Max();
            var minStep = steps.Min();
            var maxStep = steps.Max();

            // Define a transformation function (Pure Functional)
            // Input: Raw Record -> Output: ProcessedVector (MinMax Scaled)
            Func<RawFitnessRecord, ProcessedVector> minMaxScaler = (record) =>
            {
                // Avoid division by zero if range is 0
                double Scale(double val, double min, double max) => (max == min) ? 0 : (val - min) / (max - min);

                return new ProcessedVector
                {
                    Features = new double[]
                    {
                        Scale(record.HeartRate, minHr, maxHr),
                        Scale(record.CaloriesBurned, minCal, maxCal),
                        Scale(record.Steps, minStep, maxStep)
                    },
                    Label = record.Label
                };
            };

            // Apply the MinMax scaler to the valid data
            var minMaxVectors = validData.Select(minMaxScaler).ToList();

            Console.WriteLine("--- Step 4: Min-Max Scaled Vectors (Range 0-1) ---");
            minMaxVectors.ForEach(v =>
            {
                Console.WriteLine($"Label: {v.Label} | Vector: [{v.Features[0]:F2}, {v.Features[1]:F2}, {v.Features[2]:F2}]");
            });
            Console.WriteLine();

            // 6. ADVANCED: SHUFFLING AND BATCHING (PLINQ)
            // Machine learning models often train on shuffled batches.
            // We use PLINQ (AsParallel) to simulate concurrent processing of data shuffling.
            // Note: Parallelism here is for demonstration; shuffling is usually sequential or seeded.
            
            // To shuffle purely functionally without modifying the list in place:
            // 1. Project with a random key.
            // 2. Order by that key.
            // 3. Materialize.
            var random = new Random();
            
            // Deferred execution warning: If we don't ToList(), the Random generator might be called 
            // lazily in a way that produces non-unique keys or unexpected behavior in parallel queries.
            // We materialize the shuffled list immediately.
            var shuffledVectors = minMaxVectors
                .AsParallel() // Enable PLINQ
                .Select(v => new { RandomKey = random.NextDouble(), Vector = v })
                .OrderBy(x => x.RandomKey)
                .Select(x => x.Vector)
                .ToList(); // Immediate Execution

            // 7. BATCHING (GROUPING)
            // Create batches of size 3 for mini-batch gradient descent simulation.
            int batchSize = 3;
            var batchedVectors = shuffledVectors
                .Select((Vector, Index) => new { Vector, Index })
                .GroupBy(x => x.Index / batchSize)
                .Select(g => g.Select(item => item.Vector).ToList())
                .ToList();

            Console.WriteLine("--- Step 5: Shuffled & Batched Data (PLINQ) ---");
            int batchNum = 1;
            foreach (var batch in batchedVectors)
            {
                Console.WriteLine($"Batch {batchNum}:");
                foreach (var vec in batch)
                {
                    Console.WriteLine($"  [{vec.Features[0]:F2}, {vec.Features[1]:F2}, {vec.Features[2]:F2}] - {vec.Label}");
                }
                batchNum++;
            }

            // 8. DEFERRED EXECUTION DEMONSTRATION
            // Let's prove that LINQ queries are lazy until materialized.
            Console.WriteLine("\n--- Step 6: Deferred Execution Demo ---");
            
            // Define a query with logging inside Select (Side effect for demo purposes only - usually forbidden)
            // We use .Select with Console.WriteLine to prove execution timing.
            var deferredQuery = validData.Select(d => 
            {
                Console.WriteLine($"Processing {d.Label}..."); 
                return d; 
            });

            Console.WriteLine("Query defined, but not executed yet.");
            
            // Trigger execution
            Console.WriteLine("Triggering execution by iterating...");
            foreach (var item in deferredQuery.Take(2)) // Take(2) stops after 2 items
            {
                // Iteration happens here
            }
            
            Console.WriteLine("Iteration complete.");
        }
    }
}
