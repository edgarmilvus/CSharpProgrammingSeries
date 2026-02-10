
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

// Source File: solution_exercise_21.cs
// Description: Solution for Exercise 21
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// 1. Load Generator
public class VectorLoadTester
{
    private readonly VectorSearchService _service;
    private readonly int _concurrentUsers;

    public VectorLoadTester(VectorSearchService service, int concurrentUsers)
    {
        _service = service;
        _concurrentUsers = concurrentUsers;
    }

    public async Task<LoadTestResult> RunScenarioAsync()
    {
        var tasks = new List<Task>();
        var results = new List<long>();
        var errors = 0;
        var sw = Stopwatch.StartNew();

        // 2. Simulate Concurrent Users
        for (int i = 0; i < _concurrentUsers; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var querySw = Stopwatch.StartNew();
                    await _service.SearchAsync(new float[128]);
                    querySw.Stop();
                    lock (results) results.Add(querySw.ElapsedMilliseconds);
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }
            }));
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        return new LoadTestResult
        {
            TotalTime = sw.Elapsed,
            AverageLatency = results.Count > 0 ? results.Average() : 0,
            P99Latency = results.Count > 0 ? results.OrderBy(x => x).ElementAt((int)(results.Count * 0.99)) : 0,
            ErrorRate = (double)errors / _concurrentUsers
        };
    }
}

// 3. Chaos Engineering (Simulated)
public class ChaosMonkey
{
    private readonly Random _random = new();

    public void InjectFailure()
    {
        // Randomly fail or delay to test resilience
        if (_random.Next(0, 10) < 2) // 20% chance
        {
            throw new TimeoutException("Vector DB Unavailable");
        }

        if (_random.Next(0, 10) < 1) // 10% chance
        {
            Thread.Sleep(2000); // High latency
        }
    }
}

public class LoadTestResult
{
    public TimeSpan TotalTime { get; set; }
    public double AverageLatency { get; set; }
    public double P99Latency { get; set; }
    public double ErrorRate { get; set; }
}

// Mock
public class VectorSearchService 
{
    private readonly ChaosMonkey _chaos = new();
    public async Task SearchAsync(float[] v) 
    {
        _chaos.InjectFailure(); 
        await Task.Delay(10); 
    }
}
