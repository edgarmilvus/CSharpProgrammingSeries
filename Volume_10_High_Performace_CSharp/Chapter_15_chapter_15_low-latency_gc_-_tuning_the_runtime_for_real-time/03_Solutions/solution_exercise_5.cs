
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class HeapVisualizer
{
    public void RunAllocationTest()
    {
        Console.WriteLine("Starting Allocation Test...");

        // 1. Allocate Small Objects (< 85KB)
        // These go to Gen 0 initially.
        var smallObjects = new object[1000];
        for (int i = 0; i < 1000; i++)
        {
            smallObjects[i] = new byte[100]; // 100 bytes
        }

        // 2. Allocate Medium Objects (Just under LOH threshold)
        // 85,000 bytes is roughly the threshold.
        // Array overhead is ~24 bytes, so 85,000 - 24 = 84,976.
        var mediumObjects = new object[10];
        for (int i = 0; i < 10; i++)
        {
            mediumObjects[i] = new byte[84000]; // Still Gen 0/1/2, not LOH
        }

        // 3. Allocate Large Objects (> 85KB)
        // These go directly to the Large Object Heap (LOH).
        var largeObjects = new object[5];
        for (int i = 0; i < 5; i++)
        {
            largeObjects[i] = new byte[100 * 1024]; // 100 KB
        }

        // Force a full garbage collection to observe movement
        Console.WriteLine("Forcing full GC...");
        var sw = Stopwatch.StartNew();
        GC.Collect(2, GCCollectionMode.Forced, true);
        sw.Stop();
        
        Console.WriteLine($"Full GC (Gen 2) took: {sw.ElapsedMilliseconds} ms");
        
        // Simulate a Pinned Object
        GCHandle pinnedHandle = GCHandle.Alloc(new byte[100], GCHandleType.Pinned);
        
        // Output generated DOT diagram
        Console.WriteLine("\n

[ERROR: Failed to render diagram.]

");

        // Cleanup
        pinnedHandle.Free();
    }

    private void GenerateDotDiagram()
    {
        string dot = @"
digraph G {
    rankdir=TB;
    node [shape=box, style=""rounded""];
    
    // Nodes
    Stack [shape=ellipse, label=""Stack (Local Variables)""];
    ManagedHeap [shape=component, label=""Managed Heap""];
    Gen0 [label=""Gen 0 (Young)\nShort-lived objects""];
    Gen1 [label=""Gen 1 (Middle)\nSurvived Gen 0 GC""];
    Gen2 [label=""Gen 2 (Old)\nLong-lived objects""];
    LOH [label=""Large Object Heap (LOH)\nObjects > 85KB""];
    PinnedObj [label=""Pinned Object\n(GCHandle)""];
    
    // Edges - Small Objects
    Stack -> Gen0 [label=""new byte[100]""];
    Gen0 -> Gen1 [label=""Survival""];
    Gen1 -> Gen2 [label=""Survival""];
    
    // Edges - Medium Objects (Promoted)
    Stack -> Gen0 [label=""new byte[84KB]"", style=dashed];
    
    // Edges - Large Objects (LOH)
    Stack -> LOH [label=""new byte[100KB]""];
    
    // Pinned Object Logic
    PinnedObj [shape=diamond];
    Gen0 -> PinnedObj [label=""Pinned""];
    
    // GC Collection Paths
    Gen0 -> Gen0 [label=""Collected (Minor)""];
    Gen2 -> Gen2 [label=""Collected (Full/Compact)""];
    LOH -> LOH [label=""Collected (Full only)""];
}";
        Console.WriteLine(dot);
    }
}
