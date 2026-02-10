
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
using System.Runtime.InteropServices;

public class PinnedMemoryDemo
{
    public static void Main()
    {
        // REAL-WORLD CONTEXT:
        // Imagine you are building a high-performance AI inference engine in C#.
        // You need to pass a tensor of floating-point values (e.g., 1024 floats)
        // to a native C++ library (like ONNX Runtime or a custom CUDA kernel) for matrix multiplication.
        // Copying this data from the managed heap to an unmanaged buffer is expensive
        // and introduces latency. We want to pass a pointer directly to the memory
        // where the data lives, but we must ensure the Garbage Collector (GC)
        // doesn't move the memory while the native code is accessing it.

        Console.WriteLine("--- Basic 'fixed' Statement Example ---");
        RunFixedStatementExample();

        Console.WriteLine("\n--- Pinned Object Handle (GCHandle) Example ---");
        RunGCHandleExample();
    }

    static void RunFixedStatementExample()
    {
        // 1. Allocate a managed array on the heap.
        // The GC is free to move this array in memory during a collection cycle.
        float[] tensorData = new float[8] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };

        // 2. Use the 'fixed' statement.
        // This pins the 'tensorData' array in memory, preventing the GC from relocating it.
        // It returns a 'float*' pointer that is valid only within the fixed block.
        // The 'using' pattern is used here to ensure the pin is released automatically
        // (via the 'fixed' statement's scope exit), even if an exception occurs.
        using (var pinHandle = new PinnedArray<float>(tensorData))
        {
            // 3. Access the pointer.
            // We can now safely pass 'pinHandle.Pointer' to native APIs or perform
            // low-level pointer arithmetic.
            unsafe
            {
                float* ptr = pinHandle.Pointer;

                Console.WriteLine($"Array pinned at address: {(IntPtr)ptr:X}");

                // Simulate native processing: Read values via pointer
                // (e.g., passing 'ptr' to a C++ function).
                for (int i = 0; i < tensorData.Length; i++)
                {
                    // Dereferencing the pointer to read the value.
                    Console.Write(*(ptr + i) + " ");
                }
                Console.WriteLine();
            }
        }
        // 4. The 'using' block ends here.
        // The pin is released. The GC is now free to move 'tensorData' again if needed.
    }

    static void RunGCHandleExample()
    {
        // 1. Create a complex object (struct) that contains a fixed-size buffer.
        // Note: 'fixed' buffers can only be used in 'unsafe' contexts and 'struct' types.
        MyStruct dataStruct = new MyStruct();
        dataStruct.Initialize();

        // 2. Pin the object using GCHandle.
        // This is useful when the object is complex or when you need to pin
        // an object for a longer duration outside of a specific code block scope.
        GCHandle handle = GCHandle.Alloc(dataStruct, GCHandleType.Pinned);

        try
        {
            // 3. Get the address of the struct.
            IntPtr address = handle.AddrOfPinnedObject();

            // 4. Cast to a pointer to access the fixed buffer inside the struct.
            unsafe
            {
                // We know the layout of MyStruct. We access the buffer directly.
                // Note: In a real scenario, we might use Marshal.PtrToStructure or
                // pointer casting if the struct is blittable.
                MyStruct* ptr = (MyStruct*)address;
                
                Console.WriteLine($"Struct pinned at address: {address:X}");
                
                // Access the fixed buffer inside the struct
                // The buffer is named 'internalBuffer' in the struct definition.
                for (int i = 0; i < 4; i++)
                {
                    Console.Write(ptr->internalBuffer[i] + " ");
                }
                Console.WriteLine();
            }
        }
        finally
        {
            // 5. CRITICAL: Always free the GCHandle.
            // If you forget this, the object remains pinned permanently (until app exit),
            // causing memory fragmentation and preventing the GC from optimizing memory.
            handle.Free();
        }
    }
}

// Helper wrapper to mimic the 'using' pattern for 'fixed' blocks
// This is a common pattern to make pinning safer and more readable.
public sealed class PinnedArray<T> : IDisposable where T : unmanaged
{
    private GCHandle _handle;
    public unsafe T* Pointer { get; }

    public PinnedArray(T[] array)
    {
        _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        unsafe
        {
            Pointer = (T*)_handle.AddrOfPinnedObject();
        }
    }

    public void Dispose()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
    }
}

// Example struct containing a fixed buffer
// 'unsafe' is required to declare a struct with a fixed buffer field.
public unsafe struct MyStruct
{
    // 'fixed' creates a buffer of a specific size inline within the struct.
    // This is useful for small, fixed-size data blocks required by native APIs.
    public fixed float internalBuffer[4];

    public void Initialize()
    {
        // We cannot use standard array initialization syntax inside a fixed buffer.
        // We must assign values individually or use pointer arithmetic.
        // This method is a helper to populate the buffer for the example.
        fixed (float* ptr = internalBuffer)
        {
            ptr[0] = 10.0f;
            ptr[1] = 20.0f;
            ptr[2] = 30.0f;
            ptr[3] = 40.0f;
        }
    }
}
