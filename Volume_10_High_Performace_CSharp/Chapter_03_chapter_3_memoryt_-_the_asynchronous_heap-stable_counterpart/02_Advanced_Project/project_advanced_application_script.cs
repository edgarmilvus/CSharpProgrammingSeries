
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
using System.Threading.Tasks;

namespace HighPerformanceAI.MemoryManagement
{
    /// <summary>
    /// Simulates an AI Tokenizer that reads raw text and converts it into integer tokens.
    /// This class demonstrates how Memory<T> allows us to pass buffers between methods
    /// without copying data, even in asynchronous contexts.
    /// </summary>
    public class Tokenizer
    {
        // A simple dictionary mapping characters to tokens (for simulation purposes).
        // In a real scenario, this would be a complex BPE (Byte Pair Encoding) model.
        // We use arrays as a basic data structure to store our vocabulary.
        private readonly char[] _vocabChars = { 'H', 'e', 'l', 'o', ' ', 'W', 'r', 'd', '!' };
        private readonly int[] _vocabTokens = { 101, 102, 103, 104, 105, 106, 107, 108, 109 };

        /// <summary>
        /// Asynchronously processes a chunk of text into tokens.
        /// Uses ReadOnlyMemory<char> as input to avoid allocating strings for substrings.
        /// </summary>
        /// <param name="textChunk">The memory slice containing the text to process.</param>
        /// <returns>A task representing the operation, returning the count of tokens found.</returns>
        public async Task<int> ProcessChunkAsync(ReadOnlyMemory<char> textChunk)
        {
            // We cannot use 'await' in a synchronous method, so we simulate I/O latency.
            // This mimics a real-world scenario where tokenization might involve 
            // checking a remote model or a heavy computation.
            await Task.Delay(50); 

            int tokenCount = 0;

            // CRITICAL: We access the Memory via the Span property.
            // Span<T> is used for synchronous, stack-allocated access to the data.
            // This is zero-copy; we are reading directly from the original buffer.
            ReadOnlySpan<char> span = textChunk.Span;

            for (int i = 0; i < span.Length; i++)
            {
                char c = span[i];

                // Basic lookup logic (simulating tokenization)
                for (int j = 0; j < _vocabChars.Length; j++)
                {
                    if (_vocabChars[j] == c)
                    {
                        tokenCount++;
                        // In a real app, we would write the token ID (e.g., _vocabTokens[j])
                        // to an output buffer here.
                        break;
                    }
                }
            }

            return tokenCount;
        }
    }

    /// <summary>
    /// A custom memory provider using MemoryManager<T>.
    /// This is useful when you want to integrate a custom memory source (like a pooled buffer)
    /// into the .NET Memory ecosystem without copying data.
    /// </summary>
    public class PooledTextBuffer : MemoryManager<char>
    {
        private readonly char[] _buffer;
        private readonly int _length;

        public PooledTextBuffer(string text)
        {
            // Simulate pooling by renting an array.
            _buffer = text.ToCharArray();
            _length = _buffer.Length;
        }

        public override Memory<char> Memory => _buffer.AsMemory(0, _length);

        public override Span<char> GetSpan() => _buffer.AsSpan(0, _length);

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            // Pins the underlying array so it won't be moved by the GC.
            // This is crucial for high-performance interop or specific I/O operations.
            return new MemoryHandle(_buffer, elementIndex);
        }

        public override void Unpin()
        {
            // No-op for array-backed memory, but required for the abstraction.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Return the array to the pool (simulated).
                Array.Clear(_buffer, 0, _buffer.Length);
            }
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== High-Performance AI Token Processing ===\n");

            // 1. SETUP: Create a large text stream simulation.
            // In a real AI pipeline, this might be a stream from a file or network.
            string fullText = "Hello World! Hello World! Hello World!";
            Console.WriteLine($"Input Text: '{fullText}'");

            // 2. MEMORY MANAGEMENT: Use a custom MemoryManager to wrap our text.
            // This avoids creating a new string for every substring operation.
            using (var pooledBuffer = new PooledTextBuffer(fullText))
            {
                var tokenizer = new Tokenizer();
                int totalTokens = 0;

                // 3. SLICING: Process the stream in chunks using Slicing.
                // We define a window size (chunk size).
                int chunkSize = 12;

                Console.WriteLine($"\nProcessing in chunks of {chunkSize} characters...\n");

                // 4. LOOP: Iterate through the buffer without copying data.
                for (int start = 0; start < fullText.Length; start += chunkSize)
                {
                    // Calculate the end of the slice, ensuring we don't go out of bounds.
                    int length = Math.Min(chunkSize, fullText.Length - start);

                    // SLICE OPERATION:
                    // Memory<T>.Slice() returns a new Memory<T> instance that points to the
                    // same underlying data but with a different offset and length.
                    // This is extremely cheap (O(1)) compared to string.Substring (O(N)).
                    ReadOnlyMemory<char> chunk = pooledBuffer.Memory.Slice(start, length);

                    // Display the slice info
                    Console.WriteLine($"[Dispatch] Processing Slice [{start}..{start + length - 1}]");

                    // 5. ASYNC PROCESSING: Pass the Memory to the async tokenizer.
                    // Because Memory<T> is heap-safe, it can cross await boundaries safely.
                    // Span<T> cannot be used here because it is stack-ref and cannot be 
                    // guaranteed to be valid after an 'await'.
                    int chunkTokens = await tokenizer.ProcessChunkAsync(chunk);
                    totalTokens += chunkTokens;

                    Console.WriteLine($"[Result]   Tokens found: {chunkTokens}");
                }

                Console.WriteLine($"\nTotal Tokens Processed: {totalTokens}");
            }

            Console.WriteLine("\nProcessing Complete.");
        }
    }
}
