
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
using System.Diagnostics;
using System.Threading.Tasks;

public class AsyncVsSyncDemo
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting the demonstration...\n");

        // 1. Run the synchronous version (Blocking)
        Console.WriteLine("--- 1. Synchronous Execution (Blocking) ---");
        Stopwatch syncWatch = Stopwatch.StartNew();
        await RunSynchronousWorkflow();
        syncWatch.Stop();
        Console.WriteLine($"Synchronous workflow completed in {syncWatch.ElapsedMilliseconds}ms\n");

        // 2. Run the asynchronous version (Non-blocking)
        Console.WriteLine("--- 2. Asynchronous Execution (Non-blocking) ---");
        Stopwatch asyncWatch = Stopwatch.StartNew();
        await RunAsynchronousWorkflow();
        asyncWatch.Stop();
        Console.WriteLine($"Asynchronous workflow completed in {asyncWatch.ElapsedMilliseconds}ms");
    }

    // Simulates a blocking I/O operation (e.g., database query without async)
    private static void SimulateBlockingWork(string taskName, int delayMs)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting {taskName} (Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId})");
        // Thread.Sleep blocks the current thread, preventing it from doing anything else.
        System.Threading.Thread.Sleep(delayMs); 
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Finished {taskName}");
    }

    // Simulates a non-blocking I/O operation (e.g., database query with async)
    private static async Task SimulateAsyncWork(string taskName, int delayMs)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting {taskName} (Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId})");
        // Task.Delay yields control back to the caller, freeing the thread to do other work.
        await Task.Delay(delayMs); 
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Finished {taskName}");
    }

    private static async Task RunSynchronousWorkflow()
    {
        // These run one after another. Total time = Sum of delays.
        SimulateBlockingWork("Database Query", 1000);
        SimulateBlockingWork("Image Processing", 1000);
        SimulateBlockingWork("File Upload", 1000);
    }

    private static async Task RunAsynchronousWorkflow()
    {
        // These run concurrently. Total time ≈ Max(delays).
        Task task1 = SimulateAsyncWork("Database Query", 1000);
        Task task2 = SimulateAsyncWork("Image Processing", 1000);
        Task task3 = SimulateAsyncWork("File Upload", 1000);

        // Wait for all concurrent tasks to finish
        await Task.WhenAll(task1, task2, task3);
    }
}
