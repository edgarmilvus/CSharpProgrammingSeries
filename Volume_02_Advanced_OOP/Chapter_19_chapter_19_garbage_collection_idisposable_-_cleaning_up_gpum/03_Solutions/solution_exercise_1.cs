
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Runtime.InteropServices;

public class GpuTensorBuffer : IDisposable
{
    private IntPtr _devicePointer;
    private readonly int _sizeInBytes;
    private bool _disposed = false;

    public GpuTensorBuffer(int sizeInBytes)
    {
        _sizeInBytes = sizeInBytes;
        // Simulate GPU memory allocation
        _devicePointer = Marshal.AllocHGlobal(sizeInBytes);
        Console.WriteLine($"Allocated {_sizeInBytes} bytes at {_devicePointer}.");
    }

    // Finalizer: The safety net for unmanaged resources
    ~GpuTensorBuffer()
    {
        // We pass 'false' to indicate we are being called from the finalizer,
        // not from explicit user code.
        Dispose(false);
    }

    // Public Dispose method (IDisposable implementation)
    public void Dispose()
    {
        // Pass 'true' to indicate this is a deterministic disposal call
        Dispose(true);
        
        // Suppress the finalizer to prevent the GC from calling it later
        // (since we've already cleaned up).
        GC.SuppressFinalize(this);
    }

    // Protected virtual method implementing the Standard Dispose Pattern
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Free managed resources here.
            // This block is only executed if called via Dispose() or 'using'.
            // Example: If this class held a FileStream, we would call _fileStream.Dispose() here.
            // In this specific case, we have no managed resources to clean up.
        }

        // Free unmanaged resources.
        // This block is executed in BOTH cases (Dispose() and Finalizer).
        if (_devicePointer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_devicePointer);
            Console.WriteLine($"Freed {_sizeInBytes} bytes at {_devicePointer}.");
            _devicePointer = IntPtr.Zero; // Prevent double-free
        }

        _disposed = true;
    }
}

public class Program
{
    public static void Main()
    {
        // Scenario A: Explicit Disposal (Good practice)
        Console.WriteLine("--- Scenario A: Explicit Disposal ---");
        using (var buffer = new GpuTensorBuffer(1024))
        {
            Console.WriteLine("Buffer in use.");
        } // Dispose() is called automatically here

        // Scenario B: Implicit Disposal (Simulating a leak handled by Finalizer)
        Console.WriteLine("\n--- Scenario B: Implicit Disposal (GC) ---");
        var leakyBuffer = new GpuTensorBuffer(2048);
        leakyBuffer = null; 
        
        // Force Garbage Collection to trigger the finalizer
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        Console.WriteLine("GC Collection complete.");
    }
}
