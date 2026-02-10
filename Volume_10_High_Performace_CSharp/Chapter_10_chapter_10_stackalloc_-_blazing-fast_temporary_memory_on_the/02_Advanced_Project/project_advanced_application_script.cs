
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HighPerformanceTokenProcessing
{
    class Program
    {
        // REAL-WORLD CONTEXT:
        // Imagine a high-frequency trading (HFT) system or a real-time chat server.
        // These systems process millions of small text messages (tokens) per second.
        // The critical path involves:
        // 1. Reading raw bytes from a network buffer.
        // 2. Parsing the token (e.g., identifying a command like "BUY", "SELL", "PING").
        // 3. Dispatching to a handler.
        // 
        // The bottleneck is often memory allocation. Creating a "string" for every token triggers
        // the Garbage Collector (GC), causing pauses (latency spikes). In HFT, a 1ms pause can cost millions.
        // Solution: Use `stackalloc` to allocate memory on the stack for temporary parsing,
        // and `Span<T>` to slice the input buffer without copying data.

        static void Main(string[] args)
        {
            Console.WriteLine("--- High-Performance Token Processor Simulation ---");
            
            // 1. Setup: Simulate a raw byte stream buffer containing multiple commands.
            // In a real system, this comes from NetworkStream.Read().
            byte[] rawNetworkBuffer = System.Text.Encoding.UTF8.GetBytes(
                "PING|12345;BUY|AAPL|100;SELL|TSLA|50;PING|67890;");

            Console.WriteLine($"Input Buffer: {System.Text.Encoding.UTF8.GetString(rawNetworkBuffer)}");
            Console.WriteLine();

            // 2. Process the buffer using our optimized pipeline.
            ProcessBufferOptimized(rawNetworkBuffer);

            Console.WriteLine("\n--- Processing Complete ---");
        }

        /// <summary>
        /// The core processing loop. 
        /// CRITICAL: We pass the input buffer as a ReadOnlySpan<byte>.
        /// This avoids passing the entire array reference and allows slicing without allocation.
        /// </summary>
        static void ProcessBufferOptimized(ReadOnlySpan<byte> buffer)
        {
            int cursor = 0;
            while (cursor < buffer.Length)
            {
                // Find the end of the current token (delimited by ';')
                int tokenEnd = cursor;
                while (tokenEnd < buffer.Length && buffer[tokenEnd] != (byte)';')
                {
                    tokenEnd++;
                }

                // Slice the current token (excluding the ';')
                // This is a zero-copy operation. No memory is allocated here.
                ReadOnlySpan<byte> tokenSpan = buffer.Slice(cursor, tokenEnd - cursor);

                if (tokenSpan.Length > 0)
                {
                    // PROCESS TOKEN
                    // We pass the small span to the parser.
                    // The parser will use `stackalloc` internally for its temporary needs.
                    ParseAndDispatchToken(tokenSpan);
                }

                // Move cursor past the ';'
                cursor = tokenEnd + 1;
            }
        }

        /// <summary>
        /// Parses a single token and dispatches it.
        /// Uses `stackalloc` for temporary string formatting and SIMD for validation.
        /// </summary>
        static unsafe void ParseAndDispatchToken(ReadOnlySpan<byte> token)
        {
            // SAFETY CHECK: Stackalloc is dangerous with arbitrary sizes.
            // We enforce a strict maximum limit for the stack allocation.
            // If a token exceeds this, we fall back to a slower, safer method (or throw).
            const int MaxStackAllocSize = 128; 

            if (token.Length > MaxStackAllocSize)
            {
                Console.WriteLine($"[!] Token too large for stack: {token.Length} bytes. Skipping.");
                return;
            }

            // --- CONCEPT: stackalloc ---
            // We allocate a buffer on the stack. 
            // Memory is reclaimed automatically when the method returns. 
            // Zero GC pressure.
            // NOTE: 'unsafe' context is required for 'stackalloc' in older C# versions, 
            // but modern C# allows it in safe contexts with Span<T>.
            // However, we use 'unsafe' here to demonstrate pointer access for SIMD if needed, 
            // though Vector256 operations work on Span<T> too.
            // 
            // We allocate a buffer to hold a copy of the token if we need to modify it 
            // (e.g., convert to uppercase for command matching).
            Span<byte> tempBuffer = stackalloc byte[MaxStackAllocSize];
            
            // Copy token to tempBuffer. 
            // 'token.CopyTo' is highly optimized.
            token.CopyTo(tempBuffer);
            
            // Resize the span to the actual token length within the stack buffer.
            tempBuffer = tempBuffer.Slice(0, token.Length);

            // --- CONCEPT: SIMD Vectorization (AVX2) ---
            // We want to identify the command type quickly.
            // Let's assume the format is: COMMAND|DATA
            // We need to find the '|' delimiter.
            // We can use AVX2 (Vector256) to scan 32 bytes at once.
            
            int delimiterIndex = -1;
            bool useSimd = Avx2.IsSupported && token.Length >= 32;

            if (useSimd)
            {
                // Vectorized search for the '|' character (ASCII 124)
                delimiterIndex = FindDelimiterVectorized(tempBuffer);
            }
            else
            {
                // Fallback to scalar loop for small tokens or unsupported hardware
                for (int i = 0; i < tempBuffer.Length; i++)
                {
                    if (tempBuffer[i] == (byte)'|')
                    {
                        delimiterIndex = i;
                        break;
                    }
                }
            }

            // If no delimiter found, treat whole token as command (or invalid)
            if (delimiterIndex == -1) delimiterIndex = tempBuffer.Length;

            // Extract Command Part (before '|')
            // Since we are on the stack, slicing is instant.
            ReadOnlySpan<byte> commandSpan = tempBuffer.Slice(0, delimiterIndex);

            // --- LOGIC: Pattern Matching (Basic) ---
            // We cannot use Switch Expressions or Pattern Matching (unless introduced).
            // We use basic string comparison on the span.
            // Note: We compare against byte sequences representing the ASCII strings.
            
            // "PING" = 80, 73, 78, 71
            if (IsMatch(commandSpan, 80, 73, 78, 71)) 
            {
                HandlePing();
            }
            // "BUY" = 66, 85, 89
            else if (IsMatch(commandSpan, 66, 85, 89))
            {
                // Extract Data Part (after '|')
                ReadOnlySpan<byte> dataSpan = tempBuffer.Slice(delimiterIndex + 1);
                HandleBuy(dataSpan);
            }
            // "SELL" = 83, 69, 76, 76
            else if (IsMatch(commandSpan, 83, 69, 76, 76))
            {
                ReadOnlySpan<byte> dataSpan = tempBuffer.Slice(delimiterIndex + 1);
                HandleSell(dataSpan);
            }
            else
            {
                HandleUnknown(commandSpan);
            }
        }

        /// <summary>
        /// SIMD Accelerated Delimiter Finder.
        /// Scans 32 bytes at once using AVX2.
        /// </summary>
        static unsafe int FindDelimiterVectorized(Span<byte> buffer)
        {
            // We need to pin the memory or get a pointer to use low-level intrinsics effectively,
            // though Vector256.Operations can work on Span in newer runtimes.
            // Here we use the pointer approach for explicit control.
            fixed (byte* ptr = buffer)
            {
                Vector256<byte> delimiterVector = Vector256.Create((byte)'|');
                int i = 0;
                int length = buffer.Length;

                // Process 32 bytes at a time
                for (; i <= length - 32; i += 32)
                {
                    Vector256<byte> chunk = Avx.LoadVector256(ptr + i);
                    // Compare equality
                    Vector256<byte> comparison = Avx2.CompareEqual(chunk, delimiterVector);
                    
                    // MoveMask creates an integer where each bit corresponds to a byte comparison result.
                    // If any bit is set, we found a delimiter.
                    int mask = Avx2.MoveMask(comparison);
                    if (mask != 0)
                    {
                        // Found it! Calculate the exact index.
                        // Count trailing zeros to find the first match.
                        return i + BitOperations.TrailingZeroCount(mask);
                    }
                }

                // Handle remaining bytes (scalar fallback)
                for (; i < length; i++)
                {
                    if (buffer[i] == (byte)'|') return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Helper to compare a Span against specific byte values.
        /// </summary>
        static bool IsMatch(ReadOnlySpan<byte> span, params byte[] values)
        {
            if (span.Length != values.Length) return false;
            for (int i = 0; i < values.Length; i++)
            {
                if (span[i] != values[i]) return false;
            }
            return true;
        }

        // --- Handler Methods (Simulated Logic) ---
        static void HandlePing() => Console.WriteLine("  [Action] PING received. Responding PONG.");
        
        static void HandleBuy(ReadOnlySpan<byte> data)
        {
            // Convert Span to string only for display (allocation happens here for demo)
            // In real HFT, we would parse the bytes directly to integers/floats.
            string dataStr = System.Text.Encoding.UTF8.GetString(data);
            Console.WriteLine($"  [Action] BUY order: {dataStr}");
        }

        static void HandleSell(ReadOnlySpan<byte> data)
        {
            string dataStr = System.Text.Encoding.UTF8.GetString(data);
            Console.WriteLine($"  [Action] SELL order: {dataStr}");
        }

        static void HandleUnknown(ReadOnlySpan<byte> cmd)
        {
            string cmdStr = System.Text.Encoding.UTF8.GetString(cmd);
            Console.WriteLine($"  [Warning] Unknown command: {cmdStr}");
        }
    }
}
