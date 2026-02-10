
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace SmartHomePlugin
{
    // 1. Define the typed data contract using a modern C# record.
    // Using 'record' provides immutability, value-based equality, and deconstruction support.
    public record HomeEnvironment
    {
        public string RoomName { get; init; } = string.Empty;
        public double TemperatureCelsius { get; init; }
        
        // 4. Edge Case Consideration: We handle clamping in the property setter or the function logic.
        // Here, we use a private backing field to enforce the 0-100 constraint.
        private int _lightBrightness;
        public int LightBrightness
        {
            get => _lightBrightness;
            init => _lightBrightness = Math.Clamp(value, 0, 100); // Clamp to valid range
        }
        
        public string LightColor { get; init; } = "#FFFFFF";
    }

    public class SmartHomeController
    {
        [KernelFunction, Description("Sets the environment for a specific room.")]
        public void SetEnvironmentAsync(
            [Description("The room configuration details")] HomeEnvironment environment)
        {
            // 3. Output the JSON representation to verify binding.
            // We use JsonSerializerOptions for pretty printing.
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonOutput = JsonSerializer.Serialize(environment, options);
            
            Console.WriteLine("--- Smart Home Controller Received ---");
            Console.WriteLine(jsonOutput);
            Console.WriteLine("--------------------------------------");
        }
    }
}
