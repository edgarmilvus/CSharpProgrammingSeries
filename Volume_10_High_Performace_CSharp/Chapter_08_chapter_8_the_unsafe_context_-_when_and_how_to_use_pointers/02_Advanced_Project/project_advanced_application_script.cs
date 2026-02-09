
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HighPerformanceAI.Memory
{
    /// <summary>
    /// Demonstrates unsafe pointer manipulation for high-speed memory operations.
    /// Scenario: AI Inference Engine Tokenizer.
    /// In a real-time AI model (like a Large Language Model), the tokenizer must rapidly
    /// identify known tokens within a massive input buffer. Using safe C# (Span<T>) is fast,
    /// but using unsafe pointers allows us to bypass bounds checking and array overhead
    /// for raw memory throughput.
    /// </summary>
    public unsafe class TokenizerBenchmark
    {
        // Configuration for our synthetic data
        private const int BufferSize = 1024 * 1024; // 1 MB buffer
        private const int TokenCount = 1000;
        private const int TokenLength = 5;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== AI Tokenizer: Unsafe Memory Optimization ===");
            Console.WriteLine($"Buffer Size: {BufferSize} bytes");
            Console.WriteLine($"Target Token Length: {TokenLength} chars");
            Console.WriteLine();

            // 1. Setup: Generate synthetic token data and a search buffer
            byte[] searchBuffer = new byte[BufferSize];
            byte[] targetToken = new byte[TokenLength];

            // Fill buffer with random data (simulating text stream)
            Random.Shared.NextBytes(searchBuffer);
            // Generate a specific token to find (e.g., "AI_TOK")
            Random.Shared.NextBytes(targetToken); 
            
            // Inject the target token at specific locations to ensure we find it
            int injectionIndex1 = BufferSize / 3;
            int injectionIndex2 = BufferSize / 2;
            Buffer.BlockCopy(targetToken, 0, searchBuffer, injectionIndex1, TokenLength);
            Buffer.BlockCopy(targetToken, 0, searchBuffer, injectionIndex2, TokenLength);

            Console.WriteLine($"Target Token injected at indices: {injectionIndex1}, {injectionIndex2}");
            Console.WriteLine();

            // 2. Warm up JIT and GC
            Console.WriteLine("Warming up...");
            RunSafeSearch(searchBuffer, targetToken);
            RunUnsafeSearch(searchBuffer, targetToken);
            Console.WriteLine();

            // 3. Benchmark Safe Implementation
            Console.WriteLine("Running Safe Implementation (Bounds Checked)...");
            long safeCount = 0;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                safeCount = RunSafeSearch(searchBuffer, targetToken);
            }
            sw.Stop();
            Console.WriteLine($"Safe Result: Found {safeCount} matches. Time: {sw.ElapsedMilliseconds} ms");

            // 4. Benchmark Unsafe Implementation
            Console.WriteLine("Running Unsafe Implementation (Pointer Arithmetic)...");
            long unsafeCount = 0;
            sw.Restart();
            for (int i = 0; i < 100; i++)
            {
                unsafeCount = RunUnsafeSearch(searchBuffer, targetToken);
            }
            sw.Stop();
            Console.WriteLine($"Unsafe Result: Found {unsafeCount} matches. Time: {sw.ElapsedMilliseconds} ms");
            
            Console.WriteLine();
            Console.WriteLine("Optimization Complete.");
        }

        /// <summary>
        /// Standard Safe C# implementation using array indexing.
        /// Relies on the runtime to check bounds on every access.
        /// </summary>
        private static long RunSafeSearch(byte[] buffer, byte[] token)
        {
            long matches = 0;
            int limit = buffer.Length - token.Length;

            // Standard for-loop with bounds checking
            for (int i = 0; i < limit; i++)
            {
                bool found = true;
                for (int j = 0; j < token.Length; j++)
                {
                    if (buffer[i + j] != token[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) matches++;
            }
            return matches;
        }

        /// <summary>
        /// High-performance Unsafe implementation using pointers.
        /// Bypasses array bounds checking and uses pointer dereferencing.
        /// </summary>
        private static unsafe long RunUnsafeSearch(byte[] buffer, byte[] token)
        {
            long matches = 0;
            
            // FIXED STATEMENT:
            // Pins the managed byte array in memory so the Garbage Collector (GC)
            // cannot move it while we are using pointers. This is crucial because
            // pointers point to physical memory addresses; if the GC compacts the heap
            // and moves the array, our pointers would become invalid (dangling).
            fixed (byte* pBuffer = buffer)
            fixed (byte* pToken = token)
            {
                byte* pCurrent = pBuffer;
                byte* pEnd = pBuffer + buffer.Length - token.Length;
                int tokenLen = token.Length;

                // POINTER LOOP:
                // We iterate until pCurrent reaches the safe end limit.
                while (pCurrent < pEnd)
                {
                    // Direct memory comparison using pointers.
                    // We compare the first byte first (cheap check).
                    if (*pCurrent == *pToken)
                    {
                        bool match = true;

                        // INNER LOOP:
                        // Compare remaining bytes. 
                        // Note: In a production SIMD scenario, we would load 16 or 32 bytes 
                        // at once using Vector<T>, but here we demonstrate raw pointer arithmetic.
                        for (int i = 1; i < tokenLen; i++)
                        {
                            // Dereference pointers to compare memory at specific offsets
                            if (*(pCurrent + i) != *(pToken + i))
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match) matches++;
                    }

                    // POINTER INCREMENT:
                    // Move to the next byte in memory. 
                    // Pointer arithmetic automatically scales by the size of the type (byte = 1).
                    pCurrent++;
                }
            }
            // END OF FIXED BLOCK:
            // The buffer is now unpinned and safe for the GC to manage again.
            
            return matches;
        }
    }
}
