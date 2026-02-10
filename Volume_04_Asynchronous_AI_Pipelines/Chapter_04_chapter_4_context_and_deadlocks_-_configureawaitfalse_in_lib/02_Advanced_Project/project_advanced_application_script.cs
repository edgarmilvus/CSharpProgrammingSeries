
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDeadlockPrevention
{
    /// <summary>
    /// Simulates a real-world library component: a reusable Data Fetcher.
    /// This library is designed to be consumed by various application types (Console, UI, Web).
    /// It performs asynchronous I/O operations.
    /// </summary>
    public class LibraryDataFetcher
    {
        /// <summary>
        /// Fetches data asynchronously from a remote source.
        /// This method demonstrates the CRITICAL use of ConfigureAwait(false).
        /// </summary>
        public async Task<string> FetchDataAsync(string sourceUrl)
        {
            // Simulate network latency
            Console.WriteLine($"[Library] Starting fetch from: {sourceUrl}");
            
            // CRITICAL: ConfigureAwait(false) breaks the context.
            // Without this, if the caller is a UI thread or a specific SynchronizationContext
            // (like ASP.NET Core pre-3.0), the continuation (code after await) would try to 
            // resume on that original context. If that context is blocked waiting for this task,
            // a DEADLOCK occurs.
            await Task.Delay(1000).ConfigureAwait(false);
            
            Console.WriteLine($"[Library] Data received from: {sourceUrl}");
            return $"Data from {sourceUrl} at {DateTime.Now.Ticks}";
        }
    }

    /// <summary>
    /// Simulates a Console Application consuming the library.
    /// Console apps typically do not have a SynchronizationContext, but using ConfigureAwait(false)
    /// is still best practice for library code to ensure performance and safety regardless of consumer.
    /// </summary>
    public class ConsoleApplication
    {
        public async Task RunAsync()
        {
            Console.WriteLine("--- Console App Start ---");
            var fetcher = new LibraryDataFetcher();
            
            // Standard async/await usage.
            // In a Console App, the context is usually null, so ConfigureAwait(false) doesn't change behavior significantly
            // here, but it ensures the library is robust if used elsewhere.
            string data = await fetcher.FetchDataAsync("https://api.example.com/data");
            
            Console.WriteLine($"[Console App] Processed: {data}");
            Console.WriteLine("--- Console App End ---");
        }
    }

    /// <summary>
    /// Simulates a UI or Legacy ASP.NET Context where SynchronizationContext matters.
    /// This class mimics a single-threaded environment (like WinForms or WPF UI thread).
    /// </summary>
    public class SynchronizationContextSimulator
    {
        private readonly SingleThreadSynchronizationContext _context;

        public SynchronizationContextSimulator()
        {
            // Create a single-threaded context to simulate a UI thread
            _context = new SingleThreadSynchronizationContext();
        }

        public void RunWithDeadlockRisk()
        {
            Console.WriteLine("\n--- Simulating UI Thread (Deadlock Risk Scenario) ---");
            
            // We run the logic within our specific synchronization context
            SynchronizationContext.SetSynchronizationContext(_context);
            
            // NOTE: We are using .Result here to simulate a synchronous call from a UI event handler
            // or legacy code that blocks the UI thread waiting for an async result.
            // This is where deadlocks typically happen.
            try 
            {
                var fetcher = new LibraryDataFetcher();
                
                // DEADLOCK TRIGGER:
                // 1. FetchDataAsync starts.
                // 2. Task.Delay yields.
                // 3. Execution returns to the SynchronizationContext (UI thread).
                // 4. The UI thread blocks on .Result, waiting for the task to finish.
                // 5. Task.Delay finishes.
                // 6. The continuation tries to resume on the original SynchronizationContext (UI thread).
                // 7. BUT the UI thread is blocked by .Result waiting for the task.
                // 8. DEADLOCK.
                
                // NOTE: If we had used ConfigureAwait(false) in the Library, this would NOT deadlock.
                // Because the continuation would run on the thread pool, releasing the UI thread.
                // However, since this is a demonstration of the PROBLEM, we assume the Library 
                // (or the awaiter) did NOT use ConfigureAwait(false).
                
                // To simulate the deadlock safely in this console demo, we actually have to run it 
                // on the thread pool manually, otherwise the main thread blocks indefinitely.
                // In a real WinForms app, this would just freeze the app.
                
                Task<string> task = fetcher.FetchDataAsync("https://api.example.com/ui-data");
                
                // We simulate the blocking wait
                string data = task.Result; 
                
                Console.WriteLine($"[UI Thread] Processed: {data}");
            }
            catch (AggregateException ex)
            {
                // In a real deadlock, the app hangs. Here, we might get a timeout or cancellation exception
                // depending on how we simulate it. 
                Console.WriteLine($"[UI Thread] Error: {ex.InnerException?.Message}");
            }
        }

        public async Task RunSafeAsync()
        {
            Console.WriteLine("\n--- Simulating UI Thread (Safe with ConfigureAwait(false)) ---");
            SynchronizationContext.SetSynchronizationContext(_context);

            var fetcher = new LibraryDataFetcher();
            
            // Because the Library uses ConfigureAwait(false), we can safely await here
            // even in a UI context, without blocking the UI thread logic flow.
            // However, strictly speaking, in a UI app you usually DO want ConfigureAwait(true)
            // to update the UI after the await. 
            // The key is: Library code should be agnostic. 
            // If we were to block the UI thread (e.g. using .Result) on a method that uses ConfigureAwait(false),
            // it would still deadlock because the continuation runs on a thread pool thread, 
            // but the UI thread is blocked waiting. The UI wouldn't update, but the app wouldn't freeze 
            // entirely if there are other operations. 
            // Actually, the specific deadlock resolved by ConfigureAwait(false) is when the 
            // CONTEXT is required to resume, but the context is blocked waiting for the task.
            
            string data = await fetcher.FetchDataAsync("https://api.example.com/ui-data-safe");
            Console.WriteLine($"[UI Thread] Processed: {data}");
        }
    }

    /// <summary>
    /// A custom SingleThreadSynchronizationContext to simulate UI threads.
    /// </summary>
    internal sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public void RunOnCurrentThread()
        {
            while (_queue.TryTake(out var workItem, Timeout.Infinite))
            {
                workItem.Key(workItem.Value);
            }
        }

        public void Complete() => _queue.CompleteAdding();
    }

    // Helper class for the simulation
    internal static class BlockingCollection<T>
    {
        private static readonly System.Collections.Concurrent.BlockingCollection<T> _inner = new System.Collections.Concurrent.BlockingCollection<T>();
        public static void Add(T item) => _inner.Add(item);
        public static bool TryTake(out T item, int timeout) => _inner.TryTake(out item, timeout);
        public static void CompleteAdding() => _inner.CompleteAdding();
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Run the standard Console Application
            var consoleApp = new ConsoleApplication();
            await consoleApp.RunAsync();

            // 2. Demonstrate the Deadlock Scenario (Simulated)
            // Note: In a real UI app, this freezes the main thread. 
            // Here we simulate the logic to explain the flow.
            var uiSimulator = new SynchronizationContextSimulator();
            uiSimulator.RunWithDeadlockRisk();

            // 3. Demonstrate the Safe Approach
            // Even in a UI context, if the library uses ConfigureAwait(false),
            // awaiting the task is safe from deadlocks caused by the library's continuations.
            await uiSimulator.RunSafeAsync();
            
            Console.WriteLine("\nApplication Finished.");
        }
    }
}
