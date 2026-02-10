
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IOWorker
{
    class Program
    {
        // Refactored method: Decouples I/O start from CPU work dependency
        public async Task<string> ProcessRequest(string query)
        {
            // 1. Start the I/O operation immediately.
            // We do not 'await' it yet; we keep the Task object.
            Task<string> ioTask = SimulateDbLookup(query);

            // 2. We can do other non-dependent work here if any.
            // However, the CPU work depends on the I/O result.
            
            // 3. Await the I/O result.
            string dbResult = await ioTask;

            // 4. Perform CPU-bound work.
            // Note: The CPU work cannot start until the I/O finishes in this specific logic flow
            // because the input to the CPU work is the result of the I/O.
            // However, by not blocking the thread during the I/O wait (using 'await'),
            // the thread is free to handle other requests while this one waits.
            await SimulateCpuCalculation();

            return $"Processed: {dbResult}";
        }

        // Simulates an I/O-bound database lookup
        private async Task<string> SimulateDbLookup(string query)
        {
            await Task.Delay(1000);
            return $"DB_Data_for_{query}";
        }

        // Simulates a CPU-bound calculation
        private async Task SimulateCpuCalculation()
        {
            // In a real scenario, CPU work should be offloaded to Task.Run 
            // to avoid blocking the SynchronizationContext. 
            // Here we simulate it with a delay.
            await Task.Delay(500);
        }

        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Starting concurrent processing of 3 requests...");

            var program = new Program();
            
            // Launch 3 requests concurrently
            var task1 = program.ProcessRequest("Query1");
            var task2 = program.ProcessRequest("Query2");
            var task3 = program.ProcessRequest("Query3");

            // Wait for all to complete
            await Task.WhenAll(task1, task2, task3);

            stopwatch.Stop();
            
            Console.WriteLine($"Results: {task1.Result}, {task2.Result}, {task3.Result}");
            Console.WriteLine($"Total concurrent time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");

            // Analysis:
            // The total time should be roughly 1.5 seconds (1s I/O + 0.5s CPU).
            // While the CPU work cannot overlap with the I/O work for the SAME request 
            // (because the CPU work needs the I/O result), the I/O waits for different 
            // requests overlap perfectly. 
            // Request 1 starts I/O, Request 2 starts I/O, Request 3 starts I/O.
            // While all 3 wait for I/O (1 second), no thread is blocked.
            // Then the CPU work executes (likely sequentially if on a single thread context, 
            // or parallel if offloaded, but total time is dominated by the sum of the longest path).
        }
    }
}
