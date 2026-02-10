
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
using System.Linq;

namespace StructVsClassAnalysis
{
    // Class implementation: Heap allocated, reference type
    public class TokenClass
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public float[] Embeddings { get; set; }
        public double Confidence { get; set; }

        public TokenClass(int id)
        {
            Id = id;
            Text = $"Token_{id}";
            Embeddings = new float[128];
            Confidence = 1.0;
        }
    }

    // Struct implementation: Stack or Array allocated, value type
    public struct TokenStruct
    {
        public int Id;
        public string Text; // Reference type (pointer on stack/heap)
        public float[] Embeddings; // Reference type (pointer on stack/heap)
        public double Confidence;

        public TokenStruct(int id)
        {
            Id = id;
            Text = $"Token_{id}";
            Embeddings = new float[128]; // Note: This allocates on heap, but the struct holds the reference
            Confidence = 1.0;
        }
    }

    public class PipelineSimulator
    {
        // Dummy operation to prevent JIT optimization
        private static double _globalSum = 0;

        public static (long TimeMs, long MemoryDelta) ProcessTokensClass(int count)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long memBefore = GC.GetTotalMemory(true);

            var sw = Stopwatch.StartNew();
            
            // Allocate N objects on the heap
            for (int i = 0; i < count; i++)
            {
                var token = new TokenClass(i);
                // Dummy operation
                _globalSum += token.Confidence;
            }

            sw.Stop();
            long memAfter = GC.GetTotalMemory(false); // Don't force collection here to see live allocation

            return (sw.ElapsedMilliseconds, memAfter - memBefore);
        }

        public static (long TimeMs, long MemoryDelta) ProcessTokensStruct(int count)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long memBefore = GC.GetTotalMemory(true);

            var sw = Stopwatch.StartNew();

            // Allocate a single block of memory for structs (on heap via array)
            // In a real scenario, we might use Span<T> on stack, but for 10M items, 
            // we must use an array to avoid StackOverflow.
            TokenStruct[] tokens = new TokenStruct[count];
            
            for (int i = 0; i < count; i++)
            {
                tokens[i] = new TokenStruct(i);
                // Dummy operation
                _globalSum += tokens[i].Confidence;
            }

            sw.Stop();
            long memAfter = GC.GetTotalMemory(false);

            return (sw.ElapsedMilliseconds, memAfter - memBefore);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int count = 10_000_000;

            Console.WriteLine($"Processing {count:N0} tokens...\n");

            // Run Class Test
            var (classTime, classMem) = PipelineSimulator.ProcessTokensClass(count);
            Console.WriteLine($"[Class] Time: {classTime}ms, Memory Delta: {classMem / 1024.0 / 1024.0:F2} MB");

            // Run Struct Test
            var (structTime, structMem) = PipelineSimulator.ProcessTokensStruct(count);
            Console.WriteLine($"[Struct] Time: {structTime}ms, Memory Delta: {structMem / 1024.0 / 1024.0:F2} MB");
        }
    }
}
