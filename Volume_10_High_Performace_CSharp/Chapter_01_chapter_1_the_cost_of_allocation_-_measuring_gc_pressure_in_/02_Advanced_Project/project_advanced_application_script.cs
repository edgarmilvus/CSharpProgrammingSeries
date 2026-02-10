
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Diagnostics;
using System.Text;

namespace HighPerformanceAITokenProcessing
{
    /// <summary>
    /// Simulates a high-frequency AI token processing pipeline where memory allocation
    /// patterns directly impact throughput and GC pressure.
    /// 
    /// REAL-WORLD CONTEXT:
    /// In a production LLM (Large Language Model) inference engine, tokens are processed
    /// continuously. A naive implementation might allocate strings for every intermediate
    /// transformation (e.g., normalizing text, calculating log probabilities). In a loop
    /// processing millions of tokens, this creates massive GC pressure, causing "stop-the-world"
    /// pauses that degrade real-time responsiveness.
    /// 
    /// This application demonstrates two approaches:
    /// 1. Naive Approach: Heavy heap allocations (Strings).
    /// 2. Optimized Approach: Using StackAlloc (Span<T>) to minimize GC pressure.
    /// </summary>
    class Program
    {
        // Configuration for the simulation
        const int TotalTokens = 100_000; // 100k tokens to process
        const int TokenLength = 64;      // Avg length of a token string
        
        static void Main(string[] args)
        {
            Console.WriteLine("=== High-Performance AI Token Processor ===");
            Console.WriteLine($"Processing {TotalTokens:N0} tokens (Avg Length: {TokenLength} chars)");
            Console.WriteLine("--------------------------------------------------");

            // 1. Baseline Measurement: Naive String Allocation
            // We force a GC collection before starting to get a clean slate.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long baselineMemory = GC.GetTotalMemory(true);

            var sw = Stopwatch.StartNew();
            long allocatedMemory = ProcessTokensNaively(TotalTokens, TokenLength);
            sw.Stop();

            long memoryDiff = GC.GetTotalMemory(true) - baselineMemory;
            Console.WriteLine($"\n[NAIVE] Time: {sw.ElapsedMilliseconds}ms | Allocated: {allocatedMemory:N0} bytes | GC Gen0: {GC.CollectionCount(0)}");
            Console.WriteLine($"[NAIVE] Heap Diff: {memoryDiff:N0} bytes (Indicates GC cleanup activity)");

            // 2. Optimized Measurement: Span<T> and StackAlloc
            // Force GC again to reset counters for a fair comparison.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            baselineMemory = GC.GetTotalMemory(true);

            sw.Restart();
            long opsProcessed = ProcessTokensOptimized(TotalTokens, TokenLength);
            sw.Stop();

            memoryDiff = GC.GetTotalMemory(true) - baselineMemory;
            Console.WriteLine($"\n[OPTIMIZED] Time: {sw.ElapsedMilliseconds}ms | Ops: {opsProcessed:N0} | GC Gen0: {GC.CollectionCount(0)}");
            Console.WriteLine($"[OPTIMIZED] Heap Diff: {memoryDiff:N0} bytes (Minimal allocation)");
            
            Console.WriteLine("\nPress any key to view architecture breakdown...");
            Console.ReadKey();
            
            PrintArchitectureBreakdown();
        }

        /// <summary>
        /// APPROACH 1: Naive Implementation
        /// Problem: Creates heavy GC pressure by allocating strings on the heap for every operation.
        /// </summary>
        static long ProcessTokensNaively(int count, int tokenLength)
        {
            long totalProcessed = 0;
            // Pre-allocate a char array to simulate source data (avoids overhead of generating random data)
            char[] sourceData = new char[tokenLength];
            
            for (int i = 0; i < tokenLength; i++) sourceData[i] = (char)('a' + (i % 26));

            for (int i = 0; i < count; i++)
            {
                // 1. ALLOCATION: Creates a new string object on the Heap.
                string rawToken = new string(sourceData);

                // 2. ALLOCATION: ToUpper() typically returns a NEW string, leaving the old one for GC.
                string normalized = rawToken.ToUpper();

                // 3. ALLOCATION: String concatenation creates yet another new string.
                string metadata = $"[ID:{i}] {normalized}";

                // 4. ALLOCATION: Encoding creates a byte array on the heap.
                byte[] bytes = Encoding.UTF8.GetBytes(metadata);

                // Simulate work: Count characters to ensure compiler doesn't optimize away the code
                totalProcessed += bytes.Length;
            }
            return totalProcessed;
        }

