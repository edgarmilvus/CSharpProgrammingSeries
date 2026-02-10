
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
using System.Buffers;
using System.Runtime.InteropServices;

public class PooledMemoryManager<T> : MemoryManager<T>
{
    private T[] _array;
    private readonly int _length;
    private GCHandle _pinnedHandle;
    private bool _isPinned;

    public PooledMemoryManager(int length)
    {
        _length = length;
        _array = ArrayPool<T>.Shared.Rent(length);
    }

    public override Span<T> GetSpan()
    {
        return _array.AsSpan(0, _length);
    }

    public override Memory<T> Memory => CreateMemory(_length);

    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if (_isPinned)
            throw new InvalidOperationException("Memory is already pinned.");

        if (elementIndex < 0 || elementIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));

        // Pin the array to get a stable pointer
        _pinnedHandle = GCHandle.Alloc(_array, GCHandleType.Pinned);
        _isPinned = true;

        unsafe
        {
            // Get the pointer to the start of the array (plus offset)
            void* pointer = (void*)((IntPtr)_pinnedHandle.AddrOfPinnedObject() + elementIndex * sizeof(T));
            
            // Return a MemoryHandle that knows how to unpin itself
            return new MemoryHandle(pointer, default, this);
        }
    }

    public override void Unpin()
    {
        if (_isPinned)
        {
            _pinnedHandle.Free();
            _isPinned = false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_array != null)
        {
            Unpin(); // Ensure unpinned before returning
            ArrayPool<T>.Shared.Return(_array);
            _array = null;
        }
    }
}

// Test Method
public static class MemoryManagerTest
{
    public static void Run()
    {
        using var manager = new PooledMemoryManager<int>(1024);
        
        // Get the span to perform SIMD-like operations
        Span<int> span = manager.GetSpan();
        
        // Populate with data
        for(int i=0; i<span.Length; i++) span[i] = i;

        // Simulate a SIMD operation (Vector<T> requires specific hardware support, 
        // but we can demonstrate the logic conceptually)
        // Here we just do a simple scalar addition for the example
        for (int i = 0; i < span.Length; i++)
        {
            span[i] += 10;
        }

        Console.WriteLine($"First element after operation: {span[0]}");
        // Disposal happens automatically via 'using', returning array to pool
    }
}
