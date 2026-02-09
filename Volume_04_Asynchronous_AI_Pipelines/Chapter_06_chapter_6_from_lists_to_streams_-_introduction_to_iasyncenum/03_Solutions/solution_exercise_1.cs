
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

// 1. Define the Document record
public record Document(string Id, string Content);

// 2. Simulated Database Class
public class SimulatedDatabase
{
    // Approach 1: Eager Loading
    // Returns a Task that completes only after ALL documents are fetched and stored in memory.
    public async Task<List<Document>> GetAllDocumentsEagerAsync(int count)
    {
        var documents = new List<Document>(capacity: count); // Pre-allocate memory
        
        for (int i = 0; i < count; i++)
        {
            // Simulate network latency per document
            await Task.Delay(100); 
            
            documents.Add(new Document($"ID_{i}", $"Content_{i}"));
        }
        
        return documents;
    }

    // Approach 2: Lazy Streaming
    // Returns an iterator that yields documents one by one.
    public async IAsyncEnumerable<Document> GetAllDocumentsStreamAsync(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Simulate network latency per document
            await Task.Delay(100);
            
            // Yield the document immediately after creation
            yield return new Document($"ID_{i}", $"Content_{i}");
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        const int documentCount = 100; // Using 100 for quick testing, imagine 10,000
        var db = new SimulatedDatabase();
        var stopwatch = new Stopwatch();

        Console.WriteLine($"--- Fetching {documentCount} Documents ---");

        // 1. Measure Eager Loading
        Console.WriteLine("\n[Approach 1: Eager Loading (List)]");
        stopwatch.Restart();
        
        // The timer starts here. The method will not return until ALL 100 items 
        // (taking 100 * 100ms = 10 seconds) are fetched.
        Task<List<Document>> eagerTask = db.GetAllDocumentsEagerAsync(documentCount);
        
        // We cannot access the first item until the Task completes.
        // This simulates the "Time to First Item" being equal to "Total Time".
        List<Document> eagerDocs = await eagerTask;
        
        stopwatch.Stop();
        Console.WriteLine($"Time to retrieve first item: {stopwatch.ElapsedMilliseconds} ms (Total fetch time)");
        Console.WriteLine($"Memory usage: High (Holding {eagerDocs.Count} documents)");

        // 2. Measure Lazy Streaming
        Console.WriteLine("\n[Approach 2: Lazy Streaming (IAsyncEnumerable)]");
        stopwatch.Restart();
        
        // The method returns immediately (the iterator is created), but no data is fetched yet.
        IAsyncEnumerable<Document> stream = db.GetAllDocumentsStreamAsync(documentCount);
        
        // We await the first item using 'await foreach' (or GetAsyncEnumerator)
        await foreach (var doc in stream)
        {
            stopwatch.Stop();
            Console.WriteLine($"Time to retrieve first item: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"First item received: {doc.Id}");
            
            // In a real app, we might process this item immediately while the rest load in background.
            break; // Stop after the first item for measurement
        }
    }
}
