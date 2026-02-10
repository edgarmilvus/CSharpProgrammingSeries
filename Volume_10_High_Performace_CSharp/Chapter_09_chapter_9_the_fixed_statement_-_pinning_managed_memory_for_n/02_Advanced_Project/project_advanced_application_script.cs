
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace HighPerformanceCSharp.Chapter9
{
    // REAL-WORLD CONTEXT:
    // In AI and signal processing, we often need to perform low-level memory operations 
    // on large buffers of data (e.g., token embeddings, audio samples). 
    // This application simulates a high-performance "Vector Search" engine.
    // It calculates the Euclidean distance between a query vector and a database of 
    // document embeddings using a native C++ library for raw speed, requiring 
    // pinned managed memory to avoid copying overhead.

    // 1. DEFINING THE NATIVE LIBRARY IMPORTS
    // We simulate a native C++ DLL that contains highly optimized SIMD instructions.
    // In a real scenario, this would be 'extern "C" __declspec(dllexport) float CalculateDistance(float* v1, float* v2, int length);'
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeVectorLib
    {
        // NOTE: We use a mock implementation here for the console app to run standalone.
        // In a real deployment, this points to a compiled C++ DLL.
        [DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory", SetLastError = true)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        // A simulated native function that expects pinned pointers.
        // It calculates: Sum((v1[i] - v2[i]) ^ 2)
        public static unsafe float CalculateDistanceNative(float* vecA, float* vecB, int length)
        {
            float sumSqDiff = 0.0f;
            for (int i = 0; i < length; i++)
            {
                float diff = vecA[i] - vecB[i];
                sumSqDiff += diff * diff;
            }
            // Return square root (Euclidean distance)
            return (float)Math.Sqrt(sumSqDiff);
        }
    }

    // 2. DEFINING THE DATA STRUCTURE
    // We use a struct to represent a document embedding. 
    // Structs are value types and provide better memory locality (arrays of structs vs arrays of classes).
    // We will use a 'fixed' buffer inside this struct for inline storage.
    public unsafe struct DocumentEmbedding
    {
        public const int VectorSize = 8; // Small vector for demonstration
        public int DocId;

        // FIXED BUFFER:
        // Allows storing an inline array of floats within the struct layout.
        // This is pinned automatically when the struct itself is pinned.
        // Useful for small, fixed-size data often found in embedded systems or specific protocol headers.
        public fixed float EmbeddingData[VectorSize];

        public override string ToString()
        {
            // Helper to display data safely (using stackalloc to avoid fixed buffer access in non-fixed context)
            // In a real high-perf scenario, we'd avoid string formatting in hot paths.
            return $"DocID: {DocId}";
        }
    }

    class Program
    {
        // 3. MAIN APPLICATION LOGIC
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("=== High-Performance C# - Native Interop & Pinning Demo ===");
            Console.WriteLine("Scenario: Vector Similarity Search using Native SIMD Math\n");

            // A. PREPARE MANAGED DATA
            // We create a managed array of structs. The Garbage Collector (GC) moves this 
            // memory around compaction. We cannot pass pointers to this memory to native code
            // unless we pin it.
            const int dbSize = 5;
            DocumentEmbedding[] documentDatabase = new DocumentEmbedding[dbSize];

            // Initialize mock data
            Random rand = new Random(42);
            for (int i = 0; i < dbSize; i++)
            {
                documentDatabase[i] = new DocumentEmbedding { DocId = i + 100 };
                unsafe
                {
                    // We must use 'fixed' to access the fixed buffer inside the struct
                    fixed (float* ptr = documentDatabase[i].EmbeddingData)
                    {
                        for (int j = 0; j < DocumentEmbedding.VectorSize; j++)
                        {
                            ptr[j] = (float)rand.NextDouble(); // Random values 0.0 to 1.0
                        }
                    }
                }
            }

            // Define a query vector (managed array of floats)
            float[] queryVector = new float[DocumentEmbedding.VectorSize];
            for (int i = 0; i < queryVector.Length; i++) queryVector[i] = 0.5f;

            Console.WriteLine("Data initialized. Performing search...\n");

            // B. PERFORMANCE OPTIMIZATION: PINNING
            // We will iterate through the database and calculate distances.
            // To pass data to the native function efficiently, we pin the memory.
            
            // Strategy 1: Pinning specific structs using 'fixed' blocks.
            // Strategy 2: Pinning the managed array (queryVector) to pass its pointer.
            
            // Pin the query vector for the duration of the loop to avoid repeated pinning overhead.
            // GCHandle allows pinning objects that don't have fixed buffers.
            GCHandle queryHandle = GCHandle.Alloc(queryVector, GCHandleType.Pinned);
            IntPtr queryPtr = queryHandle.AddrOfPinnedObject();

            float minDistance = float.MaxValue;
            int bestMatchId = -1;

            try
            {
                // Iterate through the database
                for (int i = 0; i < documentDatabase.Length; i++)
                {
                    // CRITICAL: The 'fixed' statement pins the managed object (or specific field)
                    // in memory. The GC cannot relocate it. The pointer is valid only within this block.
                    // This is essential for safe native interop.
                    fixed (float* dbVecPtr = documentDatabase[i].EmbeddingData)
                    {
                        // Cast IntPtr to float* for the query vector
                        float* queryVecPtr = (float*)queryPtr;

                        // CALL NATIVE CODE
                        // We pass pointers to pinned memory. This is zero-copy.
                        float distance = NativeVectorLib.CalculateDistanceNative(
                            dbVecPtr, 
                            queryVecPtr, 
                            DocumentEmbedding.VectorSize
                        );

                        // LOGIC: Find the nearest neighbor
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            bestMatchId = documentDatabase[i].DocId;
                        }

                        // Optional: Print debug info
                        Console.WriteLine($"Checked Doc {documentDatabase[i].DocId}: Distance = {distance:F4}");
                    }
                    // The 'dbVecPtr' is automatically unpinned here. 
                    // The GC can move the struct if needed in the next iteration.
                }
            }
            finally
            {
                // Always unpin the GCHandle to prevent memory leaks.
                if (queryHandle.IsAllocated)
                    queryHandle.Free();
            }

            Console.WriteLine("\n=== RESULT ===");
            Console.WriteLine($"Best Match: Document ID {bestMatchId}");
            Console.WriteLine($"Distance: {minDistance:F4}");
        }
    }
}
