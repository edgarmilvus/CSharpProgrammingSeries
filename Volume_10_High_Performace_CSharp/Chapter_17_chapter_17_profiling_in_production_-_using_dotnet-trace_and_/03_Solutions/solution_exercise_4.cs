
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TokenProcessorJIT
{
    // Interface for dynamic dispatch
    public interface ITokenProcessor { string Process(string token); }

    // Implementations
    public class TransformerA : ITokenProcessor 
    { 
        public string Process(string token) => $"T-A-{token.ToUpper()}"; 
    }
    public class TransformerB : ITokenProcessor 
    { 
        public string Process(string token) => $"T-B-{token.ToLower()}"; 
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting JIT Analysis Application...");
            Console.WriteLine("Press 'c' for Cold Start trace, 'w' for Warm trace, 'q' to quit.");

            var cts = new CancellationTokenSource();
            
            // Pre-load the factory to avoid JIT noise in the measurement itself
            WarmUpFactory();

            while (!cts.Token.IsCancellationRequested)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q') cts.Cancel();

                if (key.KeyChar == 'c')
                {
                    Console.WriteLine("\n--- COLD START PHASE ---");
                    Console.WriteLine("Processing tokens immediately to trigger JIT...");
                    // Cold Start: Methods haven't been compiled yet
                    await RunProcessingCycle("Cold");
                }
                else if (key.KeyChar == 'w')
                {
                    Console.WriteLine("\n--- WARM PHASE ---");
                    // Warm up first
                    Console.WriteLine("Warming up...");
                    await RunProcessingCycle("Warm-Up");
                    
                    Console.WriteLine("Processing tokens (should be optimized)...");
                    // Actual warm measurement
                    await RunProcessingCycle("Warm");
                }
            }
        }

        static void WarmUpFactory()
        {
            // Minimal code to ensure basic runtime is loaded
            var _ = new TransformerA();
        }

        static async Task RunProcessingCycle(string phaseName)
        {
            var sw = Stopwatch.StartNew();
            var random = new Random();
            
            // Simulate dynamic dispatch
            for (int i = 0; i < 10000; i++)
            {
                ITokenProcessor processor = GetProcessor(i % 2 == 0);
                string result = processor.Process($"Token_{i}");
            }

            sw.Stop();
            Console.WriteLine($"{phaseName} completed in {sw.ElapsedMilliseconds}ms");
        }

        static ITokenProcessor GetProcessor(bool useA)
        {
            // Simulating dynamic loading/reflection
            if (useA) return new TransformerA();
            return new TransformerB();
        }
    }
}
