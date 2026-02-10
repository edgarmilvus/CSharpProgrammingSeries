
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record SensorData(int Id, double Value, DateTime Timestamp);

public static class AsyncPipelineExtensions
{
    // Extension to filter valid readings (Value >= 0)
    public static async IAsyncEnumerable<SensorData> FilterValidAsync(
        this IAsyncEnumerable<SensorData> source, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var data in source.WithCancellation(ct))
        {
            if (data.Value >= 0)
            {
                yield return data;
            }
        }
    }

    // Extension to transform to formatted strings
    public static async IAsyncEnumerable<string> TransformToReportAsync(
        this IAsyncEnumerable<SensorData> source, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var data in source.WithCancellation(ct))
        {
            yield return $"[{data.Timestamp:HH:mm:ss}] Sensor ID {data.Id}: {data.Value:F2}";
        }
    }

    // Extension to throttle the stream
    public static async IAsyncEnumerable<T> Throttle<T>(
        this IAsyncEnumerable<T> source, 
        TimeSpan interval, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var lastEmitTime = DateTime.MinValue;
        
        await foreach (var item in source.WithCancellation(ct))
        {
            var now = DateTime.Now;
            if (now - lastEmitTime >= interval)
            {
                yield return item;
                lastEmitTime = now;
            }
        }
    }
}

public class SensorSource
{
    private static readonly Random _random = new();

    public async IAsyncEnumerable<SensorData> GetSensorStreamAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 1; i <= 20; i++)
        {
            // Generate random value between -1.0 and 100.0
            double val = (_random.NextDouble() * 101.0) - 1.0;
            
            yield return new SensorData(i, val, DateTime.Now);

            // Rapid generation to demonstrate throttling
            await Task.Delay(100, ct); 
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        var source = new SensorSource();

        Console.WriteLine("Starting Async Pipeline...\n");

        // Pipeline Construction: Source -> Filter -> Throttle -> Transform
        var pipeline = source.GetSensorStreamAsync(cts.Token)
                             .FilterValidAsync(cts.Token)
                             .Throttle(TimeSpan.FromSeconds(1), cts.Token) // Limit to 1 item per second
                             .TransformToReportAsync(cts.Token);

        await foreach (var reportLine in pipeline)
        {
            Console.WriteLine(reportLine);
        }
        
        Console.WriteLine("Pipeline completed.");
    }
}
