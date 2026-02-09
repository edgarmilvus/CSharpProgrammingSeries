
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Runtime;

public struct GCProfile
{
    public bool IsServer { get; set; }
    public GCLatencyMode LatencyMode { get; set; }
}

public class RuntimeConfigurator
{
    public void ValidateGCSettings(GCProfile target)
    {
        Console.WriteLine("Validating Runtime GC Settings...");
        
        // Check Server GC
        bool isServer = GCSettings.IsServerGC;
        Console.WriteLine($"Current IsServerGC: {isServer}");
        if (target.IsServer && !isServer)
        {
            throw new InvalidOperationException(
                "Target profile requires Server GC, but Workstation GC is active. " +
                "Please update .csproj: <ServerGarbageCollection>true</ServerGarbageCollection>");
        }

        // Check Latency Mode
        var currentMode = GCSettings.LatencyMode;
        Console.WriteLine($"Current LatencyMode: {currentMode}");
        if (target.LatencyMode != currentMode)
        {
            // SustainedLowLatency is a runtime configuration, but we can set it programmatically 
            // if the process allows it (requires elevated privileges or specific runtime conditions).
            try
            {
                GCSettings.LatencyMode = target.LatencyMode;
                Console.WriteLine($"Updated LatencyMode to: {target.LatencyMode}");
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to set LatencyMode to {target.LatencyMode}. " +
                    "This usually happens under high memory pressure or if the mode is restricted.", ex);
            }
        }
    }

    public void SimulateStartup(string configFilePath)
    {
        // Mocking the JSON config reading
        // In a real scenario: var json = File.ReadAllText(configFilePath);
        // var config = JsonSerializer.Deserialize<GCProfile>(json);
        
        Console.WriteLine($"Reading configuration from {configFilePath}...");
        
        // Simulated configuration for High Throughput Inference
        var targetProfile = new GCProfile
        {
            IsServer = true,
            LatencyMode = GCLatencyMode.SustainedLowLatency
        };

        try
        {
            ValidateGCSettings(targetProfile);
            Console.WriteLine("GC Configuration validated successfully.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Configuration Error: {ex.Message}");
            // In a real app, this might trigger a restart or exit
            throw;
        }
    }
}
