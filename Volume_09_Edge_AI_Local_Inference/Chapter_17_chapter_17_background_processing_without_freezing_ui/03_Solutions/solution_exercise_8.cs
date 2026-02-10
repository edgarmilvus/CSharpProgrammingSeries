
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

// Source File: solution_exercise_8.cs
// Description: Solution for Exercise 8
// ==========================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class Benchmark
{
    // Simulate heavy synchronous work (Freezes UI)
    public void RunSync(int msDelay)
    {
        Thread.Sleep(msDelay); // Blocks the calling thread
    }

    // Simulate heavy async work (Non-blocking)
    public async Task RunAsync(int msDelay)
    {
        await Task.Run(() => Thread.Sleep(msDelay)); // Offloads to thread pool
    }

    // Simulate a quick UI update check
    public void SimulateUIWork()
    {
        Stopwatch sw = Stopwatch.StartNew();
        // Minimal work to simulate checking for input or rendering a frame
        Thread.SpinWait(1000); 
        sw.Stop();
        Console.WriteLine($"    [UI Responsiveness Check: {sw.ElapsedTicks} ticks]");
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var benchmark = new Benchmark();
        int workDuration = 2000; // 2 seconds

        Console.WriteLine("--- SYNC EXECUTION (UI Freezes) ---");
        Stopwatch syncTotal = Stopwatch.StartNew();
        
        // 1. Start inference (Blocks)
        benchmark.RunSync(workDuration); 
        
        // 2. Try to update UI (Will only run after 2 seconds)
        benchmark.SimulateUIWork();
        
        syncTotal.Stop();
        Console.WriteLine($"Total Sync Time: {syncTotal.ElapsedMilliseconds}ms");
        Console.WriteLine("Note: UI check ran AFTER inference finished (High Latency).\n");

        Console.WriteLine("--- ASYNC EXECUTION (UI Responsive) ---");
        Stopwatch asyncTotal = Stopwatch.StartNew();

        // 1. Start inference (Does not block)
        Task inferenceTask = benchmark.RunAsync(workDuration);

        // 2. Immediately check UI responsiveness (Runs while inference is in background)
        benchmark.SimulateUIWork();

        // 3. Wait for inference to finish
        await inferenceTask;
        
        asyncTotal.Stop();
        Console.WriteLine($"Total Async Time: {asyncTotal.ElapsedMilliseconds}ms");
        Console.WriteLine("Note: UI check ran DURING inference (Low Latency).");
    }
}
