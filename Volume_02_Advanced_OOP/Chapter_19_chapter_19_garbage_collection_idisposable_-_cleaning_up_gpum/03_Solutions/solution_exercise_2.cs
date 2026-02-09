
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Runtime.InteropServices;

// 1. Define the Delegate
public delegate void CleanupAction(IntPtr pointer);

public class DynamicTensor : IDisposable
{
    private IntPtr _devicePointer;
    private readonly CleanupAction _cleanupLogic;
    private bool _disposed = false;

    // 2. Accept the delegate in the constructor
    public DynamicTensor(int sizeInBytes, CleanupAction cleanupLogic)
    {
        _devicePointer = Marshal.AllocHGlobal(sizeInBytes);
        _cleanupLogic = cleanupLogic;
        Console.WriteLine($"Allocated {sizeInBytes} bytes using custom cleanup logic.");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Managed resources would go here
        }

        // 4. Invoke the delegate
        if (_devicePointer != IntPtr.Zero)
        {
            // Check if a delegate was actually provided to avoid NullReferenceException
            if (_cleanupLogic != null)
            {
                _cleanupLogic(_devicePointer);
            }
            _devicePointer = IntPtr.Zero;
        }

        _disposed = true;
    }

    public void PerformOperation() => Console.WriteLine("Performing matrix multiplication...");
}

public class Program
{
    public static void Main()
    {
        // 5. Use a Lambda Expression to define the cleanup behavior
        Console.WriteLine("--- Test 1: Standard Allocator ---");
        CleanupAction customCleanup = (ptr) => 
        {
            Console.WriteLine($"[LOG] Freeing memory at {ptr} via standard allocator...");
            Marshal.FreeHGlobal(ptr);
        };

        using (var tensor = new DynamicTensor(1024, customCleanup))
        {
            tensor.PerformOperation();
        }

        Console.WriteLine("\n--- Test 2: No-Op / Managed Allocator ---");
        // Example of a "No-Op" cleanup (e.g., for a tensor backed by managed memory)
        CleanupAction noOpCleanup = (ptr) => 
        {
            Console.WriteLine($"[LOG] Pointer {ptr} is managed by GC, no unmanaged free required.");
        };

        using (var managedTensor = new DynamicTensor(1024, noOpCleanup))
        {
            // Do work
        }
    }
}
