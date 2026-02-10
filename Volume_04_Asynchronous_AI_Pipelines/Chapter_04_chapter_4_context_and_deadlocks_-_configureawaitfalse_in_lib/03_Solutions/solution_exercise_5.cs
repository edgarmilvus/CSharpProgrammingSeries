
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

public class SmartLibrary
{
    public async Task ProcessDataWithProgressAsync(IProgress<int> progress)
    {
        // 1. Capture the current context (UI thread context)
        var context = SynchronizationContext.Current;
        
        // 2. Offload heavy work to ThreadPool
        await Task.Run(async () =>
        {
            for (int i = 0; i <= 100; i += 10)
            {
                // Simulate work
                await Task.Delay(100);
                
                // 3. Report progress
                // We cannot just call progress.Report(i) here because IProgress<T>
                // captures the context at the time of creation (usually UI thread).
                // However, if we are inside Task.Run, we are on ThreadPool.
                // IProgress implementation (like WinForms Progress<T)) uses Post/Send 
                // to marshal back to the context automatically.
                
                // If we were doing manual context dispatching:
                if (context != null)
                {
                    context.Post(_ => 
                    {
                        // This runs on the original context (UI Thread)
                        // Safe to update UI here
                        Console.WriteLine($"Progress: {i}%");
                    }, null);
                }
                else
                {
                    // No context (e.g. Console App / ASP.NET Core)
                    // Report directly
                    Console.WriteLine($"Progress: {i}%");
                }
            }
        }).ConfigureAwait(false); // Important: Don't capture context for the heavy loop
    }
}

// Mock IProgress implementation to show how it handles context
public class Progress<T> : IProgress<T>
{
    private readonly Action<T> _handler;
    private readonly SynchronizationContext _context;

    public Progress(Action<T> handler)
    {
        _handler = handler;
        _context = SynchronizationContext.Current;
    }

    public void Report(T value)
    {
        if (_context != null)
        {
            // Marshal back to the captured context
            _context.Post(_ => _handler(value), null);
        }
        else
        {
            _handler(value);
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("--- Exercise 5: Hybrid Pattern ---");
        
        // Simulate UI Context
        var uiContext = new SingleThreadSynchronizationContext();
        
        // Run the test on the simulated UI thread
        var t = new Thread(() => uiContext.Run());
        t.Start();

        uiContext.Post(async _ => 
        {
            var lib = new SmartLibrary();
            
            // Create progress reporter (captures UI context)
            var progress = new Progress<int>(p => 
            {
                // This lambda runs on the UI Thread (Simulated)
                Console.WriteLine($"[UI Thread] Update UI: {p}%");
            });

            Console.WriteLine("Starting processing...");
            
            // Call the library method
            await lib.ProcessDataWithProgressAsync(progress);
            
            Console.WriteLine("Processing Complete.");
            
            // Stop the simulation
            Environment.Exit(0);
        }, null);
    }
}
