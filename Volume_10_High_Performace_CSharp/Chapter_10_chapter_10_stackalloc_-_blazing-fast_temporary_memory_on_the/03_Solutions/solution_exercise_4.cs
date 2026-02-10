
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Runtime.InteropServices;

public static class SpanReinterpreter
{
    public static Span<T> ReinterpretStackBuffer<T>(int byteCount) where T : unmanaged
    {
        // 1. Safety Check: Ensure byteCount is a multiple of sizeof(T)
        int typeSize = Unsafe.SizeOf<T>();
        if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
        if (byteCount % typeSize != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), 
                $"The byte count ({byteCount}) must be a multiple of the size of {typeof(T).Name} ({typeSize}).");
        }

        // 2. Allocate on the stack
        // Note: If byteCount is large, this will throw StackOverflowException.
        // Caller must ensure reasonable sizes.
        Span<byte> byteSpan = stackalloc byte[byteCount];

        // 3. Reinterpret the bytes as T
        // MemoryMarshal.Cast is the high-performance way to reinterpret spans
        return MemoryMarshal.Cast<byte, T>(byteSpan);
    }

    // Example usage simulation
    public static void SimulateVector3Processing()
    {
        // 24 bytes = 3 * 8 bytes (assuming Vector3 is 12 bytes on x64, 
        // but let's stick to the requirement of 24 bytes for the example).
        // Actually, System.Numerics.Vector3 is typically 12 bytes (3 floats).
        // Let's simulate a custom struct or assume the user wants to fit 2 Vector3s (24 bytes).
        
        try 
        {
            // We want to interpret 24 bytes. 
            // Let's check Vector3 size: 3 floats * 4 = 12 bytes.
            // 24 bytes / 12 bytes = 2 elements.
            int byteCount = 24;
            Span<Vector3> vectors = ReinterpretStackBuffer<Vector3>(byteCount);

            // Now we can treat the stack memory as Vector3s
            vectors[0] = new Vector3(1.0f, 2.0f, 3.0f);
            vectors[1] = new Vector3(4.0f, 5.0f, 6.0f);

            // Verify
            Console.WriteLine($"Vector 1: {vectors[0]}");
            Console.WriteLine($"Vector 2: {vectors[1]}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
