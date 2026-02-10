
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TokenProcessorMonitor
{
    class Program
    {
        // Define a Meter and Instruments
        private static readonly Meter TokenMeter = new Meter("TokenProcessor", "1.0.0");
        private static readonly Counter<long> TokensProcessed = TokenMeter.CreateCounter<long>("tokens_processed");
        private static readonly Histogram<double> TokenProcessingDuration = TokenMeter.CreateHistogram<double>("token_processing_duration_ms");

        static async Task Main(string[] args)
        {
            bool simulateLoad = args.Contains("--simulate-load");
            
            Console.WriteLine($"Starting Token Processor. Load Simulation: {simulateLoad}");
            Console.WriteLine("Press 'l' to toggle load, 'q' to quit.");

            // Seed random for variability
            var random = new Random();
            
            // Simulate an infinite loop processing tokens
            var cts = new CancellationTokenSource();
            
            // Monitor keyboard input in a separate task
            var inputTask = Task.Run(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                    {
                        cts.Cancel();
                        break;
                    }
                    if (key.KeyChar == 'l' || key.KeyChar == 'L')
                    {
                        simulateLoad = !simulateLoad;
                        Console.WriteLine($"\nLoad simulation toggled to: {simulateLoad}");
                    }
                }
            });

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    // 1. Simulate Token Processing
                    var token = $"token_{random.Next(1000)}";
                    var sw = Stopwatch.StartNew();

                    // Simulate CPU work
                    double complexity = simulateLoad ? 10000 : 1000; 
                    double result = 0;
                    for (int i = 0; i < complexity; i++) 
                    {
                        result += Math.Sqrt(i); // CPU intensive work
                    }

                    // 2. Simulate Memory Allocation (Pressure)
                    if (simulateLoad)
                    {
                        // High allocation: Create large strings to trigger GC
                        var list = new List<string>();
                        for (int i = 0; i < 1000; i++)
                        {
                            list.Add(new string('x', 100)); // Allocates ~100KB per iteration
                        }
                        // Force reference to prevent optimization
                        if (list.Count > 999999) Console.WriteLine("Should not happen");
                    }
                    else
                    {
                        // Low allocation
                        var smallObj = new object(); 
                    }

                    sw.Stop();

                    // 3. Record Custom Metrics
                    TokensProcessed.Add(1, new KeyValuePair<string, object?>("token_type", "text"));
                    TokenProcessingDuration.Record(sw.Elapsed.TotalMilliseconds);

                    // Small delay to prevent 100% CPU usage if not simulating load
                    if (!simulateLoad) Thread.Sleep(10);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Shutting down...");
            }
        }
    }
}
