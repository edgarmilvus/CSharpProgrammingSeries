
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

namespace MathForAI
{
    // We define a class to represent a single data point (a sensor reading).
    // In AI, data often comes in "vectors" or lists of numbers.
    // We use a Class (Chapter 16) to group related data together.
    class SensorReading
    {
        // Fields (Chapter 17) to store the raw values.
        // We use 'double' because mathematical calculations for AI often require decimal precision.
        public double Temperature;
        public double Humidity;
        public double Pressure;

        // Constructor (Chapter 18) to initialize the reading with specific values.
        public SensorReading(double temp, double hum, double press)
        {
            Temperature = temp;
            Humidity = hum;
            Pressure = press;
        }
    }

    class Program
    {
        // We define static fields (Chapter 19) to hold our configuration.
        // These are shared across the application and represent the "weights" or thresholds.
        // In a real AI model, these numbers are learned from data. Here, we set them manually.
        static double WeightTemperature = 0.6;
        static double WeightHumidity = 0.3;
        static double WeightPressure = 0.1;
        static double DecisionThreshold = 0.5; // A value to compare against.

        // Main entry point.
        static void Main(string[] args)
        {
            Console.WriteLine("--- AI Data Processing System ---");
            Console.WriteLine("Calculating Sigmoid Activation and Euclidean Distances.");
            Console.WriteLine();

            // 1. DATA COLLECTION
            // We create a List (Chapter 20) to hold multiple SensorReading objects.
            // This allows us to process data dynamically.
            List<SensorReading> dataset = new List<SensorReading>();

            // Adding sample data manually (simulating input from sensors).
            // In a real app, this might come from Console.ReadLine (Chapter 5) or a file.
            dataset.Add(new SensorReading(22.5, 45.0, 1013.2));
            dataset.Add(new SensorReading(30.1, 80.0, 1005.5));
            dataset.Add(new SensorReading(18.0, 40.0, 1020.0));

            // 2. PROCESSING LOOP
            // We use a foreach loop (Chapter 12) to iterate over the list.
            // This allows us to apply the same math to every data point.
            int index = 1;
            foreach (SensorReading reading in dataset)
            {
                Console.WriteLine($"Processing Reading #{index}:");
                Console.WriteLine($"  Raw Data: Temp={reading.Temperature}C, Hum={reading.Humidity}%, Press={reading.Pressure}hPa");

                // --- A. CALCULATE RAW SCORE (Weighted Sum) ---
                // We perform basic arithmetic (Chapter 4) to combine the inputs.
                // Formula: Score = (Temp * W_Temp) + (Hum * W_Hum) + (Press * W_Press)
                double rawScore = (reading.Temperature * WeightTemperature) +
                                  (reading.Humidity * WeightHumidity) +
                                  (reading.Pressure * WeightPressure);

                Console.WriteLine($"  Raw Weighted Score: {rawScore:F4}");

                // --- B. SIGMOID ACTIVATION ---
                // We use System.Math.Pow (Chapter 21) to implement the Sigmoid function.
                // The Sigmoid function squashes any number into a range between 0 and 1.
                // This is crucial for probability estimation in AI.
                // Formula: 1 / (1 + e^(-x))
                double sigmoidOutput = CalculateSigmoid(rawScore);

                Console.WriteLine($"  Sigmoid Activation (Probability): {sigmoidOutput:F4}");

                // --- C. DECISION LOGIC ---
                // We use if/else statements (Chapter 6) to make a decision based on the math.
                // We compare the sigmoid output to our threshold.
                if (sigmoidOutput > DecisionThreshold)
                {
                    Console.WriteLine("  Decision: ANOMALY DETECTED (High Probability)");
                }
                else
                {
                    Console.WriteLine("  Decision: NORMAL OPERATION");
                }

                // --- D. DISTANCE CALCULATION (Euclidean) ---
                // We calculate how far this reading is from a "standard" or "ideal" reading.
                // This uses System.Math.Sqrt (Chapter 21).
                // Formula: Distance = sqrt((x1-x2)^2 + (y1-y2)^2 + ...)
                double distance = CalculateEuclideanDistance(reading);

                Console.WriteLine($"  Distance from Ideal: {distance:F4}");
                Console.WriteLine(); // Empty line for readability
                index++;
            }

            // 3. ADVANCED LOGIC: FINDING THE CLOSEST POINT
            // We iterate again to find which data point is closest to the ideal.
            // This requires tracking the minimum distance found so far.
            Console.WriteLine("--- Analysis Complete ---");
            AnalyzeClosestPoint(dataset);
        }

