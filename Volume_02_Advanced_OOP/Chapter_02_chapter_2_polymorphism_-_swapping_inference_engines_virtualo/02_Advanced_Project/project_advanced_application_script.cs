
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

namespace SmartFactorySystem
{
    // ---------------------------------------------------------
    // 1. DATA ABSTRACTION
    // We define an abstract base class for all sensor data.
    // This allows us to handle heterogeneous data (floats, arrays)
    // through a common interface.
    // ---------------------------------------------------------
    public abstract class SensorData
    {
        public DateTime Timestamp { get; set; }
        
        // Virtual method to display data. Derived classes will override this.
        public virtual string GetFormattedData()
        {
            return $"[{Timestamp}] Base Data";
        }
    }

    // ---------------------------------------------------------
    // 2. CONCRETE DATA IMPLEMENTATIONS
    // These represent specific sensor readings.
    // ---------------------------------------------------------

    // A simple sensor reading (e.g., Temperature).
    public class FloatSensorData : SensorData
    {
        public float Value { get; set; }

        // OVERRIDE: Providing specific implementation for float data.
        public override string GetFormattedData()
        {
            return $"[{Timestamp}] Float Reading: {Value:F2} units";
        }
    }

    // A complex sensor reading (e.g., Vibration spectrum represented as a tensor/array).
    // Since we cannot use arrays or generics, we simulate a tensor with fixed fields.
    public class TensorSensorData : SensorData
    {
        public float X_Axis { get; set; }
        public float Y_Axis { get; set; }
        public float Z_Axis { get; set; }

        // OVERRIDE: Providing specific implementation for tensor data.
        public override string GetFormattedData()
        {
            return $"[{Timestamp}] Tensor Reading: [{X_Axis:F2}, {Y_Axis:F2}, {Z_Axis:F2}]";
        }
    }

    // ---------------------------------------------------------
    // 3. INFERENCE ENGINE ABSTRACTION
    // This is the core interface for our polymorphic behavior.
    // It defines a contract for any analysis engine we might want to use.
    // ---------------------------------------------------------
    public interface IInferenceEngine
    {
        // Abstract method signature. Every engine must implement this.
        // It accepts generic SensorData and returns a boolean (Anomaly detected?).
        bool Analyze(SensorData data);
        
        string GetEngineName();
    }

    // ---------------------------------------------------------
    // 4. CONCRETE INFERENCE ENGINES
    // These are the interchangeable algorithms.
    // ---------------------------------------------------------

    // Engine 1: Simple Threshold Checker
    public class ThresholdEngine : IInferenceEngine
    {
        private float _limit;

        public ThresholdEngine(float limit)
        {
            _limit = limit;
        }

        // IMPLEMENTATION: Checks if the float value exceeds the limit.
        public bool Analyze(SensorData data)
        {
            // We must check the type to access specific properties.
            // This is a common pattern in polymorphic systems when downcasting.
            if (data is FloatSensorData floatData)
            {
                Console.WriteLine($"  [Threshold Engine] Checking limit {_limit}...");
                return floatData.Value > _limit;
            }
            
            // If the data type doesn't match, we treat it as invalid for this engine.
            Console.WriteLine("  [Threshold Engine] Cannot analyze non-float data.");
            return false;
        }

        public string GetEngineName() => "Threshold Limit Checker";
    }

    // Engine 2: Statistical Anomaly Detector (Simulated)
    public class StatisticalEngine : IInferenceEngine
    {
        // IMPLEMENTATION: Checks deviation in tensor data.
        public bool Analyze(SensorData data)
        {
            if (data is TensorSensorData tensorData)
            {
                Console.WriteLine("  [Stat Engine] Calculating variance...");
                // Logic: If any axis is near zero, it might be a sensor failure.
                float magnitude = tensorData.X_Axis + tensorData.Y_Axis + tensorData.Z_Axis;
                return magnitude < 0.5f; 
            }

            Console.WriteLine("  [Stat Engine] Cannot analyze non-tensor data.");
            return false;
        }

        public string GetEngineName() => "Statistical Variance Analyzer";
    }

    // Engine 3: Neural Network Simulator (Mock)
    public class NeuralEngine : IInferenceEngine
    {
        // IMPLEMENTATION: A complex "black box" decision.
        public bool Analyze(SensorData data)
        {
            // In a real scenario, this would process weights and layers.
            // Here, we simulate a decision based on timestamp.
            Console.WriteLine("  [Neural Engine] Processing through layers...");
            
            // Simulating a random failure chance for demonstration
            Random rnd = new Random();
            int seed = (int)(data.Timestamp.Ticks % 100);
            return (seed > 80); // Only flags anomalies on rare timestamps
        }

        public string GetEngineName() => "Neural Network Simulator";
    }

    // ---------------------------------------------------------
    // 5. MAIN SYSTEM (CLIENT CODE)
    // This demonstrates the runtime swapping of engines.
    // Notice how the AnalyzeData method does not change regardless
    // of which engine is passed in.
    // ---------------------------------------------------------
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Smart Factory System Initializing ===\n");

            // Create sample data streams
            SensorData tempReading = new FloatSensorData 
            { 
                Timestamp = DateTime.Now, 
                Value = 120.5f 
            };

            SensorData vibrationReading = new TensorSensorData 
            { 
                Timestamp = DateTime.Now.AddSeconds(1), 
                X_Axis = 0.1f, 
                Y_Axis = 0.2f, 
                Z_Axis = 0.05f 
            };

            // --- SCENARIO 1: Using Threshold Engine ---
            Console.WriteLine("--- SCENARIO 1: Threshold Analysis ---");
            IInferenceEngine engineA = new ThresholdEngine(100.0f);
            ProcessData(tempReading, engineA);
            ProcessData(vibrationReading, engineA); // Should fail gracefully

            Console.WriteLine();

            // --- SCENARIO 2: Swapping to Statistical Engine ---
            Console.WriteLine("--- SCENARIO 2: Statistical Analysis ---");
            IInferenceEngine engineB = new StatisticalEngine();
            ProcessData(tempReading, engineB); // Should fail gracefully
            ProcessData(vibrationReading, engineB); // Should detect anomaly

            Console.WriteLine();

            // --- SCENARIO 3: Swapping to Neural Engine ---
            Console.WriteLine("--- SCENARIO 3: Neural Analysis ---");
            IInferenceEngine engineC = new NeuralEngine();
            ProcessData(tempReading, engineC);
            
            Console.WriteLine("\n=== System Halted ===");
        }

        // The Core Logic: This method accepts the ABSTRACTION (Interface),
        // not the concrete implementation. This is the essence of Polymorphism.
        public static void ProcessData(SensorData data, IInferenceEngine engine)
        {
            // 1. Display the data using the overridden virtual method
            string formattedData = data.GetFormattedData();
            Console.WriteLine($"Processing: {formattedData}");

            // 2. Analyze using the swapped engine
            bool isAnomaly = engine.Analyze(data);

            // 3. Report result
            if (isAnomaly)
            {
                Console.WriteLine($"  >> ALERT: Anomaly detected by {engine.GetEngineName()}!");
            }
            else
            {
                Console.WriteLine($"  >> Status: Normal.");
            }
        }
    }
}
