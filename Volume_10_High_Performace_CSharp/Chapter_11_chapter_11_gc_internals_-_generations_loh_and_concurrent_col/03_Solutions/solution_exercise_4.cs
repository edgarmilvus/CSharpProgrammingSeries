
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Buffers;
using System.Threading.Tasks;
using System.Linq;

namespace GCInternalsExercise4
{
    public class OptimizedTokenProcessor
    {
        // Thread-safe collection for results
        private readonly ConcurrentBag<string> _results = new ConcurrentBag<string>();

        public void ProcessTokens(List<string> rawTokens)
        {
            // Reset results
            _results.Clear();

            Parallel.ForEach(rawTokens, token =>
            {
                // 1. Use ArrayPool for byte buffers (if we were doing byte manipulation)
                // For this string example, we focus on MemoryPool<char> and Span<char>

                // 2. Rent memory from MemoryPool
                using (IMemoryOwner<char> owner = MemoryPool<char>.Shared.Rent(token.Length))
                {
                    var memory = owner.Memory;
                    var span = memory.Span;

                    // Copy token to the rented memory
                    token.AsSpan().CopyTo(span);
                    
                    // Operate on Span<T> (No allocations here)
                    // Example: ToUpper simulation and Remove "SPECIAL"
                    // Note: String manipulation on Span is allocation-free
                    
                    int validLength = token.Length;
                    
                    // Simulate processing: ToUpper (conceptually)
                    for(int i=0; i<validLength; i++)
                    {
                        // Simple char replacement logic for demo
                        if (span[i] >= 'a' && span[i] <= 'z') 
                            span[i] = (char)(span[i] - 32);
                    }

                    // Remove "SPECIAL" (Simulated logic for brevity)
                    // In a real scenario, we might search and slice.
                    // Here we just take the slice.
                    var resultSpan = span.Slice(0, validLength);

                    // 3. Only allocate the final string result
                    // This is unavoidable if we need to store the string, 
                    // but we avoided intermediate strings during processing.
                    string result = new string(resultSpan);
                    _results.Add(result);
                }
            });
        }

        public IEnumerable<string> GetResults() => _results;
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Generate dummy data
            var tokens = new List<string>();
            for (int i = 0; i < 100_000; i++)
            {
                tokens.Add($"token_{i}_data");
            }

            var processor = new OptimizedTokenProcessor();

            // Measure Memory Before
            long memBefore = GC.GetTotalMemory(false);
            
            var sw = Stopwatch.StartNew();
            processor.ProcessTokens(tokens);
            sw.Stop();

            // Measure Memory After
            // Force a GC to see if managed heap grew significantly (it shouldn't much due to pooling)
            GC.Collect();
            long memAfter = GC.GetTotalMemory(true);

            Console.WriteLine("--- Optimized Processing Results ---");
            Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Memory Before: {memBefore / 1024.0:F2} KB");
            Console.WriteLine($"Memory After: {memAfter / 1024.0:F2} KB");
            Console.WriteLine($"Memory Delta: {(memAfter - memBefore) / 1024.0:F2} KB");
            Console.WriteLine($"GC Collections (Gen 0): {GC.CollectionCount(0)}");
            
            // Verify correctness
            Console.WriteLine($"Processed {processor.GetResults().Count()} tokens.");
        }
    }
}
