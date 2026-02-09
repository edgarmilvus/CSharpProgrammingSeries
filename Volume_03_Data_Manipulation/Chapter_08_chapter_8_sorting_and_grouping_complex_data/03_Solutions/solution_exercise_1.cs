
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// --- Domain Models ---
public record Location(string Zone, int RackId);
public record SensorReading(string Id, Location Location, double Temperature, double BatteryLevel, string TimestampString);
public record CleanInput(string SensorId, DateTime Timestamp, double NormalizedTemp);

public class DataCleaningExercise
{
    public static List<CleanInput> ProcessReadings(List<SensorReading> rawStream)
    {
        // The pipeline is defined here. 
        // 1. Where filters the source stream based on criteria.
        // 2. Select projects the filtered items into the new shape.
        // 3. ToList forces immediate execution, materializing the results.
        
        return rawStream
            .Where(r => r.BatteryLevel > 0 && r.Temperature > -100) // Filter invalid data
            .Select(r => new CleanInput(
                SensorId: r.Id,
                Timestamp: DateTime.Parse(r.TimestampString), // Parsing string to DateTime
                NormalizedTemp: (r.Temperature - 25.0) / 10.0  // Normalization logic
            ))
            .ToList(); 
    }
}
