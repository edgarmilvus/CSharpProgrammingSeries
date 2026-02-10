
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace TravelPlanner
{
    // 2. Define the Disruption Model
    public enum DisruptionType { Traffic, Weather, Mechanical }

    public record Disruption
    {
        public DisruptionType Type { get; init; }
        public int Severity { get; init; } // 1-10
        public string Description { get; init; } = string.Empty;
    }

    public class TravelItineraryPlanner
    {
        [KernelFunction, Description("Calculates travel time considering distance, base speed, and disruptions.")]
        public double CalculateTravelTime(
            [Description("Distance in miles")] double distance,
            [Description("Base speed in mph")] double baseSpeed,
            [Description("List of disruptions affecting the trip")] IList<Disruption> disruptions)
        {
            // 3. Update the Logic
            double effectiveSpeed = baseSpeed;

            if (disruptions != null)
            {
                foreach (var disruption in disruptions)
                {
                    // Formula: Reduce speed based on severity (e.g., severity 5 reduces speed by 10mph)
                    effectiveSpeed -= (disruption.Severity * 2);
                }
            }

            // Ensure speed never drops below 10 mph
            effectiveSpeed = Math.Max(effectiveSpeed, 10);

            // Calculate time
            return distance / effectiveSpeed;
        }
    }

    /*
     * 5. Test Scenario / Prompt Engineering
     * 
     * To invoke this function effectively, the LLM needs to understand the structure of the 'disruptions' parameter.
     * 
     * User Prompt Example:
     * "Calculate the travel time for a 100-mile trip at 60mph. 
     *  There is heavy traffic (severity 8) and light rain (severity 3)."
     * 
     * Expected LLM Internal Invocation:
     * The LLM should map 'heavy traffic' to Type: Traffic, Severity: 8, 
     * and 'light rain' to Type: Weather, Severity: 3.
     * 
     * It will then serialize this into a JSON array for the 'disruptions' parameter:
     * [
     *   { "Type": "Traffic", "Severity": 8, "Description": "Heavy traffic" },
     *   { "Type": "Weather", "Severity": 3, "Description": "Light rain" }
     * ]
     */
}
