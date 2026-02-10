
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class Program
{
    // Entry point of the application
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Async Document Ingestion Engine...");

        // 1. Define a list of document IDs to fetch (simulating a queue of work)
        var documentIds = new List<string>
        {
            "doc_001.txt", "doc_002.pdf", "doc_003.md", "doc_004.html"
        };

        // 2. Start a stopwatch to measure the performance gain of concurrency
        var stopwatch = Stopwatch.StartNew();

        // 3. Create a list of tasks. 
        //    We DO NOT await them immediately. We start them "in parallel" 
        //    by not awaiting inside the loop.
        var ingestionTasks = documentIds.Select(id => IngestDocumentAsync(id));

        // 4. Await all tasks to complete concurrently
        var results = await Task.WhenAll(ingestionTasks);

        stopwatch.Stop();

        // 5. Process results
        Console.WriteLine($"\nFinished ingesting {results.Length} documents in {stopwatch.ElapsedMilliseconds}ms.");
        
        foreach (var result in results)
        {
            Console.WriteLine($" - Processed: {result}");
        }
    }

    /// <summary>
    /// Simulates fetching a document from a remote source and parsing it.
    /// In a real scenario, this would involve HTTP requests (HttpClient) and file I/O (aiofiles equivalent).
    /// </summary>
    /// <param name="docId">The identifier of the document to fetch.</param>
    /// <returns>A string representing the processed content.</returns>
    private static async Task<string> IngestDocumentAsync(string docId)
    {
        Console.WriteLine($"[Start] Fetching {docId}...");

        // Simulate a network delay (e.g., waiting for an API response)
        // Random is used to simulate varying network latency
        var randomDelay = new Random().Next(500, 1500); 
        await Task.Delay(randomDelay);

        // Simulate parsing/processing the document
        var processedContent = $"Content of {docId} (processed in {randomDelay}ms)";

        Console.WriteLine($"[Done]  Finished {docId}.");

        return processedContent;
    }
}
