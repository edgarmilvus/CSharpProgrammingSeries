
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

public struct Point
{
    public int X;
    public int Y;
}

public class MemoryVisualizer
{
    public void InspectMemory(object o)
    {
        // When 'o' is a boxed Point:
        // Memory Layout on Heap:
        // [Object Header: 12-16 bytes (Method Table Pointer + Sync Block Index)]
        // [Point Data: 8 bytes (two ints)]
        // [Padding to align to pointer size]

        int headerSize = IntPtr.Size * 2; // Approx overhead (Pointer size * 2)
        int dataSize = Marshal.SizeOf(typeof(Point));
        int totalSize = headerSize + dataSize;

        Console.WriteLine($"Object Type: {o.GetType()}");
        Console.WriteLine($"Data Size: {dataSize} bytes");
        Console.WriteLine($"Object Header Overhead: {headerSize} bytes");
        Console.WriteLine($"Total Heap Allocation: {totalSize} bytes");
    }
}

public class Program
{
    public static void Main()
    {
        Point p = new Point { X = 10, Y = 20 };
        MemoryVisualizer viz = new MemoryVisualizer();

        Console.WriteLine("--- Value Type (Stack) ---");
        Console.WriteLine($"Struct 'p' is on the stack. Size: {Marshal.SizeOf(p)} bytes (approx).");
        
        Console.WriteLine("\n--- Boxed Type (Heap) ---");
        // Boxing happens here: a new heap object is created
        object boxedP = p;
        viz.InspectMemory(boxedP);
        
        Console.WriteLine("\nConclusion: Boxing adds a ~16 byte header to the data.");
    }
}
