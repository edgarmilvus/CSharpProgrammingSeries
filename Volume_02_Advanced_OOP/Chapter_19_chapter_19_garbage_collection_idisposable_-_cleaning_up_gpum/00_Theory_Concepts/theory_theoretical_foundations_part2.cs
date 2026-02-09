
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System;

public class GpuTensor : IDisposable
{
    // Pointer to unmanaged GPU memory
    private IntPtr _devicePointer;
    private bool _disposed = false;

    // Constructor allocates unmanaged memory
    public GpuTensor(int sizeInBytes)
    {
        // Simulate allocation (e.g., cudaMalloc)
        _devicePointer = Marshal.AllocHGlobal(sizeInBytes); 
        Console.WriteLine($"Allocated {sizeInBytes} bytes at {_devicePointer}");
    }

    // Public Dispose method - the deterministic cleanup path
    public void Dispose()
    {
        Dispose(true);
        // We are explicitly disposing, so we don't need the finalizer to run
        GC.SuppressFinalize(this);
    }

    // Protected virtual method containing the actual cleanup logic
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Managed resources cleanup:
            // If this class held references to other IDisposable objects (e.g., a file stream),
            // we would call their Dispose() methods here.
            // Example: _logStream?.Dispose();
        }

        // Unmanaged resources cleanup:
        // This runs regardless of whether Dispose() or the finalizer called it.
        if (_devicePointer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_devicePointer); // Simulate cudaFree
            _devicePointer = IntPtr.Zero;
            Console.WriteLine($"Freed GPU memory at {_devicePointer}");
        }

        _disposed = true;
    }

    // Finalizer (Destructor syntax)
    // This acts as a safety net if Dispose() is never called.
    ~GpuTensor()
    {
        // The GC calls this, but we cannot access managed objects here safely.
        Dispose(false);
    }
}
