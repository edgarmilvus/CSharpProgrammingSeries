
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

public class NativeCallbackContext
{
    public byte[] Buffer { get; set; }
    public int Status { get; set; }
}

public class GCHandlePinningExample
{
    // Simulated native callback delegate
    private delegate void NativeCallback(IntPtr data, int length);
    
    // Simulated native function
    private static void SimulateNativeOperation(NativeCallback callback, IntPtr contextPtr)
    {
        // Native code writes to memory
        // For simulation, we just invoke the callback
        callback(contextPtr, 1024);
    }

    public static void Execute()
    {
        var context = new NativeCallbackContext 
        { 
            Buffer = new byte[1024], 
            Status = 0 
        };

        // 1. Allocate GCHandle
        GCHandle handle = GCHandle.Alloc(context, GCHandleType.Pinned);
        
        try
        {
            // 2. Retrieve address
            // We pin the specific array inside the class for efficiency, 
            // but here we pin the object reference to access fields.
            IntPtr contextPtr = handle.AddrOfPinnedObject();
            
            // Define the callback logic
            NativeCallback callback = (ptr, len) =>
            {
                // Access the managed memory from the "native" context
                // Note: We must be careful here as we are technically in a managed delegate
                // invoked by simulated native code.
                if (GCHandle.FromPtr(ptr).Target is NativeCallbackContext ctx)
                {
                    ctx.Buffer[0] = 255; // Write to buffer
                    ctx.Status = 1;      // Update status
                }
            };

            // 3. Simulate Native Interaction
            SimulateNativeOperation(callback, contextPtr);

            Console.WriteLine($"Status: {context.Status}, Buffer[0]: {context.Buffer[0]}");
        }
        finally
        {
            // 4. Critical: Free the handle
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }
}
