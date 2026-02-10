
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GCInternalsExercise2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting LOH Fragmentation Simulation...");
            
            // We will use arrays > 85,000 bytes to land on the LOH.
            // 90,000 bytes is sufficient.
            const int arraySize = 90_000; 
            const int arrayCount = 100;

            var allocatedArrays = new List<byte[]>();

            // Phase 1: Allocation
            Console.WriteLine($"Phase 1: Allocating {arrayCount} arrays of {arraySize} bytes...");
            for (int i = 0; i < arrayCount; i++)
            {
                allocatedArrays.Add(new byte[arraySize]);
            }

            // Phase 2: Fragmentation
            Console.WriteLine("Phase 2: Removing references to every other array (creating holes)...");
            for (int i = 0; i < allocatedArrays.Count; i++)
            {
                if (i % 2 == 0) // Remove even indices
                {
                    allocatedArrays[i] = null;
                }
            }

            // Phase 3: Force Garbage Collection
            Console.WriteLine("Phase 3: Forcing Gen 2 Collection...");
            // We must collect twice to ensure objects are finalized (if any) and truly unreachable.
            // Note: byte[] doesn't have a finalizer, so one collection is usually enough, 
            // but Gen 2 collections are expensive, so we do it once explicitly.
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            // Phase 4: The Allocation Test
            // Total free memory = (arrayCount / 2) * arraySize
            // We need a contiguous block of size 'arraySize'.
            // Since we freed every other block, the largest contiguous block is exactly 'arraySize'.
            // However, LOH compaction is not default in older .NET versions (though enabled by default in .NET 6+).
            // To simulate fragmentation failure, we rely on the fact that without compaction, 
            // the free space is scattered.
            
            // Calculate total free memory theoretically available
            long totalFreedBytes = (arrayCount / 2) * arraySize;
            Console.WriteLine($"Theoretical Freed Memory: {totalFreedBytes / 1024.0:F2} KB");

            bool success = false;
            long elapsedMs = 0;

            try
            {
                Console.WriteLine("Phase 4: Attempting to allocate a block that fits total free space but requires contiguity...");
                
                var sw = Stopwatch.StartNew();
                
                // Attempt allocation. 
                // In a fragmented LOH, this might succeed if the allocator finds a hole, 
                // but if we fill the memory slightly more, it will fail.
                // To guarantee a demonstration of fragmentation, we try to allocate slightly 
                // more than the single hole size but less than total free space? 
                // Actually, the prompt asks to allocate a block fitting *total* freed memory 
                // but larger than any single freed block.
                
                // Let's adjust: We have 50 holes of 90,000 bytes.
                // We want to allocate one block of size > 90,000 but < 4,500,000.
                // Let's try 100,000 bytes. It fits in the total free space (4.5MB), 
                // but does it fit in a contiguous hole? Yes, the holes are 90k, so 100k won't fit in a hole.
                
                byte[] testArray = new byte[100_000]; 
                sw.Stop();
                elapsedMs = sw.ElapsedMilliseconds;
                success = true;
                Console.WriteLine($"Allocation SUCCEEDED. Size: {testArray.Length} bytes.");
            }
            catch (OutOfMemoryException)
            {
                elapsedMs = 0; // Stopwatch not started or failed immediately
                Console.WriteLine("Allocation FAILED (OutOfMemoryException). Fragmentation prevented contiguity.");
            }

            // Analysis
            Console.WriteLine("\n--- Analysis ---");
            if (success)
            {
                Console.WriteLine("Result: Allocation succeeded. This indicates that either LOH compaction occurred (default in .NET 6+),");
                Console.WriteLine("or the allocator found a contiguous block large enough (100k) within the fragmented heap.");
                Console.WriteLine("In strictly fragmented environments (older .NET or manual pinning), this would fail.");
            }
            else
            {
                Console.WriteLine("Result: Allocation failed. Despite having enough total memory (fragmented across holes),");
                Console.WriteLine("the LOH could not provide a contiguous block of the requested size.");
            }
        }
    }
}
