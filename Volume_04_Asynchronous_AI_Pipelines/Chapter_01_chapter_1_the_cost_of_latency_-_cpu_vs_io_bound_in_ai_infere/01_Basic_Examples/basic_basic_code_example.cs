
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    // Simulated external dependencies (e.g., Vector Database, External API, File System)
    // These represent the I/O-bound portion of an AI pipeline.
    public static async Task Main()
    {
        Console.WriteLine("--- Synchronous (Blocking) Execution ---");
        await RunSynchronousExample();

        Console.WriteLine("\n--- Asynchronous (Non-Blocking) Execution ---");
        await RunAsynchronousExample();
    }

    // 1. Synchronous Example: The "Bad" Way (Blocking the Thread)
    static async Task RunSynchronousExample()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        // Simulating a user request that requires fetching context from a vector DB
        // and generating a response from an LLM.
        Console.WriteLine("User Request 1: 'What is the capital of France?'");
        string result1 = FetchContextAndGenerateResponse_Sync("France");
        Console.WriteLine($"Response 1: {result1} (Time: {stopwatch.ElapsedMilliseconds}ms)");

        Console.WriteLine("User Request 2: 'What is 2+2?'");
        string result2 = FetchContextAndGenerateResponse_Sync("Math");
        Console.WriteLine($"Response 2: {result2} (Time: {stopwatch.ElapsedMilliseconds}ms)");

        // In a real web server, blocking like this means the thread is stuck here
        // and cannot handle other incoming requests.
        stopwatch.Stop();
    }

    // 2. Asynchronous Example: The "Good" Way (Non-Blocking)
    static async Task RunAsynchronousExample()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        Console.WriteLine("User Request 3: 'Explain Quantum Computing'");
        Console.WriteLine("User Request 4: 'Write a Python Hello World'");

        // Kick off tasks concurrently. 
        // We do NOT await immediately. We store the "promise" (Task) in a variable.
        Task<string> task1 = FetchContextAndGenerateResponse_Async("Quantum");
        Task<string> task2 = FetchContextAndGenerateResponse_Async("Python");

        // Now we await them. This allows the thread to do other work while waiting.
        // If task1 finishes first, we process it immediately.
        string result1 = await task1;
        Console.WriteLine($"Response 3: {result1} (Time: {stopwatch.ElapsedMilliseconds}ms)");

        string result2 = await task2;
        Console.WriteLine($"Response 4: {result2} (Time: {stopwatch.ElapsedMilliseconds}ms)");
        
        stopwatch.Stop();
    }

    // --- SIMULATION HELPERS ---

    // Synchronous I/O Simulation (Blocking)
    // This mimics a database call that halts the thread execution.
    static string FetchContextAndGenerateResponse_Sync(string query)
    {
        // Simulate Network Latency (I/O Bound)
        // Thread sleeps, consuming zero CPU but blocking the thread for 2000ms.
        Thread.Sleep(2000); 
        
        // Simulate Model Inference (CPU Bound)
        // Simulate heavy computation.
        Thread.Sleep(500); 

        return $"Processed: {query}";
    }

    // Asynchronous I/O Simulation (Non-Blocking)
    // This mimics a modern async database driver or HTTP client.
    static async Task<string> FetchContextAndGenerateResponse_Async(string query)
    {
        // Simulate Network Latency (I/O Bound)
        // Task.Delay yields control back to the caller. The thread is free to handle other requests.
        await Task.Delay(2000);

        // Simulate Model Inference (CPU Bound)
        // Even though this is CPU work, keeping it on the thread is fine 
        // because the I/O part didn't block the thread.
        await Task.Delay(500);

        return $"Processed: {query}";
    }
}
