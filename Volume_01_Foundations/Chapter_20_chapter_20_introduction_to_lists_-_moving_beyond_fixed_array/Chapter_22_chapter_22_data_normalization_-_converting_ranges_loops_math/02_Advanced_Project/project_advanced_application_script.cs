
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic; // Required for List<T>

namespace SmartHomeLighting
{
    class Program
    {
        // Global constants for our lighting system configuration
        // These define the standard range we want our output to be in (0% to 100%)
        const double TargetMin = 0.0;
        const double TargetMax = 100.0;

        static void Main(string[] args)
        {
            Console.WriteLine("--- Smart Home Lighting: Data Normalization System ---");
            Console.WriteLine("System initializing...");
            Console.WriteLine();

            // 1. SETUP: Simulate raw sensor data from different rooms
            // In a real system, these values might come from photoresistors.
            // Notice the wide, inconsistent ranges (e.g., 0-1023 vs 0-5.0).
            
            // Kitchen Sensor: Analog reading (0 to 1023)
            int[] kitchenRawReadings = new int[5] { 50, 300, 650, 900, 1023 };
            
            // Living Room Sensor: Voltage reading (0.0 to 5.0)
            double[] livingRoomRawReadings = new double[5] { 0.1, 1.2, 2.5, 3.8, 4.9 };
            
            // Hallway Sensor: Percentage (0 to 1) - already normalized, but we will re-normalize to 0-100
            double[] hallwayRawReadings = new double[5] { 0.05, 0.25, 0.50, 0.75, 0.95 };

            // 2. PROCESSING: Normalize Kitchen Data
            Console.WriteLine("Processing Kitchen Sensor Data (Range 0-1023)...");
            Console.WriteLine("Raw\t|\tNormalized Brightness (%)");
            Console.WriteLine("-----------------------------------------");
            
            for (int i = 0; i < kitchenRawReadings.Length; i++)
            {
                // Get the current raw value
                double currentRaw = kitchenRawReadings[i];
                
                // Calculate the normalized value using the Linear Interpolation formula
                // Formula: normalized = (value - min) / (max - min) * (targetMax - targetMin) + targetMin
                double normalizedBrightness = NormalizeRange(currentRaw, 0, 1023, TargetMin, TargetMax);
                
                // Display the result
                Console.WriteLine($"{currentRaw}\t|\t{normalizedBrightness:F2}%");
            }
            Console.WriteLine();

            // 3. PROCESSING: Normalize Living Room Data
            Console.WriteLine("Processing Living Room Sensor Data (Range 0.0-5.0)...");
            Console.WriteLine("Raw\t|\tNormalized Brightness (%)");
            Console.WriteLine("-----------------------------------------");

            // Using a foreach loop (Chapter 12) for cleaner iteration
            foreach (double rawValue in livingRoomRawReadings)
            {
                double normalizedBrightness = NormalizeRange(rawValue, 0.0, 5.0, TargetMin, TargetMax);
                Console.WriteLine($"{rawValue}\t|\t{normalizedBrightness:F2}%");
            }
            Console.WriteLine();

            // 4. ADVANCED: Dynamic Thresholding
            // We will now process the hallway data, but only apply lights if the normalized value
            // is above a certain safety threshold (e.g., don't turn on lights if it's already bright).
            
            Console.WriteLine("Processing Hallway with Safety Threshold (Min 20%)...");
            Console.WriteLine("Raw\t|\tNormalized\t|\tAction");
            Console.WriteLine("--------------------------------------------------");

            foreach (double rawValue in hallwayRawReadings)
            {
                double normalized = NormalizeRange(rawValue, 0.0, 1.0, TargetMin, TargetMax);
                
                // Logic to determine action based on normalized data
                string action;
                if (normalized < 20.0)
                {
                    action = "Turn Lights ON (High Brightness)";
                }
                else if (normalized >= 20.0 && normalized < 80.0)
                {
                    action = "Dim Lights (Ambient Light Detected)";
                }
                else
                {
                    action = "Turn Lights OFF (High Ambient Light)";
                }

                Console.WriteLine($"{rawValue}\t|\t{normalized:F2}%\t\t|\t{action}");
            }

            Console.WriteLine("\nSystem Normalization Complete.");
        }

        /// <summary>
        /// Converts a value from one arbitrary range to another using linear interpolation.
        /// This method encapsulates the math logic defined in Chapter 22.
        /// </summary>
        /// <param name="value">The input value to normalize.</param>
        /// <param name="sourceMin">The minimum bound of the input range.</param>
        /// <param name="sourceMax">The maximum bound of the input range.</param>
        /// <param name="targetMin">The desired minimum bound of the output range.</param>
        /// <param name="targetMax">The desired maximum bound of the output range.</param>
        /// <returns>The value mapped to the target range.</returns>
        static double NormalizeRange(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            // Step 1: Calculate the size of the source range
            double sourceRange = sourceMax - sourceMin;

            // Step 2: Calculate the size of the target range
            double targetRange = targetMax - targetMin;

            // Step 3: Calculate where the value sits relative to the source minimum (0.0 to 1.0 scale)
            // We use Math.Abs to ensure we don't divide by zero if ranges are identical
            double scaledValue = (value - sourceMin) / Math.Abs(sourceRange);

            // Step 4: Map the scaled value to the target range
            double finalValue = (scaledValue * targetRange) + targetMin;

            return finalValue;
        }
    }
}
