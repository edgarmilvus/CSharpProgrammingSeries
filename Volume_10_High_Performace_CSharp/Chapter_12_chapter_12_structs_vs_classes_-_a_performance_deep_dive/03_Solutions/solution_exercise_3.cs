
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpanRefactoring
{
    public class TokenClass
    {
        public int Id { get; set; }
        public double Score { get; set; }
        public int Category { get; set; } // 0-3
    }

    public struct TokenStruct
    {
        public int Id;
        public double Score;
        public int Category;
    }

    public class LegacyProcessor
    {
        // Legacy method causing GC pressure
        public static double[] LegacyProcessBatch(List<TokenClass> tokens)
        {
            // Allocates a List (heap) and internal array
            var categoryScores = new List<double>(4) { 0, 0, 0, 0 };

            foreach (var token in tokens)
            {
                // Accessing properties causes overhead
                categoryScores[token.Category] += token.Score;
            }

            // Returns a new array (heap allocation)
            return categoryScores.ToArray();
        }
    }

    public class RefactoredProcessor
    {
        // Refactored method: Zero heap allocations for logic
        public static double[] RefactoredProcessBatch(ReadOnlySpan<TokenStruct> tokens)
        {
            // Stack allocation: Memory lives on the stack, zero GC pressure
            // Note: We cannot return a Span to the stack, so we copy to heap at the very end 
            // ONLY if the result must be returned. 
            // However, to strictly follow "no heap allocations inside processing", we use a fixed array.
            
            Span<double> categoryScores = stackalloc double[4]; 
            
            foreach (ref readonly var token in tokens)
            {
                // Direct memory access, no virtual calls or property overhead
                categoryScores[token.Category] += token.Score;
            }

            // If we must return a double[], we copy here. 
            // In a pure pipeline, we might process the Span directly without returning.
            double[] result = new double[4];
            categoryScores.CopyTo(result);
            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int batchSize = 50_000;
            var rng = new Random(42);

            // Setup Legacy Data
            var legacyList = new List<TokenClass>(batchSize);
            for (int i = 0; i < batchSize; i++)
            {
                legacyList.Add(new TokenClass { Id = i, Score = rng.NextDouble(), Category = rng.Next(0, 4) });
            }

            // Setup Refactored Data
            var structArray = new TokenStruct[batchSize];
            for (int i = 0; i < batchSize; i++)
            {
                structArray[i] = new TokenStruct { Id = i, Score = rng.NextDouble(), Category = rng.Next(0, 4) };
            }

            // Measure Legacy
            GC.Collect();
            long memBefore = GC.GetTotalMemory(true);
            var sw = Stopwatch.StartNew();
            var legacyResult = LegacyProcessor.LegacyProcessBatch(legacyList);
            sw.Stop();
            long memAfter = GC.GetTotalMemory(false);
            Console.WriteLine($"Legacy: Time={sw.ElapsedMilliseconds}ms, Allocated={(memAfter - memBefore) / 1024.0:F2} KB");

            // Measure Refactored
            GC.Collect();
            memBefore = GC.GetTotalMemory(true);
            sw.Restart();
            // We pass the underlying array as a ReadOnlySpan
            var refactoredResult = RefactoredProcessor.RefactoredProcessBatch(structArray);
            sw.Stop();
            memAfter = GC.GetTotalMemory(false);
            Console.WriteLine($"Refactored: Time={sw.ElapsedMilliseconds}ms, Allocated={(memAfter - memBefore) / 1024.0:F2} KB");
        }
    }
}
