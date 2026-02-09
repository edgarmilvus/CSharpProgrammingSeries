
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

public class SensorMetadata
{
    public string SensorId { get; set; }
    public double CalibrationOffset { get; set; }
}

public void ExecuteExercise3()
{
    var readings = DataSource.GetReadings();
    var metadata = new List<SensorMetadata>
    {
        new SensorMetadata { SensorId = "S1", CalibrationOffset = 1.5 },
        new SensorMetadata { SensorId = "S2", CalibrationOffset = -2.0 },
        new SensorMetadata { SensorId = "S3", CalibrationOffset = 0.5 }
        // S4 has no metadata
    };

    // 1. Define the query (Deferred Execution)
    // We start with Query Syntax for the declarative join...
    var query = 
        from r in readings
        join m in metadata on r.SensorId equals m.SensorId
        // ...and immediately switch to Method Syntax for the pipeline
        select new { Reading = r, Meta = m } into rm
        where rm.Reading.IsValid
        select new 
        { 
            SensorId = rm.Reading.SensorId,
            CalibratedValue = rm.Reading.Value + rm.Meta.CalibrationOffset,
            Timestamp = rm.Reading.Timestamp
        };

    // 2. Execute the pipeline
    // At this point, the join logic and projections are applied.
    var results = query.ToList();

    foreach (var item in results)
    {
        Console.WriteLine($"Sensor: {item.SensorId}, Calibrated: {item.CalibratedValue:F2}");
    }
}
