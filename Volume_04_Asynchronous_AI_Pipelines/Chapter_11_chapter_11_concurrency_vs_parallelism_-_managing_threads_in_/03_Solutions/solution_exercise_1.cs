
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class WebScraper
{
    // Simulates a network call with 500ms latency
    public async Task<string> DownloadUrlAsync(string url)
    {
        // Simulate network latency (I/O-bound operation)
        await Task.Delay(500);
        return $"Content from {url}";
    }

    // Sequential download: awaits each task before starting the next
    public async Task<List<string>> DownloadAllSequentiallyAsync(List<string> urls)
    {
        var results = new List<string>();
        foreach (var url in urls)
        {
            var content = await DownloadUrlAsync(url);
            results.Add(content);
        }
        return results;
    }

    // Concurrent download: starts all tasks and awaits them together
    public async Task<List<string>> DownloadAllConcurrentlyAsync(List<string> urls)
    {
        var tasks = new List<Task<string>>();
        foreach (var url in urls)
        {
            // Start the task without awaiting it immediately
            tasks.Add(DownloadUrlAsync(url));
        }

        // Await all tasks to complete
        var results = await Task.WhenAll(tasks);
        return new List<string>(results);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var scraper = new WebScraper();
        var urls = new List<string>
        {
            "https://example.com/1",
            "https://example.com/2",
            "https://example.com/3",
            "https://example.com/4",
            "https://example.com/5"
        };

        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine("Starting Sequential Download...");
        await scraper.DownloadAllSequentiallyAsync(urls);
        stopwatch.Stop();
        Console.WriteLine($"Sequential Time: {stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        Console.WriteLine("\nStarting Concurrent Download...");
        await scraper.DownloadAllConcurrentlyAsync(urls);
        stopwatch.Stop();
        Console.WriteLine($"Concurrent Time: {stopwatch.ElapsedMilliseconds}ms");
    }
}
