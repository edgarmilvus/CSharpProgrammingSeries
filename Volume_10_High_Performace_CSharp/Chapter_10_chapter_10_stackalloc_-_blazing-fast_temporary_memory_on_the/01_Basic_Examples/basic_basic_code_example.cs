
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public unsafe class StackallocTokenizer
{
    public static void Main()
    {
        // 1. Simulate a raw input buffer (e.g., from a network stream).
        // In a real scenario, this might be pinned or unmanaged memory.
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes("ID:00123456789012345678");

        // 2. Process the data.
        // We wrap the raw data in a ReadOnlySpan to avoid copying.
        ProcessTokenData(rawData);
    }

    private static unsafe void ProcessTokenData(ReadOnlySpan<byte> input)
    {
        // 3. Define the size of the temporary buffer we need.
        // We are looking for a 16-byte GUID-like ID at the end of the string.
        const int TokenLength = 16;

        // 4. Allocate memory on the stack.
        // 'stackalloc' creates a block of memory of type byte* (pointer to byte).
        // This memory is NOT managed by the Garbage Collector.
        byte* tempBuffer = stackalloc byte[TokenLength];

        // 5. Create a Span<T> over the stack memory.
        // Span<T> provides a safe, bounds-checked view over the pointer.
        // This is crucial for preventing buffer overruns.
        Span<byte> tokenSpan = new Span<byte>(tempBuffer, TokenLength);

        // 6. Extract the token from the input into the stack buffer.
        // We assume the ID starts at index 4 ("ID:0" -> index 4 is '0').
        // We use a slice of the input to target the specific segment.
        input.Slice(4, TokenLength).CopyTo(tokenSpan);

        // 7. Process the token using SIMD (if available).
        // We are going to calculate a simple checksum (XOR of all bytes) 
        // using Vector<T> to demonstrate high-speed processing.
        ProcessWithSimd(tokenSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ProcessWithSimd(Span<byte> tokenSpan)
    {
        // Pin the span to get a pointer for SIMD operations.
        // This ensures the garbage collector doesn't move the memory (though stack memory isn't moved anyway,
        // pinning is required for certain low-level API interactions).
        fixed (byte* ptr = tokenSpan)
        {
            // Check for AVX2 support (256-bit vectors).
            if (Avx2.IsSupported)
            {
                // 8. Vectorization Setup.
                // We process 32 bytes at a time (256 bits / 8 bits per byte).
                int i = 0;
                int length = tokenSpan.Length;
                
                // Initialize a vector accumulator for the XOR operation.
                Vector256<byte> accumulator = Vector256<byte>.Zero;

                // 9. Process chunks of 32 bytes.
                for (; i <= length - 32; i += 32)
                {
                    // Load 32 bytes from memory into a vector register.
                    Vector256<byte> data = Avx.LoadVector256(ptr + i);
                    
                    // Perform XOR operation across the vector lanes.
                    accumulator = Avx2.Xor(accumulator, data);
                }

                // 10. Horizontal XOR (Scalar Cleanup).
                // Since XOR is associative, we can XOR the accumulator parts together 
                // to get a single byte result representing the checksum.
                byte checksum = 0;
                for (int j = 0; j < 32; j++)
                {
                    checksum ^= accumulator.GetElement(j);
                }

                // 11. Process remaining bytes (tail).
                for (; i < length; i++)
                {
                    checksum ^= ptr[i];
                }

                Console.WriteLine($"SIMD Checksum: {checksum:X}");
            }
            else
            {
                // Fallback scalar implementation.
                byte checksum = 0;
                for (int i = 0; i < tokenSpan.Length; i++)
                {
                    checksum ^= ptr[i];
                }
                Console.WriteLine($"Scalar Checksum: {checksum:X}");
            }
        }
    }
}
