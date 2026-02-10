
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Runtime.InteropServices;

public static class UnmanagedBridge
{
    public static unsafe void ProcessUnmanagedBuffer(IntPtr ptr, int length)
    {
        // Safety Check: Ensure the pointer is not null
        if (ptr == IntPtr.Zero || length <= 0)
        {
            return;
        }

        // Approach: Using Span<T> to wrap unmanaged memory directly.
        // This avoids the allocation and copy overhead of Marshal.Copy.
        
        // CRITICAL SAFETY CONTEXT:
        // We are casting an IntPtr to a void* and creating a Span over it.
        // While Span<T> is memory-safe, wrapping unmanaged memory requires 
        // ensuring the memory remains valid (allocated) for the duration of the Span's usage.
        // If the unmanaged memory is freed (e.g., via Marshal.FreeHGlobal) while the 
        // Span is active, accessing it will cause undefined behavior (access violation).
        
        unsafe
        {
            // Cast IntPtr to void* and create the Span
            Span<float> buffer = new Span<float>((void*)ptr, length);

            // Modify the data in place
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] *= 2.0f;
            }
        }
    }
}
