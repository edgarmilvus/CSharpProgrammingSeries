
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

// Define a simple immutable record for sensor data.
// Records are ideal for functional programming as they are immutable by default.
public record SensorData(
    string SensorId, 
    double Vibration, 
    double Temperature, 
    double Voltage, 
    bool IsAnomaly
);

public class DataPreprocessingPipeline
{
    public static void Main()
    {
        Console.WriteLine("--- Starting Functional Data Pipeline ---\n");

        // 1. SIMULATION: Generate raw, noisy data.
        // We use a helper method to create a list of records.
        // In a real scenario, this would come from a database or stream.
        List<SensorData> rawDataset = GenerateRawSensorData(100);

        Console.WriteLine($"[Initial State] Raw dataset count: {rawDataset.Count}");

        // 2. EXECUTION: Define the pipeline.
        // CRITICAL CONCEPT: Deferred Execution.
        // At this moment, NO processing happens. We are merely building a blueprint (IEnumerable).
        // The query is not executed until we iterate over it (e.g., .ToList()).
        var preprocessingPipeline = rawDataset
            .Where(data => 
                // FILTERING: Remove nulls or invalid ranges (simulated by checking for extreme values)
                data.Vibration >= 0 && 
                data.Temperature > -273.15 && // Absolute zero check
                !data.IsAnomaly // Exclude known anomalies for this specific training batch
            )
            .Select(data => 
                // NORMALIZATION: Transform raw values to 0-1 range.
                // We calculate min/max based on domain knowledge (or pre-calculated stats).
                // Vibration: 0-100 range -> 0-1
                // Temperature: 0-100 range -> 0-1
                // Voltage: 0-24 range -> 0-1
                new SensorData(
                    SensorId: data.SensorId,
                    Vibration: data.Vibration / 100.0,
                    Temperature: (data.Temperature + 10) / 110.0, // Shifted for negative temps
                    Voltage: data.Voltage / 24.0,
                    IsAnomaly: data.IsAnomaly
                )
            )
            // 3. SHUFFLING: Randomize the order.
            // Since LINQ is declarative, we cannot easily shuffle without materializing the sequence
            // or using a random sort key. We use OrderBy with a random key.
            // Note: In a pure functional language, we would pass a random generator seed.
            .OrderBy(_ => Guid.NewGuid()); // Guid.NewGuid() provides a pseudo-random key

        Console.WriteLine("[Pipeline Defined] Deferred execution state active. No data processed yet.\n");

        // 4. IMMEDIATE EXECUTION: Materialize the results.
        // The pipeline executes here. The data is cleaned, normalized, and shuffled in one pass.
        // We use .ToList() to trigger execution.
        List<SensorData> processedDataset = preprocessingPipeline.ToList();

        // 5. VERIFICATION: Output results.
        Console.WriteLine($"[Final State] Processed dataset count: {processedDataset.Count}");
        Console.WriteLine("\n--- Sample of Processed Data (First 5 Records) ---");
        Console.WriteLine("ID\t\tVibration\tTemperature\tVoltage");
        
        // We use Take(5) to limit output. This is a safe operation on the materialized list.
        foreach (var record in processedDataset.Take(5))
        {
            // Formatted string output
            Console.WriteLine($"{record.SensorId}\t{record.Vibration:F4}\t\t{record.Temperature:F4}\t\t{record.Voltage:F4}");
        }

        Console.WriteLine("\n--- Pipeline Complete ---");
    }

    // Helper method to simulate raw data generation
    static List<SensorData> GenerateRawSensorData(int count)
    {
        var random = new Random();
        var data = new List<SensorData>();

        for (int i = 0; i < count; i++)
        {
            // Simulate noise: occasionally inject invalid data or anomalies
            bool isAnomaly = random.NextDouble() > 0.9; // 10% anomaly rate
            double vibration = isAnomaly ? 150.0 : random.NextDouble() * 90.0; // Sometimes exceeds 100
            double temp = isAnomaly ? -300.0 : random.NextDouble() * 80.0 - 5.0; // Sometimes below absolute zero
            double voltage = random.NextDouble() * 24.0;

            data.Add(new SensorData(
                SensorId: $"S-{i:D4}",
                Vibration: vibration,
                Temperature: temp,
                Voltage: voltage,
                IsAnomaly: isAnomaly
            ));
        }
        return data;
    }
}
