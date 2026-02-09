
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
using System.Buffers;
using System.Numerics; // Required for Vector<T> (SIMD)
using System.Runtime.CompilerServices;
using System.Text;

namespace HighPerformanceTokenization
{
    public static class ZeroAllocProcessor
    {
        // Configuration for our SIMD hardware acceleration
        private const int VectorSize = 64; // AVX2 register size (256 bits / 8 bytes = 32, but 64 is safe for 512-bit future proofing)
        private const int TokenThreshold = 5; // Minimum length of a token to consider

        public static void ProcessHeadlines()
        {
            // 1. INPUT: Simulating a raw buffer of text (e.g., from a network stream).
            // In a real AI scenario, this would be a massive Tensor buffer or memory-mapped file.
            string rawText = "Buy NVDA now! Price target $1000. #AI Boom!!!";
            
            // 2. STACK ALLOCATION: Using 'stackalloc' to create a buffer on the Stack.
            // CRITICAL: Stack memory is NOT garbage collected. It is instantly freed when the scope ends.
            // This avoids the Heap allocation cost of 'new byte[]'.
            Span<byte> buffer = stackalloc byte[rawText.Length * 2]; // UTF-8 encoding might be larger
            
            // 3. ZERO-ALLOCATION ENCODING: Convert String to Bytes without creating a heap string.
            // We write directly into our stack-allocated Span.
            int bytesWritten = Encoding.UTF8.GetBytes(rawText.AsSpan(), buffer);
            
            // 4. SLICING: Create a view of the valid data.
            // This is a 'struct', so it copies by reference, not by value. Zero allocation.
            Span<byte> validData = buffer.Slice(0, bytesWritten);

            // 5. PROCESSING LOOP: Tokenize and Hash using SIMD.
            Console.WriteLine($"Processing: '{rawText}'");
            Console.WriteLine("--------------------------------------------------");
            
            // We use a manual loop. Standard LINQ is forbidden here because it boxes structs
            // and allocates delegates, destroying performance.
            int tokenCount = 0;
            long cumulativeSimdHash = 0;
            
            // State machine for tokenization
            int start = -1;
            
            for (int i = 0; i < validData.Length; i++)
            {
                byte b = validData[i];
                bool isLetterOrDigit = (b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9');

                if (isLetterOrDigit)
                {
                    if (start == -1) start = i;
                }
                else if (start != -1)
                {
                    // Found a token boundary
                    int length = i - start;
                    if (length >= TokenThreshold)
                    {
                        // 6. SIMD MAGIC: Process the token slice.
                        // We pass the specific slice of the token to our vectorized hasher.
                        // This function processes 64 bytes at a time using CPU registers.
                        long tokenHash = ComputeSimdHash(validData.Slice(start, length));
                        
                        cumulativeSimdHash += tokenHash;
                        tokenCount++;
                        
                        // Print for demonstration
                        Console.WriteLine($"Token: {validData.Slice(start, length).ToString()} | Hash: {tokenHash:X}");
                    }
                    start = -1; // Reset
                }
            }

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Total Tokens: {tokenCount}");
            Console.WriteLine($"Cumulative SimdHash: {cumulativeSimdHash:X}");
        }

        // 7. HARDWARE ACCELERATION: The core vectorized operation.
        // This method performs a mathematical operation on multiple bytes simultaneously.
        private static long ComputeSimdHash(Span<byte> token)
        {
            // If the hardware doesn't support Vector<T>, we fallback (though modern AI PCs do).
            if (!Vector.IsHardwareAccelerated || token.Length < Vector<byte>.Count)
            {
                // Simple fallback loop
                long hash = 0;
                foreach (byte b in token) hash += b;
                return hash;
            }

            // 8. VECTORIZATION: We treat the byte span as a Vector.
            // The CPU loads 32 or 64 bytes into a single register and adds them in ONE instruction.
            // This is exponentially faster than a 'for' loop.
            Vector<byte> accumulator = Vector<byte>.Zero;
            int i = 0;
            int vectorSize = Vector<byte>.Count;

            // Process full vectors
            while (i <= token.Length - vectorSize)
            {
                Vector<byte> chunk = new Vector<byte>(token.Slice(i, vectorSize));
                accumulator = Vector.Add(accumulator, chunk); // SIMD Addition
                i += vectorSize;
            }

            // Reduce the vector to a single value
            long sum = 0;
            for (; i < token.Length; i++)
            {
                sum += token[i];
            }

            // Add the vector remainder
            for (int j = 0; j < vectorSize; j++)
            {
                sum += accumulator[j];
            }

            return sum;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ZeroAllocProcessor.ProcessHeadlines();
        }
    }
}
