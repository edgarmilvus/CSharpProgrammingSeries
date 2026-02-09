
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

// Define the Sensor class to represent individual smart sensors
public class Sensor
{
    public string Id { get; set; }
    public double Temperature { get; set; }
    public double Pressure { get; set; }
    public double Vibration { get; set; }

    public Sensor(string id, double temp, double press, double vib)
    {
        Id = id;
        Temperature = temp;
        Pressure = press;
        Vibration = vib;
    }
}

// Define the AI Monitoring System that processes sensor data
public class AIMonitoringSystem
{
    // List to hold our sensor network (simulating real-time data collection)
    private List<Sensor> _sensorNetwork;

    public AIMonitoringSystem()
    {
        _sensorNetwork = new List<Sensor>();
    }

    // Method to add a sensor to the network
    public void AddSensor(Sensor sensor)
    {
        _sensorNetwork.Add(sensor);
    }

    // Core method: Process all sensors using a passed processing function (Func) and reporting action (Action)
    // Func<Sensor, double>: Takes a Sensor and returns a double (e.g., an anomaly score)
    // Action<Sensor, double>: Takes a Sensor and the processed result, performs a side effect (e.g., logging)
    public void ProcessNetwork(Func<Sensor, double> processingLogic, Action<Sensor, double> reportingAction)
    {
        // Iterate through each sensor in the network
        foreach (Sensor sensor in _sensorNetwork)
        {
            // Apply the processing logic (Func) to get a result
            double result = processingLogic(sensor);

            // Apply the reporting action (Action) to handle the result
            reportingAction(sensor, result);
        }
    }
}

// Main program to demonstrate the system
public class Program
{
    public static void Main()
    {
        // 1. Instantiate the monitoring system
        AIMonitoringSystem monitor = new AIMonitoringSystem();

        // 2. Add some sample sensors (simulating real-world deployment)
        monitor.AddSensor(new Sensor("Sensor-01", 25.5, 101.3, 0.2));
        monitor.AddSensor(new Sensor("Sensor-02", 85.0, 105.6, 2.5)); // Potential anomaly
        monitor.AddSensor(new Sensor("Sensor-03", 30.1, 98.7, 0.1));
        monitor.AddSensor(new Sensor("Sensor-04", 120.0, 110.0, 3.8)); // Critical anomaly

        // 3. Define processing logic using a lambda expression (Func)
        // This function calculates an "Anomaly Score" based on temperature, pressure, and vibration.
        // Higher score indicates higher likelihood of system failure.
        Func<Sensor, double> anomalyScoreLogic = (sensor) =>
        {
            // Simple weighted formula for demonstration
            double tempScore = Math.Max(0, sensor.Temperature - 70) * 0.5;
            double pressureScore = Math.Max(0, sensor.Pressure - 102) * 0.3;
            double vibrationScore = Math.Max(0, sensor.Vibration - 1.0) * 0.2;
            return tempScore + pressureScore + vibrationScore;
        };

        // 4. Define reporting logic using a lambda expression (Action)
        // This action logs the sensor data and its anomaly score to the console.
        // It also includes conditional logic for alerts.
        Action<Sensor, double> logReportAction = (sensor, score) =>
        {
            Console.WriteLine($"Sensor: {sensor.Id} | Temp: {sensor.Temperature}°C | Press: {sensor.Pressure} kPa | Vib: {sensor.Vibration} mm/s");
            Console.WriteLine($"   -> Anomaly Score: {score:F2}");

            // Conditional reporting based on the score
            if (score > 10.0)
            {
                Console.WriteLine("   -> ALERT: CRITICAL ANOMALY DETECTED! Immediate maintenance required.");
            }
            else if (score > 5.0)
            {
                Console.WriteLine("   -> WARNING: Elevated anomaly. Schedule inspection.");
            }
            else
            {
                Console.WriteLine("   -> STATUS: Normal operating parameters.");
            }
            Console.WriteLine(); // Empty line for readability
        };

        // 5. Execute the monitoring process
        // We pass our defined logic (anomalyScoreLogic) and reporting (logReportAction) to the system.
        Console.WriteLine("=== Starting Real-Time AI Monitoring System ===");
        Console.WriteLine();
        monitor.ProcessNetwork(anomalyScoreLogic, logReportAction);

        // 6. Demonstrate flexibility: Change the processing logic on the fly
        // Define a new Func for "Efficiency Analysis" instead of anomaly detection.
        // This calculates a normalized efficiency score (0-100).
        Func<Sensor, double> efficiencyLogic = (sensor) =>
        {
            // Ideal conditions: Temp ~30, Pressure ~100, Vibration ~0.1
            double tempDiff = Math.Abs(sensor.Temperature - 30);
            double pressDiff = Math.Abs(sensor.Pressure - 100);
            double vibDiff = Math.Abs(sensor.Vibration - 0.1);

            // Convert differences to a penalty score, then invert to efficiency
            double penalty = (tempDiff * 0.4) + (pressDiff * 0.3) + (vibDiff * 0.3);
            double efficiency = 100 - (penalty * 2); // Scale penalty
            return Math.Max(0, efficiency); // Ensure non-negative
        };

        // Define a new reporting action for efficiency (e.g., send to a different dashboard)
        Action<Sensor, double> efficiencyReportAction = (sensor, score) =>
        {
            Console.WriteLine($"[EFFICIENCY DASHBOARD] Sensor: {sensor.Id}");
            Console.WriteLine($"   -> Efficiency Score: {score:F1}%");
            if (score < 60)
            {
                Console.WriteLine("   -> RECOMMENDATION: Optimize sensor parameters.");
            }
            Console.WriteLine();
        };

        // 7. Run the system with the new logic
        Console.WriteLine("=== Switching to Efficiency Analysis Mode ===");
        Console.WriteLine();
        monitor.ProcessNetwork(efficiencyLogic, efficiencyReportAction);
    }
}
