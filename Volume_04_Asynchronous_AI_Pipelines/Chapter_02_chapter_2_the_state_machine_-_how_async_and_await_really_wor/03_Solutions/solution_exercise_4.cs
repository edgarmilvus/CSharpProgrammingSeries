
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncStateMachinesExercises
{
    // 1. Custom Awaiter
    public struct CustomTaskAwaiter : INotifyCompletion
    {
        private readonly int _delayMs;

        public CustomTaskAwaiter(int delayMs)
        {
            _delayMs = delayMs;
            Console.WriteLine($"  [Awaiter] Created with delay {_delayMs}ms");
        }

        // The state machine checks this property repeatedly (polling) if true, it executes the continuation immediately.
        // If false, it calls OnCompleted.
        public bool IsCompleted => false; 

        public void GetResult()
        {
            // In a real implementation, this would block or throw.
            // Here, we simulate the "result" retrieval logic.
            Console.WriteLine($"  [Awaiter] GetResult() called.");
        }

        public void OnCompleted(Action continuation)
        {
            Console.WriteLine($"  [Awaiter] OnCompleted called. Scheduling continuation on ThreadPool.");
            
            // Simulate the async operation completing later.
            // We offload the continuation to a timer to mimic the state machine resuming.
            Timer _ = new Timer(_ => 
            {
                Console.WriteLine($"  [Timer] Delay finished. Invoking continuation.");
                continuation(); 
            }, null, _delayMs, Timeout.Infinite);
        }
    }

    // 2. Custom Stateless Task
    public class CustomStatelessTask
    {
        private readonly int _delayMs;

        public CustomStatelessTask(int delayMs)
        {
            _delayMs = delayMs;
        }

        public CustomTaskAwaiter GetAwaiter()
        {
            return new CustomTaskAwaiter(_delayMs);
        }
    }

    public class Exercise4
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("=== Exercise 4: Custom State Machine Implementation ===\n");

            Console.WriteLine("Calling custom awaitable...");
            
            // The compiler transforms this method into a state machine.
            // It calls GetAwaiter(), checks IsCompleted, and calls OnCompleted if needed.
            await new CustomStatelessTask(500);

            Console.WriteLine("Continuation executed. Method finished.");
        }
    }
}
