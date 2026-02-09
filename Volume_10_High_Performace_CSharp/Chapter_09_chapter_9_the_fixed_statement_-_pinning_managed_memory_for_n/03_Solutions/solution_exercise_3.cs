
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

public unsafe class StringProcessor
{
    public static void ProcessStringFixed(ReadOnlySpan<char> input)
    {
        // 1. Pin the span to get a stable pointer to the characters
        fixed (char* ptr = input)
        {
            // 2. Treat the char* as a byte* to process raw bytes
            byte* bytePtr = (byte*)ptr;
            
            // 3. Calculate byte length (char is 2 bytes)
            int byteLength = input.Length * sizeof(char);
            int vectorSize = Vector<byte>.Count;
            int i = 0;

            // 4. SIMD Loop
            // Ensure we have enough data for a full vector
            while (i <= byteLength - vectorSize)
            {
                // Read unaligned bytes into a Vector
                Vector<byte> vector = Unsafe.ReadUnaligned<Vector<byte>>(bytePtr + i);
                
                // Example Token Detection: Check for a specific byte pattern (e.g., 0x00)
                // In a real scenario, this would be complex logic
                if (Vector<byte>.IsHardwareAccelerated)
                {
                    // Just a dummy operation to demonstrate SIMD usage
                    Vector<byte> zero = Vector<byte>.Zero;
                    if (Vector.EqualsAll(vector, zero))
                    {
                        // Found null bytes (rare in valid UTF-16 strings unless padding)
                    }
                }
                
                i += vectorSize;
            }

            // 5. Handle Remainder (Tail)
            // Process remaining bytes that don't fit in a vector
            for (; i < byteLength; i++)
            {
                byte b = bytePtr[i];
                // Process single byte
            }
        }
    }

    // Helper for testing
    public static void RunTest()
    {
        string test = "Hello SIMD World!";
        // Convert to UTF-8 bytes for demonstration context (though we process chars here)
        ProcessStringFixed(test.AsSpan());
    }
}
