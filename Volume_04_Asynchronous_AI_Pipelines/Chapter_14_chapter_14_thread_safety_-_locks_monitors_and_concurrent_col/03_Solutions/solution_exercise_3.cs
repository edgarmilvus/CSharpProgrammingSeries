
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public record Result(string DocumentId, string Content);

// Version A: Coarse Lock
public class CoarseBatchProcessor
{
    private readonly List<Result> _results = new List<Result>();
    private readonly object _lock = new object();

    public async Task ProcessDocuments(IEnumerable<string> documents, Func<string, Task<string>> llmProcessor)
    {
        var tasks = new List<Task>();
        foreach (var doc in documents)
        {
            tasks.Add(Task.Run(async () =>
            {
                var content = await llmProcessor(doc);
                
                // COARSE LOCK: Locking the entire list for every write
                lock (_lock)
                {
                    _results.Add(new Result(doc, content));
                }
            }));
        }
        await Task.WhenAll(tasks);
    }
}

// Version B: Fine Lock (Concurrent Collection)
public class FineBatchProcessor
{
    private readonly ConcurrentDictionary<string, Result> _results = new ConcurrentDictionary<string, Result>();

    public async Task ProcessDocuments(IEnumerable<string> documents, Func<string, Task<string>> llmProcessor)
    {
        var tasks = new List<Task>();
        foreach (var doc in documents)
        {
            tasks.Add(Task.Run(async () =>
            {
                var content = await llmProcessor(doc);
                
                // FINE LOCK: The dictionary handles internal locking per bucket
                // No need for an explicit lock object here
                _results[doc] = new Result(doc, content);
            }));
        }
        await Task.WhenAll(tasks);
    }
}

public class BatchProcessorBenchmark
{
    public static async Task RunBenchmark()
    {
        int docCount = 10000;
        var docs = Enumerable.Range(0, docCount).Select(i => $"Doc_{i}");
        
        // Mock LLM Processor
        Func<string, Task<string>> mockProcessor = async (doc) => 
        {
            await Task.Delay(1); // Simulate minimal work
            return "Processed";
        };

        // Benchmark Coarse Lock
        var coarse = new CoarseBatchProcessor();
        var sw = Stopwatch.StartNew();
        await coarse.ProcessDocuments(docs, mockProcessor);
        sw.Stop();
        Console.WriteLine($"Coarse Lock Time: {sw.ElapsedMilliseconds}ms");

        // Benchmark Fine Lock
        var fine = new FineBatchProcessor();
        sw.Restart();
        await fine.ProcessDocuments(docs, mockProcessor);
        sw.Stop();
        Console.WriteLine($"Fine Lock Time: {sw.ElapsedMilliseconds}ms");
    }
}
