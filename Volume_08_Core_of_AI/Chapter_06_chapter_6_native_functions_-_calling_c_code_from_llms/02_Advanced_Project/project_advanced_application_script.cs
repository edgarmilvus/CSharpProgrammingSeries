
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

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace SmartHomeEnergyApp
{
    // ==========================================
    // 1. DATA MODELS
    // ==========================================
    // We define simple classes to represent complex data structures.
    // These will be automatically serialized/deserialized by the Kernel.
    public class DeviceStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public int PowerConsumptionWatts { get; set; } // 0 if off
    }

    public class EnergyReport
    {
        public double TotalKwhConsumed { get; set; }
        public double EstimatedCost { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    // ==========================================
    // 2. NATIVE PLUGIN: ENERGY MANAGER
    // ==========================================
    // This class encapsulates the deterministic logic for energy management.
    // It uses the [KernelFunction] attribute to expose methods to the LLM.
    public class EnergyManagerPlugin
    {
        // Hardcoded electricity rate for calculation (e.g., $0.15 per kWh)
        private const double ElectricityRate = 0.15;

        /// <summary>
        /// Calculates the total energy consumption and cost for a list of active devices.
        /// </summary>
        [KernelFunction("calculate_energy_report")]
        [Description("Calculates total energy consumption in kWh and estimated cost based on device status.")]
        public EnergyReport CalculateEnergyReport(
            [Description("List of devices with their current status")] List<DeviceStatus> devices,
            [Description("Hours of usage to calculate for")] double hours)
        {
            double totalWatts = 0;
            int activeCount = 0;

            // Iterate through devices using basic loops (no LINQ)
            foreach (var device in devices)
            {
                if (device.IsOn)
                {
                    totalWatts += device.PowerConsumptionWatts;
                    activeCount++;
                }
            }

            // Calculate kWh (Watts * Hours / 1000)
            double kwh = (totalWatts * hours) / 1000.0;
            double cost = kwh * ElectricityRate;

            // Generate a recommendation string based on logic
            string recommendation = "Energy usage is normal.";
            if (cost > 5.0)
            {
                recommendation = "High energy usage detected. Consider turning off non-essential devices.";
            }
            else if (activeCount == 0)
            {
                recommendation = "No devices are active. System is in standby mode.";
            }

            return new EnergyReport
            {
                TotalKwhConsumed = Math.Round(kwh, 2),
                EstimatedCost = Math.Round(cost, 2),
                Recommendation = recommendation
            };
        }

        /// <summary>
        /// Identifies high-consumption devices to suggest turning off.
        /// </summary>
        [KernelFunction("suggest_energy_savers")]
        [Description("Identifies devices consuming more than a threshold watts.")]
        public List<string> SuggestEnergySavers(
            [Description("List of devices with their current status")] List<DeviceStatus> devices,
            [Description("Power threshold in watts to consider a device high consumption")] int thresholdWatts)
        {
            List<string> suggestions = new List<string>();

            // Basic loop and conditional logic
            foreach (var device in devices)
            {
                if (device.IsOn && device.PowerConsumptionWatts > thresholdWatts)
                {
                    suggestions.Add($"Turn off '{device.Name}' (Consumes {device.PowerConsumptionWatts}W)");
                }
            }

            return suggestions;
        }
    }

    // ==========================================
    // 3. MAIN APPLICATION
    // ==========================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Smart Home Energy Management System ===\n");

            // --- Step 1: Initialize the Kernel ---
            // We use a simple kernel builder. In a real scenario, we would add LLM services here.
            // For this demo, we will simulate the LLM's function calling capability manually
            // to demonstrate how the native code is invoked.
            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromType<EnergyManagerPlugin>("Energy");
            var kernel = builder.Build();

            // --- Step 2: Simulate Real-World Data ---
            // In a real app, this data would come from IoT sensors.
            var devices = new List<DeviceStatus>
            {
                new DeviceStatus { Name = "Living Room AC", IsOn = true, PowerConsumptionWatts = 1500 },
                new DeviceStatus { Name = "Kitchen Fridge", IsOn = true, PowerConsumptionWatts = 150 },
                new DeviceStatus { Name = "Gaming PC", IsOn = true, PowerConsumptionWatts = 400 },
                new DeviceStatus { Name = "Bedroom Light", IsOn = false, PowerConsumptionWatts = 0 }
            };

            // Serialize data to JSON (simulating how LLMs send parameters)
            string devicesJson = JsonSerializer.Serialize(devices);
            Console.WriteLine($"[System] Current Devices Status:\n{devicesJson}\n");

            // --- Step 3: Simulate LLM Function Calling ---
            // The LLM decides to call 'calculate_energy_report' with specific arguments.
            // In a real implementation, the LLM generates this JSON.
            var functionCallArgs = new KernelArguments
            {
                ["devices"] = devices,
                ["hours"] = 24.0 // Usage over 24 hours
            };

            Console.WriteLine("[LLM] Deciding to invoke: calculate_energy_report");
            
            // --- Step 4: Execute Native Code via Kernel ---
            var reportFunction = kernel.Plugins.GetFunction("Energy", "calculate_energy_report");
            var reportResult = await kernel.InvokeAsync(reportFunction, functionCallArgs);

            // Deserialize result back to our C# object
            var report = JsonSerializer.Deserialize<EnergyReport>(reportResult.ToString())!;

            Console.WriteLine("\n--- Generated Energy Report ---");
            Console.WriteLine($"Total Consumption: {report.TotalKwhConsumed} kWh");
            Console.WriteLine($"Estimated Cost: ${report.EstimatedCost}");
            Console.WriteLine($"Recommendation: {report.Recommendation}");

            // --- Step 5: Second Function Call (Optimization) ---
            // The LLM analyzes the report and decides to find energy savers.
            Console.WriteLine("\n[LLM] Deciding to invoke: suggest_energy_savers (Threshold: 300W)");

            var saverArgs = new KernelArguments
            {
                ["devices"] = devices,
                ["thresholdWatts"] = 300
            };

            var saverFunction = kernel.Plugins.GetFunction("Energy", "suggest_energy_savers");
            var saverResult = await kernel.InvokeAsync(saverFunction, saverArgs);

            // The result is a List<string>, serialized as JSON array
            var suggestions = JsonSerializer.Deserialize<List<string>>(saverResult.ToString())!;

            Console.WriteLine("\n--- Optimization Suggestions ---");
            foreach (var suggestion in suggestions)
            {
                Console.WriteLine($"[Action] {suggestion}");
            }

            Console.WriteLine("\n=== End of Simulation ===");
        }
    }
}
