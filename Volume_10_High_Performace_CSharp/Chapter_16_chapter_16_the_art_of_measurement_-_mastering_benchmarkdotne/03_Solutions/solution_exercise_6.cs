
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictions)]
[MemoryDiagnoser]
[Config(typeof(ParallelConfig))]
public class ParallelProcessingBenchmark
{
    private List<float[]> _tokenBatches;
    private const int BatchSize = 1000;

    private class ParallelConfig : ManualConfig
    {
        public ParallelConfig()
        {
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithId("Unrestricted"));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithId("Restricted").WithMaxDegreeOfParallelism(4));
            AddColumn(StatisticColumn.P95);
            AddColumn(StatisticColumn.P99);
            AddColumn(ThroughputColumn.Instance);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _tokenBatches = new List<float[]>(100);
        for (int i = 0; i < 100; i++)
        {
            float[] batch = new float[BatchSize];
            for (int j = 0; j < BatchSize; j++) batch[j] = (float)rng.NextDouble();
            _tokenBatches.Add(batch);
        }
    }

    // Simulate CPU-bound work
    private void ProcessBatch(float[] batch)
    {
        // Heavy computation to simulate token processing
        double sum = 0;
        for (int i = 0; i < batch.Length; i++)
        {
            sum += Math.Sqrt(batch[i]) * Math.Sin(batch[i]);
        }
        // Prevent dead code elimination
        if (sum < 0) throw new Exception();
    }

    // Simulate I/O-bound work
    private async Task ProcessBatchAsync(float[] batch)
    {
        await Task.Delay(1); // Simulate network/database call
        ProcessBatch(batch); // Still do some CPU work
    }

    [Benchmark]
    public void SynchronousParallel()
    {
        Parallel.For(0, _tokenBatches.Count, new ParallelOptions { MaxDegreeOfParallelism = -1 }, i =>
        {
            ProcessBatch(_tokenBatches[i]);
        });
    }

    [Benchmark]
    public async Task AsynchronousParallel()
    {
        var tasks = _tokenBatches.Select(batch => ProcessBatchAsync(batch));
        await Task.WhenAll(tasks);
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ParallelProcessingBenchmark>();
    }
}
