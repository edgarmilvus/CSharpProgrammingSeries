
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStateMachinesExercises
{
    public class Exercise3
    {
        public static void Run()
        {
            Console.WriteLine("=== Exercise 3: Deadlocks & SynchronizationContext ===\n");

            // Scenario 1: Simulating a UI-like context (Single-threaded context)
            Console.WriteLine("Scenario 1: Single-threaded SynchronizationContext (Simulated UI)");
            Console.WriteLine("Attempting to block on async method...");
            
            // Note: In a real WPF/WinForms app, this would freeze immediately.
            // We simulate the context by using a nested message pump or simply observing the logic.
            // For this console demo, we will show the code that *would* deadlock.
            
            try
            {
                // This mimics calling GetDataSync() from the main thread in a UI app.
                // 1. Main thread calls GetDataSync.
                // 2. GetDataSync calls GetDataAsync().Result.
                // 3. GetDataAsync starts, awaits Task.Delay.
                // 4. The await captures the SynchronizationContext (simulated here).
                // 5. The main thread blocks waiting for .Result.
                // 6. Task.Delay completes, but wants to resume on the captured context (Main Thread).
                // 7. Main Thread is blocked. Resume cannot happen. DEADLOCK.
                
                string result = GetDataSync();
                Console.WriteLine($"Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught Exception: {ex.Message}");
            }

            Console.WriteLine("\nScenario 2: Using ConfigureAwait(false) to prevent deadlock");
            string resultSafe = GetDataSyncSafe();
            Console.WriteLine($"Result: {resultSafe}");
        }

        // Synchronous wrapper that blocks
        private static string GetDataSync()
        {
            // WARNING: This pattern is dangerous in UI apps or libraries without context.
            // We are forcing the async method to complete synchronously on the current thread.
            return GetDataAsync().Result; 
        }

        private static string GetDataSyncSafe()
        {
            // ConfigureAwait(false) tells the state machine:
            // "Do not resume on the captured SynchronizationContext. Resume on the ThreadPool."
            // Since the ThreadPool is free, the task can complete even if the main thread is blocked.
            return GetDataAsync(true).Result;
        }

        // Async method with an await point
        private static async Task<string> GetDataAsync(bool useConfigureAwait = false)
        {
            Console.WriteLine($"  [GetDataAsync] Started on Thread {Thread.CurrentThread.ManagedThreadId}");
            
            var delayTask = Task.Delay(100);
            
            if (useConfigureAwait)
            {
                await delayTask.ConfigureAwait(false);
            }
            else
            {
                await delayTask;
            }

            Console.WriteLine($"  [GetDataAsync] Resumed on Thread {Thread.CurrentThread.ManagedThreadId}");
            return "Data retrieved";
        }
    }
}
