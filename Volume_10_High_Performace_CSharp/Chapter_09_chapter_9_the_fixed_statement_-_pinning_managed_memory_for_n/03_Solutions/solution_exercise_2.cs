
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ParticleData
{
    // Map fields to specific byte offsets
    [FieldOffset(0)] public float X;
    [FieldOffset(4)] public float Y;
    [FieldOffset(8)] public float Z;
    
    [FieldOffset(12)] public float VelocityX;
    [FieldOffset(16)] public float VelocityY;
    [FieldOffset(20)] public float VelocityZ;

    // Fixed buffer overlaying the same memory region
    // Note: In C#, fixed buffers cannot be public in some contexts, 
    // but we can expose the pointer via a method.
    [FieldOffset(0)] private fixed float _buffer[6];

    public float* GetPointerToData(ref ParticleData data)
    {
        // Pin the struct to get a stable address and return a pointer to the buffer
        // Note: 'fixed' here pins the struct on the stack (if local) or heap (if field).
        fixed (ParticleData* ptr = &data)
        {
            return ptr->_buffer;
        }
    }
}

public class StructTest
{
    public static unsafe void Test()
    {
        ParticleData p = new ParticleData();
        
        // Get direct pointer access
        // Note: We cannot use 'fixed ParticleData p;' directly on a struct variable 
        // unless it contains a managed pointer (which this struct doesn't).
        
        // However, to demonstrate the overlay:
        ParticleData* pPtr = (ParticleData*)Unsafe.AsPointer(ref p);
        
        // Access via fields
        p.X = 1.0f;
        p.VelocityX = 5.0f;

        // Access via buffer pointer
        float* buffer = (float*)pPtr;
        
        Console.WriteLine($"Field X: {p.X}, Buffer[0]: {buffer[0]}"); // Should be 1.0
        Console.WriteLine($"Field VelX: {p.VelocityX}, Buffer[3]: {buffer[3]}"); // Should be 5.0
        
        // Verify they match
        if (buffer[0] == p.X && buffer[3] == p.VelocityX)
        {
            Console.WriteLine("Overlay verification successful.");
        }
    }
}
