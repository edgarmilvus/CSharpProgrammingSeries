
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
using System.Buffers;
using System.Text;

public class BufferVisualizer
{
    // Requirement 4: Logging State Transitions
    public static void Log(string message) => Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss.fff} | {message}");

    public static void SimulateLifecycle()
    {
        Log("System Start");
        
        // Rent
        Log("Requesting buffer from ArrayPool...");
        var buffer = ArrayPool<byte>.Shared.Rent(1024);
        Log($"Buffer Rented (ID: {buffer.GetHashCode()}). State: RENTED");

        // Use
        Log("Writing data to buffer. State: IN_USE");
        buffer[0] = 1;

        // Resize (Simulated)
        Log("Resizing buffer...");
        var newBuffer = ArrayPool<byte>.Shared.Rent(2048);
        Array.Copy(buffer, newBuffer, 1024);
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = newBuffer;
        Log($"Buffer Resized (New ID: {buffer.GetHashCode()}). State: RENTED");

        // Return
        Log("Processing complete. Returning buffer...");
        ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        Log($"Buffer Returned. State: RETURNED_TO_POOL");

        // Leak Simulation (Bad Practice)
        Log("Simulating Leak...");
        var leakedBuffer = ArrayPool<byte>.Shared.Rent(512);
        leakedBuffer[0] = 255;
        // FORGETTING Return()
        Log($"Buffer Leaked (ID: {leakedBuffer.GetHashCode()}). State: IN_USE (Orphaned)");
        Log("GC will eventually reclaim leaked memory.");
    }

    // Interactive Challenge: Buffer Affinity (Pinning)
    public static void SimulatePinnedLifecycle()
    {
        Log("--- Pinned Buffer Simulation ---");
        var buffer = ArrayPool<byte>.Shared.Rent(1024);
        int pinCycles = 0;
        const int maxPins = 5;

        Log($"Buffer Rented. State: RENTED");

        while (pinCycles < maxPins)
        {
            // Pinning Logic
            Log($"Cycle {pinCycles + 1}: Buffer PINNED to processing stage.");
            
            // Simulate work
            // In real code, we might use GCHandle.Alloc(buffer, GCHandleType.Pinned) here
            // but for this exercise, we just log the concept.
            
            Log($"Cycle {pinCycles + 1}: Processing using cached buffer (High Locality).");
            
            pinCycles++;
            Log($"Cycle {pinCycles}: Buffer UNPINNED.");
        }

        Log("Max pin cycles reached. Returning to pool.");
        ArrayPool<byte>.Shared.Return(buffer);
        Log("Buffer Returned. State: RETURNED_TO_POOL");
    }
}

// Requirement 1: Conceptual DOT Diagram (String representation)
public static class DiagramGenerator
{
    public static string GetLifecycleDiagram()
    {
        return @"
digraph BufferLifecycle {
    node [shape=box, style=filled, color=lightblue];
    Pool [shape=ellipse, color=lightgreen];
    GC [shape=ellipse, color=red];

    Pool -> Rented [label=""Rent()"", color=green];
    Rented -> InUse [label=""Process()"", color=blue];
    InUse -> Pool [label=""Return()"", color=green];
    InUse -> GC [label=""Leak (No Return)"", color=red, style=dashed];
    
    // Pinned extension
    InUse -> Pinned [label=""Pin()"", color=purple];
    Pinned -> InUse [label=""Unpin() / Cycle"", color=purple];
    Pinned -> Pool [label=""Return()"", color=green];
}
";
    }
}