        // ---------------------------------------------------------
        // METHOD: Calculate Sigmoid
        // Purpose: Transforms a raw score into a probability (0 to 1).
        // Uses: System.Math.Pow (Chapter 21), Arithmetic (Chapter 4).
        // ---------------------------------------------------------
        static double CalculateSigmoid(double x)
        {
            // We use Math.Pow(base, exponent) to calculate e^(-x).
            // In AI, this is the "activation" step.
            // If x is very positive, e^(-x) is near 0, so result is near 1.
            // If x is very negative, e^(-x) is huge, so result is near 0.
            double ePowerNegX = Math.Pow(Math.E, -x);
            
            // Calculate 1 divided by (1 + ePowerNegX).
            return 1.0 / (1.0 + ePowerNegX);
        }

        // ---------------------------------------------------------
        // METHOD: Calculate Euclidean Distance
        // Purpose: Measures geometric distance between two points in space.
        // Uses: System.Math.Sqrt (Chapter 21), System.Math.Pow (Chapter 21).
        // ---------------------------------------------------------
        static double CalculateEuclideanDistance(SensorReading current)
        {
            // We define an "Ideal" reference point.
            // In AI, this could be a cluster center or a target state.
            double idealTemp = 20.0;
            double idealHum = 45.0;
            double idealPress = 1013.0;

            // Calculate the squared difference for each dimension.
            // (Current - Ideal)^2
            double diffTemp = current.Temperature - idealTemp;
            double sqDiffTemp = Math.Pow(diffTemp, 2);

            double diffHum = current.Humidity - idealHum;
            double sqDiffHum = Math.Pow(diffHum, 2);

            double diffPress = current.Pressure - idealPress;
            double sqDiffPress = Math.Pow(diffPress, 2);

            // Sum the squared differences.
            double sumSqDiff = sqDiffTemp + sqDiffHum + sqDiffPress;

            // Take the square root to get the final distance.
            return Math.Sqrt(sumSqDiff);
        }

        // ---------------------------------------------------------
        // METHOD: Analyze Closest Point
        // Purpose: Iterates through data to find the minimum distance.
        // Uses: foreach (Chapter 12), if/else (Chapter 6), variables (Chapter 2).
        // ---------------------------------------------------------
        static void AnalyzeClosestPoint(List<SensorReading> data)
        {
            // We need variables to track the best candidate found so far.
            double minDistance = double.MaxValue; // Start with the largest possible number.
            int closestIndex = -1;

            int currentIndex = 1;
            foreach (SensorReading reading in data)
            {
                // Calculate distance for this specific reading.
                double dist = CalculateEuclideanDistance(reading);

                // Check if this distance is smaller than the smallest one we've seen.
                if (dist < minDistance)
                {
                    // Update our tracking variables.
                    minDistance = dist;
                    closestIndex = currentIndex;
                }
                currentIndex++;
            }

            // Output the result.
            if (closestIndex != -1)
            {
                Console.WriteLine($"The reading closest to the ideal state is Reading #{closestIndex}.");
                Console.WriteLine($"Distance: {minDistance:F4}");
            }
            else
            {
                Console.WriteLine("No data available to analyze.");
            }
        }
    }
}
