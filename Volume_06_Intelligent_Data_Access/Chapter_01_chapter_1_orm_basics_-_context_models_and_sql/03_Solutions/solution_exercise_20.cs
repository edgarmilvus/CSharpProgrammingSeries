
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

// Source File: solution_exercise_20.cs
// Description: Solution for Exercise 20
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// 1. Metrics Collector
public class VectorMetrics
{
    private static readonly List<double> _latencies = new();
    private static int _queryCount = 0;
    private static int _errorCount = 0;

    public void RecordQuery(double latencyMs, bool success)
    {
        lock (_latencies)
        {
            _latencies.Add(latencyMs);
            _queryCount++;
            if (!success) _errorCount++;
        }
    }

    // 2. Anomaly Detection (Simple Z-Score)
    public bool IsAnomaly()
    {
        lock (_latencies)
        {
            if (_latencies.Count < 10) return false; // Need baseline

            var avg = _latencies.Average();
            var stdDev = Math.Sqrt(_latencies.Average(v => Math.Pow(v - avg, 2)));

            // If the last query was > 3 standard deviations from mean
            var last = _latencies.Last();
            return (last - avg) > (3 * stdDev);
        }
    }

    public (double p50, double p95, double p99) GetPercentiles()
    {
        lock (_latencies)
        {
            var sorted = _latencies.OrderBy(x => x).ToList();
            return (
                sorted[(int)(sorted.Count * 0.50)],
                sorted[(int)(sorted.Count * 0.95)],
                sorted[(int)(sorted.Count * 0.99)]
            );
        }
    }
}

// 3. Monitored Query Executor
public class MonitoredVectorSearch
{
    private readonly VectorMetrics _metrics;

    public async Task<List<Document>> SearchAsync(float[] vector)
    {
        var sw = Stopwatch.StartNew();
        bool success = false;
        try
        {
            // Execute actual search
            await Task.Delay(50); // Simulate work
            success = true;
            return new List<Document>();
        }
        catch
        {
            success = false;
            throw;
        }
        finally
        {
            sw.Stop();
            _metrics.RecordQuery(sw.ElapsedMilliseconds, success);

            // 4. Alerting
            if (_metrics.IsAnomaly())
            {
                await SendAlert("Performance Anomaly Detected!");
            }

            var (p50, p95, p99) = _metrics.GetPercentiles();
            Console.WriteLine($"Metrics: P50={p50}ms, P95={p95}ms, P99={p99}ms");
        }
    }

    private async Task SendAlert(string message)
    {
        // Integrate with PagerDuty, Slack, etc.
        Console.WriteLine($"ALERT: {message}");
        await Task.CompletedTask;
    }
}
