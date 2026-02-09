
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

namespace AI_Data_Processing
{
    // Real-world context: We are building a data pipeline for an autonomous vehicle's
    // perception system. The car receives a stream of sensor readings (LiDAR points)
    // represented as Tensors (3D coordinates). We need to filter this data in real-time
    // to identify valid road obstacles without the overhead of defining full classes
    // for every simple transformation.

    // Delegate Definition: This is the contract for our filtering logic.
    // It accepts a 3D point and returns a boolean (true to keep, false to discard).
    public delegate bool PointFilter(TensorPoint point);

    public class TensorPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Intensity { get; set; } // Reflection strength

        public TensorPoint(float x, float y, float z, float intensity)
        {
            X = x;
            Y = y;
            Z = z;
            Intensity = intensity;
        }
    }

    public class SensorPipeline
    {
        // Simulating a raw data stream from the vehicle's sensors
        private List<TensorPoint> _rawStream;

        public SensorPipeline()
        {
            _rawStream = new List<TensorPoint>();
            GenerateMockData();
        }

        // 1. DATA GENERATION (Simulation)
        // We populate our "Tensor" with noise, ground points, and obstacle points.
        private void GenerateMockData()
        {
            // Road Surface (Low Z, varying intensity)
            _rawStream.Add(new TensorPoint(10.5f, 5.2f, 0.0f, 0.4f));
            _rawStream.Add(new TensorPoint(10.6f, 5.3f, 0.1f, 0.3f));
            
            // Obstacle (Pedestrian) - High Z, distinct intensity
            _rawStream.Add(new TensorPoint(15.0f, 8.0f, 1.7f, 0.9f));
            
            // Noise (Floating points or sensor errors)
            _rawStream.Add(new TensorPoint(0.0f, 0.0f, 100.0f, 0.0f));
            
            // Another Obstacle (Vehicle)
            _rawStream.Add(new TensorPoint(20.0f, 2.0f, 1.5f, 0.8f));
            
            // Ground clutter
            _rawStream.Add(new TensorPoint(10.7f, 5.4f, 0.05f, 0.2f));
            
            // Distant obstacle (beyond range)
            _rawStream.Add(new TensorPoint(50.0f, 50.0f, 2.0f, 0.5f));
        }

        // 2. CORE LOGIC: The Filter Method
        // This method accepts a delegate (lambda) to define the filtering criteria.
        // It isolates the iteration logic from the decision logic.
        public List<TensorPoint> FilterPoints(PointFilter criteria)
        {
            List<TensorPoint> result = new List<TensorPoint>();

            // Iterating through the raw data stream
            foreach (TensorPoint point in _rawStream)
            {
                // CRITICAL: We invoke the delegate passed in.
                // The specific logic is determined at runtime by the caller.
                if (criteria(point))
                {
                    result.Add(point);
                }
            }
            return result;
        }

        // 3. TRANSFORMATION LOGIC: Mapping Intensity
        // Applies a mathematical function to every point's intensity.
        // Uses a delegate to allow dynamic formulas.
        public List<TensorPoint> NormalizeIntensity(Func<TensorPoint, TensorPoint> transformLogic)
        {
            List<TensorPoint> normalized = new List<TensorPoint>();
            foreach (TensorPoint point in _rawStream)
            {
                // Apply the transformation lambda
                normalized.Add(transformLogic(point));
            }
            return normalized;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SensorPipeline pipeline = new SensorPipeline();

            Console.WriteLine("--- SCENARIO 1: Filtering Obstacles ---");
            Console.WriteLine("Goal: Identify objects taller than 1.2 meters (potential pedestrians/vehicles).");
            
            // LAMBDA INTRODUCTION:
            // Instead of creating a class 'TallObjectFilter' implementing 'PointFilter',
            // we define the logic inline using a lambda expression.
            // Syntax: (parameters) => { body }
            List<TensorPoint> obstacles = pipeline.FilterPoints(point => 
            {
                // Logic: Check Z coordinate (height) and Intensity
                bool isTall = point.Z > 1.2f;
                bool hasSignal = point.Intensity > 0.5f;
                return isTall && hasSignal;
            });

            PrintResults(obstacles);

            Console.WriteLine("\n--- SCENARIO 2: Dynamic Range Query ---");
            Console.WriteLine("Goal: Find all points within a 25-meter radius of the car (0,0).");

            // We can define the lambda separately to reuse logic or pass it directly.
            PointFilter rangeFilter = p => (p.X * p.X + p.Y * p.Y) <= (25 * 25);
            
            List<TensorPoint> nearbyPoints = pipeline.FilterPoints(rangeFilter);
            PrintResults(nearbyPoints);

            Console.WriteLine("\n--- SCENARIO 3: Data Transformation (Map) ---");
            Console.WriteLine("Goal: Boost intensity of all points by 20% for visualization.");

            // Here we use a Func delegate (built-in) for transformation.
            // Lambda: Input TensorPoint -> Output TensorPoint
            List<TensorPoint> boostedData = pipeline.NormalizeIntensity(point => 
            {
                return new TensorPoint(
                    point.X, 
                    point.Y, 
                    point.Z, 
                    point.Intensity * 1.2f // Boost logic
                );
            });

            Console.WriteLine("Boosted Intensities:");
            foreach(var p in boostedData)
            {
                Console.WriteLine($"  Point ({p.X}, {p.Y}, {p.Z}) -> Intensity: {p.Intensity:F2}");
            }

            Console.WriteLine("\n--- SCENARIO 4: Chaining Logic (Delegates as Data) ---");
            Console.WriteLine("Goal: Filter by range, THEN filter by height.");

            // We can compose logic by passing lambdas that call other lambdas.
            // Step 1: Define base filters
            PointFilter near = p => (p.X * p.X + p.Y * p.Y) <= (25 * 25);
            PointFilter tall = p => p.Z > 1.0f;

            // Step 2: Create a combined filter using a lambda that chains delegates
            PointFilter nearAndTall = p => near(p) && tall(p);

            List<TensorPoint> finalTargets = pipeline.FilterPoints(nearAndTall);
            PrintResults(finalTargets);
        }

        // Helper method to display results
        static void PrintResults(List<TensorPoint> points)
        {
            if (points.Count == 0)
            {
                Console.WriteLine("  No points matched criteria.");
                return;
            }

            Console.WriteLine($"  Found {points.Count} points:");
            foreach (var p in points)
            {
                Console.WriteLine($"    - Pos: ({p.X}, {p.Y}, {p.Z}) | Int: {p.Intensity}");
            }
        }
    }
}
