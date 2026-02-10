
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingLLM
{
    class Program
    {
        // 1. Async Stream Simulation
        public static async IAsyncEnumerable<string> StreamResponse()
        {
            var tokens = new[] { "Hello", " ", "world", ",", " this", " is", " a", " streamed", " response", "!" };
            
            foreach (var token in tokens)
            {
                // Simulate network latency per token
                await Task.Delay(100);
                yield return token;
            }
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting stream consumption...");
            
            // Buffer for UI simulation
            var buffer = new List<string>();
            var flushInterval = TimeSpan.FromMilliseconds(500);
            var lastFlushTime = DateTime.UtcNow;
            int tokenCounter = 0;

            // 2. Consume the stream
            await foreach (var token in StreamResponse())
            {
                buffer.Add(token);
                tokenCounter++;

                // Check conditions to flush: 
                // A. Buffer size reaches 5 tokens
                // B. Time since last flush exceeds 500ms
                var now = DateTime.UtcNow;
                if (buffer.Count >= 5 || (now - lastFlushTime > flushInterval && buffer.Count > 0))
                {
                    FlushBuffer(buffer, ref lastFlushTime);
                }
            }

            // Flush any remaining tokens
            if (buffer.Count > 0)
            {
                FlushBuffer(buffer, ref lastFlushTime);
            }

            Console.WriteLine($"\nStream finished. Total tokens processed: {tokenCounter}");
        }

        private static void FlushBuffer(List<string> buffer, ref DateTime lastFlushTime)
        {
            var text = string.Join("", buffer);
            Console.WriteLine($"[UI Update]: {text}");
            buffer.Clear();
            lastFlushTime = DateTime.UtcNow;
        }
    }
}