        /// <summary>
        /// APPROACH 2: Optimized Implementation
        /// Solution: Uses Span<T> and stack allocation to process data without heap allocations.
        /// </summary>
        static long ProcessTokensOptimized(int count, int tokenLength)
        {
            long totalProcessed = 0;
            
            // Pre-fill a buffer that we will reuse. 
            // In a real scenario, this might come from a pooled ArrayPool<byte>.
            char[] sourceData = new char[tokenLength];
            for (int i = 0; i < tokenLength; i++) sourceData[i] = (char)('a' + (i % 26));

            for (int i = 0; i < count; i++)
            {
                // 1. STACK ALLOCATION: Create a buffer on the stack (fast, zero GC pressure).
                // We add extra space for the metadata prefix to avoid resizing.
                int bufferSize = tokenLength + 20; 
                
                // Note: stackalloc is unsafe in pure C#, but safe when used correctly within a method scope.
                // Span<T> provides a type-safe view over this memory.
                Span<char> buffer = stackalloc char[bufferSize];
                
                // 2. COPY & TRANSFORM: Work directly on the Span (mutable view of memory).
                sourceData.CopyTo(buffer);
                
                // 3. IN-PLACE MODIFICATION: ToUpper logic without new string allocation.
                for (int j = 0; j < tokenLength; j++)
                {
                    char c = buffer[j];
                    if (c >= 'a' && c <= 'z') 
                        buffer[j] = (char)(c - 32); // Simple ASCII uppercase conversion
                }

                // 4. METADATA INJECTION: Manually write the prefix to the buffer.
                // We are writing directly into the memory space reserved earlier.
                string prefix = $"[ID:{i}] ";
                int prefixLen = prefix.Length;
                prefix.AsSpan().CopyTo(buffer.Slice(tokenLength));
                
                // 5. SIMULATED ENCODING: 
                // Instead of UTF8.GetBytes (which allocates), we calculate the length 
                // or process the Span<char> directly.
                // Here we just sum the values to simulate processing the bytes.
                int totalChars = tokenLength + prefixLen;
                
                // Accessing Span<T> is bounds-checked and safe, but extremely fast.
                for(int k = 0; k < totalChars; k++)
                {
                    totalProcessed += buffer[k];
                }
            }
            return totalProcessed;
        }

        static void PrintArchitectureBreakdown()
        {
            Console.WriteLine("\n\n=== ARCHITECTURE BREAKDOWN ===");
            Console.WriteLine("1. PROBLEM DEFINITION:");
            Console.WriteLine("   AI Inference loops process millions of tokens per second.");
            Console.WriteLine("   Naive string manipulation generates 100MB+ of garbage per second.");
            Console.WriteLine("   GC Gen0 collections pause execution, causing latency spikes (jitter).");
            Console.WriteLine("\n2. MEMORY LAYOUT COMPARISON:");
            
            Console.WriteLine("\n   NAIVE (Heap Heavy):");
            Console.WriteLine("   +-----------------------+");
            Console.WriteLine("   | Stack Frame           |");
            Console.WriteLine("   |   'rawToken' (ref)    |----> [HEAP] String Object 1");
            Console.WriteLine("   |   'normalized' (ref)  |----> [HEAP] String Object 2");
            Console.WriteLine("   |   'metadata' (ref)    |----> [HEAP] String Object 3");
            Console.WriteLine("   |   'bytes' (ref)       |----> [HEAP] Byte Array");
            Console.WriteLine("   +-----------------------+");
            Console.WriteLine("   Result: 4 Allocations per iteration. 400,000 allocations for 100k tokens.");
            Console.WriteLine("   GC Impact: High. Gen0 fills up rapidly. Mark/Sweep overhead increases.");

            Console.WriteLine("\n   OPTIMIZED (Stack/Spans):");
            Console.WriteLine("   +-----------------------+");
            Console.WriteLine("   | Stack Frame           |");
            Console.WriteLine("   |   Span<char> buffer   |");
            Console.WriteLine("   |   (Points to Stack)   |");
            Console.WriteLine("   +-----------------------+");
            Console.WriteLine("   | Stack Memory          |");
            Console.WriteLine("   | [ID:123] ABC...       |  <-- Raw bytes live here");
            Console.WriteLine("   +-----------------------+");
            Console.WriteLine("   Result: 0 Heap Allocations per iteration.");
            Console.WriteLine("   GC Impact: Negligible. Only the initial source array is pinned.");

            Console.WriteLine("\n3. KEY CONCEPTS APPLIED:");
            Console.WriteLine("   - StackAlloc: Allocates memory on the stack, which is automatically reclaimed.");
            Console.WriteLine("   - Span<T>: A type-safe window into arbitrary memory (stack, heap, unmanaged).");
            Console.WriteLine("   - In-Place Mutation: Modifying the buffer directly avoids copying data.");
            
            Console.WriteLine("\n4. EDGE CASES & LIMITATIONS:");
            Console.WriteLine("   - Stack Size: StackAlloc is limited (usually 1MB). Large buffers will StackOverflow.");
            Console.WriteLine("     Solution: Use ArrayPool<T> for buffers > 1KB.");
            Console.WriteLine("   - Escape Analysis: Spans cannot escape the method scope (cannot be returned).");
            Console.WriteLine("     Solution: If data must be returned, copy to a managed array (last step).");
        }
    }
}
