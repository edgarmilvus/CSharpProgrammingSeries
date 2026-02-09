
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
using System.Threading;
using System.Threading.Tasks;

public class SensorDataProcessor
{
    // Define a simple immutable record for our sensor data.
    // Immutability prevents side effects and ensures thread safety.
    public record SensorReading(
        int SensorId,
        double RawValue,
        DateTime Timestamp,
        bool IsValid
    );

    // Define a record for the processed output ready for embedding.
    public record ProcessedVector(
        int SensorId,
        double NormalizedValue,
        double[] Features
    );

    public static void Main()
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Starting Data Processing Pipeline.");

        // 1. DATA GENERATION (Simulating a large dataset)
        // We generate 10,000 sensor readings to simulate a "Big Data" batch.
        // In a real scenario, this might come from a network stream or database.
        var rawSensorData = GenerateMockData(count: 10000);

        // 2. DEFERRED EXECUTION EXPLANATION
        // The following query is NOT executed yet. It is a "blueprint" of operations.
        // We define the logic, but no processing happens until we call .ToList() or iterate.
        // This allows us to compose complex queries efficiently.
        var processingPipeline = rawSensorData
            .AsParallel() // <--- PLINQ: Enables multi-core processing.
                         // Use with caution: small datasets might run slower due to overhead.
            .WithDegreeOfParallelism(Environment.ProcessorCount) // Explicitly setting thread usage.
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism) // Ensures PLINQ is used even for small chunks.

            // STAGE 1: CLEANING (Filtering)
            // We filter out invalid data. In PLINQ, this partitioning happens automatically.
            .Where(reading => reading.IsValid && reading.RawValue >= 0)

            // STAGE 2: NORMALIZATION (Transformation)
            // Pure functional mapping: Input -> Output. No side effects.
            // We calculate normalized values based on domain knowledge (0-100 scale).
            .Select(reading =>
            {
                // Simulating a heavy calculation or external lookup
                double minThreshold = 0.0;
                double maxThreshold = 100.0;
                
                // Clamp value to range
                double clamped = Math.Max(minThreshold, Math.Min(maxThreshold, reading.RawValue));
                
                // Normalize
                double normalized = (clamped - minThreshold) / (maxThreshold - minThreshold);

                // Create a feature vector for embedding (simulated)
                // In AI context, this converts a scalar to a vector representation.
                double[] features = new double[] { normalized, reading.SensorId / 1000.0 };

                return new ProcessedVector(
                    reading.SensorId,
                    normalized,
                    features
                );
            })

            // STAGE 3: SHUFFLING (Ordering)
            // Parallel processing often loses the original order. 
            // For ML training, we usually want random order to avoid bias.
            // Note: OrderBy with a random key is a heavy operation in PLINQ.
            .OrderBy(_ => Guid.NewGuid()) 

            // CRITICAL: IMMEDIATE EXECUTION
            // Up to this point, nothing has happened. 
            // .ToList() triggers the pipeline. The CPU spins up threads, 
            // partitions the data, and executes the steps.
            .ToList(); 

        // 3. CONSUMPTION
        // The result is now a concrete List<T> in memory.
        // We process the top 5 results to verify the pipeline worked.
        Console.WriteLine($"\nPipeline executed. Processed {processingPipeline.Count} vectors.");
        Console.WriteLine("Sample Output (First 5 Shuffled Vectors):");
        
        // We use standard LINQ here because the data is now small and local.
        processingPipeline.Take(5).ToList().ForEach(v =>
        {
            Console.WriteLine($"  Sensor: {v.SensorId}, Norm: {v.NormalizedValue:F4}, Vector: [{string.Join(", ", v.Features.Select(f => f.ToString("F2")))}]");
        });
    }

    // Helper method to generate mock data
    static List<SensorReading> GenerateMockData(int count)
    {
        var random = new Random();
        var data = new List<SensorReading>();
        
        for (int i = 0; i < count; i++)
        {
            // Simulate some corrupted data (IsValid = false) randomly
            bool isValid = random.NextDouble() > 0.05; // 5% chance of invalid
            double value = random.NextDouble() * 120; // Range 0-120 (some outliers)
            
            data.Add(new SensorReading(
                SensorId: i,
                RawValue: value,
                Timestamp: DateTime.Now.AddSeconds(-i),
                IsValid: isValid
            ));
        }
        return data;
    }
}
