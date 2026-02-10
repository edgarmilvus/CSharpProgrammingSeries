
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

public void ExecuteExercise4()
{
    var source = DataSource.GetReadings();

    // 1. Define the Parallel Query (Deferred)
    // AsParallel() enables partitioning of the data source.
    var parallelQuery = source
        .AsParallel()
        .Where(r => r.IsValid)
        .Select(r => 
        {
            // Simulate heavy CPU-bound work
            // In a real scenario, this might be a complex vector normalization
            double complexCalc = Math.Sqrt(r.Value * r.Value + 100); 
            // Note: Thread.Sleep is used here purely for simulation of latency.
            // In production, avoid blocking calls inside PLINQ.
            System.Threading.Thread.Sleep(10); 
            
            return new 
            { 
                Id = r.SensorId, 
                ProcessedValue = complexCalc 
            };
        });

    Console.WriteLine("Query defined. Execution is deferred.");

    // 2. Trigger Execution
    // The pipeline starts now. The work is distributed across available cores.
    // ToList() forces immediate execution and aggregation of results.
    var processedData = parallelQuery.ToList();

    Console.WriteLine($"Processing complete. {processedData.Count} items processed.");

    // 3. Safe Consumption (Side effects contained outside the query)
    foreach (var item in processedData)
    {
        Console.WriteLine($"ID: {item.Id}, Value: {item.ProcessedValue:F2}");
    }
}
