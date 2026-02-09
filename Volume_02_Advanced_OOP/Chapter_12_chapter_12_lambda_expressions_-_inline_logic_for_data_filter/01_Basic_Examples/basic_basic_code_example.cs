
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

namespace Book2_AdvancedOOP_AI_DataStructures.Chapter12
{
    /// <summary>
    /// Demonstrates basic lambda expression usage for filtering sensor data in an AI monitoring system.
    /// </summary>
    public class BasicLambdaExample
    {
        // Simulating a real-world IoT sensor reading structure
        public struct SensorReading
        {
            public string SensorId { get; set; }
            public double Temperature { get; set; }
            public double Humidity { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public static void RunExample()
        {
            // 1. DATA COLLECTION: Simulating high-frequency sensor data ingestion
            // In a real AI system, this might come from distributed edge devices
            var sensorData = new List<SensorReading>
            {
                new SensorReading { SensorId = "SENSOR_001", Temperature = 22.5, Humidity = 45.0, Timestamp = DateTime.Now.AddHours(-2) },
                new SensorReading { SensorId = "SENSOR_002", Temperature = 85.2, Humidity = 12.0, Timestamp = DateTime.Now.AddHours(-1) }, // Anomaly: High temp
                new SensorReading { SensorId = "SENSOR_003", Temperature = 21.0, Humidity = 48.0, Timestamp = DateTime.Now.AddMinutes(-30) },
                new SensorReading { SensorId = "SENSOR_004", Temperature = 90.1, Humidity = 10.0, Timestamp = DateTime.Now.AddMinutes(-15) }, // Anomaly: High temp
                new SensorReading { SensorId = "SENSOR_005", Temperature = 23.1, Humidity = 42.0, Timestamp = DateTime.Now }
            };

            Console.WriteLine("=== Raw Sensor Data ===");
            foreach (var reading in sensorData)
            {
                Console.WriteLine($"ID: {reading.SensorId} | Temp: {reading.Temperature}°C | Humidity: {reading.Humidity}%");
            }

            // 2. LAMBDA EXPRESSION: Inline logic for filtering critical temperature anomalies
            // The lambda (reading => reading.Temperature > 80.0) defines the filtering criteria
            // This replaces the need for a separate method or class definition
            var criticalReadings = sensorData
                .Where(reading => reading.Temperature > 80.0) // Lambda: Input parameter 'reading', output boolean condition
                .ToList();

            Console.WriteLine("\n=== Filtered Critical Readings (Temp > 80°C) ===");
            foreach (var reading in criticalReadings)
            {
                Console.WriteLine($"ALERT: {reading.SensorId} at {reading.Temperature}°C");
            }

            // 3. CHAINED LAMBDA: Filtering and projecting data simultaneously
            // We filter for high humidity AND extract only the SensorId
            var highHumiditySensors = sensorData
                .Where(r => r.Humidity > 40.0)          // First lambda: Filter
                .Select(r => r.SensorId)                // Second lambda: Transform (projection)
                .ToList();

            Console.WriteLine("\n=== High Humidity Sensors (IDs Only) ===");
            highHumiditySensors.ForEach(id => Console.WriteLine($"Sensor: {id}"));

            // 4. MULTI-PARAMETER LAMBDA: Using aggregation logic
            // Calculate average temperature of critical readings using Reduce pattern
            if (criticalReadings.Count > 0)
            {
                // Aggregate uses a lambda to combine elements: (accumulator, current) => new accumulator
                double avgTemp = criticalReadings.Aggregate(
                    0.0,                                // Seed value
                    (acc, reading) => acc + reading.Temperature // Lambda: Accumulator logic
                ) / criticalReadings.Count;

                Console.WriteLine($"\nAverage Critical Temperature: {avgTemp:F2}°C");
            }
        }
    }
}
