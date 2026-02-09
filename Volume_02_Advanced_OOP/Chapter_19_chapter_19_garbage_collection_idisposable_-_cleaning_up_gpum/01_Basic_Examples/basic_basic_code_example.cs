
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

// A mock class to simulate holding a valuable, limited resource like GPU memory.
// We implement IDisposable to signal that this class manages unmanaged resources.
public class GpuTensorBuffer : IDisposable
{
    private bool _isDisposed = false; // Tracks if Dispose() has been called
    private readonly int _bufferId;   // Simulates a handle to a GPU memory block

    public GpuTensorBuffer(int sizeInBytes)
    {
        _bufferId = new Random().Next(1000, 9999); // Simulate a unique GPU memory handle
        Console.WriteLine($"[Alloc] VRAM Block #{_bufferId} allocated ({sizeInBytes} bytes).");
    }

    public void PerformCalculation()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("GpuTensorBuffer", "Cannot perform calculation on disposed buffer.");
        }
        Console.WriteLine($"[Compute] Using VRAM Block #{_bufferId} for matrix multiplication...");
    }

    // The core of deterministic cleanup.
    public void Dispose()
    {
        // Call the internal cleanup method with 'true' to indicate explicit disposal.
        Dispose(true);
        
        // Tell the GC that we don't need the Finalizer anymore; we've already cleaned up.
        GC.SuppressFinalize(this);
    }

    // Protected virtual method allowing derived classes to clean up their own resources.
    protected virtual void Dispose(bool isDisposing)
    {
        if (_isDisposed) return; // Idempotency: multiple calls are safe.

        if (isDisposing)
        {
            // IMPORTANT: Only clean up managed resources here (other IDisposables).
            // We don't have any in this simple example, but if we held a Logger or Stream, we'd close it here.
            Console.WriteLine($"[Free] VRAM Block #{_bufferId} released immediately via Dispose().");
        }
        
        // Always clean up unmanaged resources (simulated here by the _bufferId).
        // This block runs whether Dispose() is called explicitly or by the Finalizer.
        // In a real scenario, this is where you call cudaFree() or clReleaseMemObject().
        
        _isDisposed = true;
    }

    // The Finalizer (Destructor) acts as a safety net.
    // If the user forgets to call Dispose(), this will eventually run.
    ~GpuTensorBuffer()
    {
        // We pass 'false' because we are in the GC thread; we cannot safely touch other managed objects.
        Dispose(false);
        Console.WriteLine($"[Warning] VRAM Block #{_bufferId} released via Finalizer. You forgot to use 'using'!");
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("--- Simulation Start ---");

        // The 'using' statement is syntactic sugar for a try/finally block.
        // It guarantees that .Dispose() is called when the scope is exited.
        using (var tensor = new GpuTensorBuffer(1024 * 1024)) // 1 MB
        {
            tensor.PerformCalculation();
        } // <--- tensor.Dispose() is called automatically here.

        Console.WriteLine("\n--- Simulation End: Scope Exited ---");

        // Now, let's demonstrate what happens if we DON'T use 'using'.
        Console.WriteLine("\n--- Negligence Simulation ---");
        CreateAndForget();
        
        // We force a Garbage Collection to prove the Finalizer runs eventually.
        // Note: In production, you should never rely on GC.Collect()!
        Console.WriteLine("\n[System] Forcing Garbage Collection...");
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        Console.WriteLine("--- Final State ---");
    }

    // This helper method simulates a common coding mistake.
    public static void CreateAndForget()
    {
        var lostTensor = new GpuTensorBuffer(2048);
        lostTensor.PerformCalculation();
        // We exit the method here.
        // 'lostTensor' goes out of scope, but we never called Dispose().
        // It becomes "Garbage" waiting for the GC to find it.
    }
}
