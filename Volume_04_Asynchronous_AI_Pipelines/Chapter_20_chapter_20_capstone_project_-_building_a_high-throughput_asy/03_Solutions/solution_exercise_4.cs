
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

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Linq;

namespace RefactoredPipeline
{
    public record RawData(string Url);
    public record ParsedData(string Content);

    public static class PipelineStages
    {
        // Stage 1: Fetch (I/O Bound)
        public static async Task FetchStageAsync(ChannelWriter<RawData> output)
        {
            var urls = Enumerable.Range(0, 10).Select(i => $"url_{i}");
            foreach (var url in urls)
            {
                await Task.Delay(500); // Simulate blocking I/O
                await output.WriteAsync(new RawData(url));
                Console.WriteLine($"Fetched: {url}");
            }
            output.Complete();
        }

        // Stage 2: Parse (CPU Bound - Simulated)
        public static async Task ParseStageAsync(ChannelReader<RawData> input, ChannelWriter<ParsedData> output, int workerCount)
        {
            var tasks = Enumerable.Range(0, workerCount).Select(async _ =>
            {
                await foreach (var data in input.ReadAllAsync())
                {
                    await Task.Delay(200); // Simulate CPU work
                    await output.WriteAsync(new ParsedData(data.Content.ToUpper()));
                    Console.WriteLine($"Parsed: {data.Content}");
                }
            });
            await Task.WhenAll(tasks);
            output.Complete();
        }

        // Stage 3: Save (I/O Bound)
        public static async Task SaveStageAsync(ChannelReader<ParsedData> input, int workerCount)
        {
            var tasks = Enumerable.Range(0, workerCount).Select(async _ =>
            {
                await foreach (var data in input.ReadAllAsync())
                {
                    await Task.Delay(100); // Simulate DB write
                    Console.WriteLine($"Saved: {data.Content}");
                }
            });
            await Task.WhenAll(tasks);
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Create Queues (Channels)
            var fetchQueue = Channel.CreateUnbounded<RawData>();
            var parseQueue = Channel.CreateUnbounded<ParsedData>();

            // Define Worker Counts
            int parseWorkers = 2; // CPU bound workers
            int saveWorkers = 3;  // I/O bound workers

            // Pipeline Orchestration
            var fetchTask = PipelineStages.FetchStageAsync(fetchQueue.Writer);
            var parseTask = PipelineStages.ParseStageAsync(fetchQueue.Reader, parseQueue.Writer, parseWorkers);
            var saveTask = PipelineStages.SaveStageAsync(parseQueue.Reader, saveWorkers);

            await Task.WhenAll(fetchTask, parseTask, saveTask);

            stopwatch.Stop();
            Console.WriteLine($"Total Pipeline Time: {stopwatch.Elapsed.TotalSeconds}s");
        }
    }
}
