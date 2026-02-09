
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class LegacyPipeline
{
    // Simulates a blocking network call
    public List<string> FetchImageData()
    {
        Thread.Sleep(1000); // Blocking I/O
        return new List<string> { "img1", "img2", "img3" };
    }

    // Simulates a CPU-bound image processing task
    public string ProcessImage(string imageId)
    {
        Thread.Sleep(500); // CPU work
        return $"Processed_{imageId}";
    }

    // Simulates writing to disk
    public void LogResult(string result)
    {
        Thread.Sleep(100); // Disk I/O
        Console.WriteLine($"Logged: {result}");
    }

    public void ProcessImagePipeline()
    {
        var images = FetchImageData();
        foreach (var img in images)
        {
            var result = ProcessImage(img);
            LogResult(result);
        }
    }
}

// Refactored Asynchronous Pipeline
public class AsyncPipeline
{
    // 1. Async version of FetchImageData
    public async Task<List<string>> FetchImageDataAsync(CancellationToken ct)
    {
        // Simulate network latency asynchronously
        await Task.Delay(1000, ct); 
        return new List<string> { "img1", "img2", "img3" };
    }

    // 2. Async version of ProcessImage (CPU-bound work offloaded)
    public async Task<string> ProcessImageAsync(string imageId, CancellationToken ct)
    {
        // Offload CPU work to prevent blocking the event loop
        return await Task.Run(() =>
        {
            Thread.Sleep(500); // Simulate CPU work
            return $"Processed_{imageId}";
        }, ct);
    }

    // 3. Streaming logs using IAsyncEnumerable
    public async IAsyncEnumerable<string> ProcessImagePipelineAsync(CancellationToken ct)
    {
        // Fetch images asynchronously
        var images = await FetchImageDataAsync(ct);

        // Create tasks for parallel processing
        var processingTasks = images.Select(img => ProcessImageAsync(img, ct)).ToList();

        // Process images in parallel using Task.WhenAll
        var results = await Task.WhenAll(processingTasks);

        // Stream results back to the caller
        foreach (var result in results)
        {
            // Simulate Disk I/O asynchronously
            await Task.Delay(100, ct); 
            yield return $"Logged: {result}";
        }
    }
}

public class Program
{
    // 4. Main entry point using async Main
    public static async Task Main(string[] args)
    {
        var pipeline = new AsyncPipeline();
        var cts = new CancellationTokenSource();

        Console.WriteLine("Starting Async Pipeline...");

        // 5. Consuming the async stream
        await foreach (var logEntry in pipeline.ProcessImagePipelineAsync(cts.Token))
        {
            Console.WriteLine(logEntry);
        }
        
        Console.WriteLine("Pipeline Complete.");
    }
}
