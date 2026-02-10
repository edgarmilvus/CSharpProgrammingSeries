
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStateMachinesExercises
{
    public class StateTracker
    {
        private readonly string _operationName;
        private readonly Stopwatch _sw;

        public StateTracker(string operationName)
        {
            _operationName = operationName;
            _sw = Stopwatch.StartNew();
        }

        public void LogState(string state)
        {
            // Format: [Elapsed ms] [Thread ID] [Operation] - State: [State]
            Console.WriteLine($"[{_sw.ElapsedMilliseconds,5}] [Thread {Thread.CurrentThread.ManagedThreadId,2}] {_operationName,-20} - State: {state}");
        }
    }

    public class Exercise1
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("=== Exercise 1: Visualizing State Transition Graph ===");
            Console.WriteLine("Note: Observe thread IDs and timestamps.\n");

            var tracker = new StateTracker("SimulateWorkAsync");

            // Initial state is Pending (before the method body executes)
            tracker.LogState("Pending -> Running (Entry)");

            await SimulateWorkAsync(tracker);

            tracker.LogState("Completed");
        }

        private static async Task SimulateWorkAsync(StateTracker tracker)
        {
            // 1. First block of execution
            tracker.LogState("Running");
            Console.WriteLine("   (Doing initial work...)");
            
            // 2. First Await Point
            tracker.LogState("Suspended (Awaiting Task.Delay)");
            await Task.Delay(200); // Non-blocking delay
            
            // 3. Resumption after first await
            // Note: Thread ID might change here depending on the SynchronizationContext
            tracker.LogState("Resumed (After Task.Delay)");
            Console.WriteLine("   (Doing work after first delay...)");

            // 4. Second Await Point
            tracker.LogState("Suspended (Awaiting Task.Delay)");
            await Task.Delay(200);

            // 5. Resumption after second await
            tracker.LogState("Resumed (After Task.Delay)");
            Console.WriteLine("   (Finishing work...)");
        }
    }
}
