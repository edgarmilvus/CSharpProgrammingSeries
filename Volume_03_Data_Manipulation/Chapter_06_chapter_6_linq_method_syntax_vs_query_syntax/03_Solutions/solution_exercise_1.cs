
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

public void ExecuteExercise1()
{
    // 1. Define the source (Deferred)
    var sourceStream = DataSource.GetReadings();

    // 2. Define the filtering logic (Deferred)
    // Note: We cannot calculate the Min inside the Select lambda because 
    // the query hasn't executed yet. We must calculate Min on the filtered set first.
    
    // Step A: Isolate the relevant data
    var zoneAStream = sourceStream
        .Where(r => r.IsValid)
        .Where(r => r.Location == "Zone A");

    // Step B: Calculate statistics (Immediate Execution)
    // We force execution here to get the scalar value needed for normalization.
    // If we don't do this, the Min would be calculated for *every* item in the projection,
    // or worse, cause a closure capture issue if not handled correctly.
    double minVal = zoneAStream.Min(r => r.Value);

    // Step C: Project the normalized data (Deferred)
    // We re-iterate the zoneAStream. Since it is an IEnumerable, we must be aware 
    // that if the source was an I/O stream (like a file), this might fail. 
    // However, for in-memory collections, it is safe.
    var normalizedReadings = zoneAStream
        .Select(r => new 
        { 
            r.SensorId, 
            r.Timestamp, 
            NormalizedValue = r.Value - minVal 
        });

    // 3. Execution (Immediate)
    // The pipeline executes only when we iterate (e.g., ToList, foreach)
    var results = normalizedReadings.ToList();

    Console.WriteLine($"Minimum Value in Zone A: {minVal}");
    foreach (var item in results)
    {
        Console.WriteLine($"Sensor: {item.SensorId}, Norm: {item.NormalizedValue:F2}");
    }
}
