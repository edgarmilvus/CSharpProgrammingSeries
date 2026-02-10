
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MemoryTDemo
{
    // A custom MemoryManager to demonstrate how to create heap-stable memory
    // without copying data. This is useful for wrapping native memory or
    // pooled buffers in the AI pipeline.
    public sealed class PooledTokenBuffer : MemoryManager<char>
    {
        private readonly char[] _pooledArray;
        private readonly int _length;

        public PooledTokenBuffer(int size)
        {
            // Simulate renting from a high-performance ArrayPool (e.g., ArrayPool<char>.Shared)
            _pooledArray = new char[size];
            _length = size;
        }

        public override Memory<char> Memory => _pooledArray.AsMemory(0, _length);

        public override Span<char> GetSpan() => _pooledArray.AsSpan(0, _length);

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            // Pinning ensures the garbage collector does not move the memory
            // while the pointer is being used (e.g., by native code or SIMD).
            return _pooledArray.AsMemory().Pin();
        }

        public override void Unpin()
        {
            // In a real scenario, this would release a pinned handle.
            // For managed arrays, this is often a no-op but is required by the interface.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Return the array to the pool or clean up resources
                Array.Clear(_pooledArray, 0, _length);
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // CONTEXT: An AI model processes a stream of tokens (text chunks).
            // We receive a large stream from a file or network, but we want to
            // process it asynchronously without copying the data into new strings
            // for every step, which would cause GC pressure.

            Console.WriteLine("--- 1. Creating Heap-Stable Memory from Array ---");
            
            // 1. Standard allocation: Memory<T> wraps an existing array.
            // This is "heap-stable" because the array is on the managed heap,
            // and Memory<T> tracks it safely across async contexts.
            char[] tokenBuffer = new char[1024];
            "Hello AI World".AsSpan().CopyTo(tokenBuffer);
            Memory<char> memorySource = tokenBuffer.AsMemory(0, 14);

            // 2. Demonstrate Async Processing
            // We pass 'memorySource' to an async method. Unlike Span<T>,
            // Memory<T> is allowed to "cross await boundaries".
            await ProcessTokenStreamAsync(memorySource);

            Console.WriteLine("\n--- 2. Using Custom MemoryManager (Pooled) ---");
            
            // 3. Custom Memory Source
            // Using MemoryManager<T> allows us to wrap custom memory sources
            // (like native memory or pooled arrays) and expose them as Memory<T>.
            using (var pooledBuffer = new PooledTokenBuffer(64))
            {
                // Fill with data
                "Optimized Token Processing".AsSpan().CopyTo(pooledBuffer.GetSpan());
                
                // Pass the Memory<T> property to the async processor
                await ProcessTokenStreamAsync(pooledBuffer.Memory);
            }

            Console.WriteLine("\n--- 3. ReadOnlyMemory<T> for Input Safety ---");
            
            // 4. Using ReadOnlyMemory<T> for input parameters
            // When the callee doesn't need to modify the data, use ReadOnlyMemory<T>.
            // This prevents accidental modification and signals intent.
            ReadOnlyMemory<char> readOnlySource = "Read-Only Data".AsMemory();
            await InspectTokenStreamAsync(readOnlySource);
        }

        // ASYNC SAFE: Span<T> cannot be used here. Memory<T> is required.
        // This method simulates an asynchronous AI pipeline step (e.g., tokenization).
        static async Task ProcessTokenStreamAsync(Memory<char> data)
        {
            Console.WriteLine($"Processing: '{data}'");
            
            // Simulate I/O latency (e.g., waiting for a GPU or network)
            await Task.Delay(50); 

            // We can access the underlying Span for synchronous processing
            // within the async method safely.
            Span<char> span = data.Span;
            
            // Example: Convert to uppercase in-place (modifying the original buffer)
            for (int i = 0; i < span.Length; i++)
            {
                if (char.IsLower(span[i]))
                    span[i] = char.ToUpper(span[i]);
            }

            Console.WriteLine($"Result:    '{data}'");
        }

        static async Task InspectTokenStreamAsync(ReadOnlyMemory<char> data)
        {
            // Simulate reading data without modifying it
            await Task.Delay(20);
            
            // We can slice the ReadOnlyMemory without allocating new memory
            ReadOnlyMemory<char> slice = data.Slice(0, 4); // "Read"
            Console.WriteLine($"Inspected prefix: '{slice}'");
        }
    }
}
