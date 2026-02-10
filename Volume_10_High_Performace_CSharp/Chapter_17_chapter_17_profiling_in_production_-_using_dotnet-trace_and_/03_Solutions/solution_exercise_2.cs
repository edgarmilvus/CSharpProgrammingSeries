
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TokenProcessorTrace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Token Processor with Bottleneck Simulation...");
            var cts = new CancellationTokenSource();

            // Cycle between slow and fast processing every 5 seconds
            var loadTask = Task.Run(async () =>
            {
                bool useSlowMethod = true;
                while (!cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine($"--- Switching Mode: {(useSlowMethod ? "SLOW (O(N^2))" : "FAST (O(N))")} ---");
                    await ProcessTokens(useSlowMethod);
                    useSlowMethod = !useSlowMethod;
                }
            });

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            cts.Cancel();
        }

        static async Task ProcessTokens(bool useSlowMethod)
        {
            var sw = Stopwatch.StartNew();
            // Run for 5 seconds per mode
            while (sw.Elapsed.TotalSeconds < 5)
            {
                string token = "SIMD_Initialized_Token_Data_Payload";
                
                if (useSlowMethod)
                {
                    ProcessTokenSlow(token);
                }
                else
                {
                    ProcessTokenFast(token);
                }
            }
        }

        // Bottleneck: Naive string concatenation in a loop (O(N^2))
        static void ProcessTokenSlow(string input)
        {
            string result = "";
            // Simulate heavy allocation and CPU via repeated concatenation
            for (int i = 0; i < 1000; i++)
            {
                result += input; 
            }
            // Prevent optimization
            if (result.Length > 1000000) Console.WriteLine("Processing...");
        }

        // Optimized: StringBuilder (O(N))
        static void ProcessTokenFast(string input)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                sb.Append(input);
            }
            string result = sb.ToString();
            if (result.Length > 1000000) Console.WriteLine("Processing...");
        }
    }
}
