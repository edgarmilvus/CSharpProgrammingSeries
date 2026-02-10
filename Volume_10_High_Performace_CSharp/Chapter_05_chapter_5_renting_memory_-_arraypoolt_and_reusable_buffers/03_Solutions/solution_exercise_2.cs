
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
using System.Buffers;

public class ReusableBuffer<T> : IDisposable
{
    private T[] _rentedArray;
    private int _count;
    private bool _disposed;

    public ReusableBuffer(int initialSize = 1024)
    {
        // Rent the initial buffer
        _rentedArray = ArrayPool<T>.Shared.Rent(initialSize);
        _count = 0;
    }

    public ReadOnlySpan<T> Data => _rentedArray.AsSpan(0, _count);

    // Requirement 3: Dynamic Resizing
    public void Write(ReadOnlySpan<T> data)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ReusableBuffer<T>));
        if (data.Length == 0) return;

        // Check if we need to resize
        if (_count + data.Length > _rentedArray.Length)
        {
            Grow(data.Length);
        }

        // Copy data into the buffer using Span (zero-copy efficient)
        data.CopyTo(_rentedArray.AsSpan(_count));
        _count += data.Length;
    }

    private void Grow(int requiredAdditionalCapacity)
    {
        // Calculate new size (double current length or fit required data)
        int newLength = Math.Max(_rentedArray.Length * 2, _rentedArray.Length + requiredAdditionalCapacity);
        
        // Rent a new, larger array
        T[] newArray = ArrayPool<T>.Shared.Rent(newLength);
        
        // Copy existing data
        _rentedArray.AsSpan(0, _count).CopyTo(newArray);

        // Return the old array to the pool
        // Important: Clear it if it contains sensitive data, though strictly not required for reference types
        ArrayPool<T>.Shared.Return(_rentedArray, clearArray: true);
        
        _rentedArray = newArray;
    }

    // Requirement 4: Disposal
    public void Dispose()
    {
        if (_disposed) return;
        
        if (_rentedArray != null)
        {
            ArrayPool<T>.Shared.Return(_rentedArray, clearArray: true);
            _rentedArray = null;
        }
        
        _disposed = true;
    }
}

// Interactive Challenge: Async Extension
public class AsyncReusableBuffer<T> : ReusableBuffer<T>
{
    // Note: ArrayPool operations are synchronous. 
    // This wrapper handles async data copying from a Stream.
    public async Task WriteFromStreamAsync(Stream stream, byte[] tempBuffer)
    {
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(tempBuffer, 0, tempBuffer.Length)) > 0)
        {
            // Assuming T is byte for stream copying, or we cast carefully.
            // For generic safety, we might need constraints, but for this exercise, we assume T is byte or compatible.
            // Using Span to slice the temp buffer
            Write(tempBuffer.AsSpan(0, bytesRead));
        }
    }
}
