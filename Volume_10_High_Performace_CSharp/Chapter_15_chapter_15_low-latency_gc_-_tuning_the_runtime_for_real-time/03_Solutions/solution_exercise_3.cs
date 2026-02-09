
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Runtime;

public class NoGCInferenceEngine
{
    // Semaphore to ensure thread safety for the NoGC region entry/exit
    private static readonly SemaphoreSlim _noGcLock = new SemaphoreSlim(1, 1);

    public void RunCriticalInference()
    {
        // Estimate memory usage for the critical section (e.g., 10 MB)
        // This must be accurate; if we allocate more than this inside the region, 
        // a runtime exception (InsufficientMemoryException) will be thrown.
        long estimatedMemoryUsage = 10 * 1024 * 1024; 

        // Acquire lock to serialize access to the NoGC region (thread safety)
        _noGcLock.Wait();
        
        bool regionEntered = false;
        try
        {
            // Attempt to enter the NoGC region
            if (GC.TryStartNoGCRegion(estimatedMemoryUsage))
            {
                regionEntered = true;
                try
                {
                    // Perform critical inference work here
                    // Simulate heavy computation without allocations
                    PerformComputation();
                }
                finally
                {
                    // Always end the region in a finally block
                    GC.EndNoGCRegion();
                }
            }
            else
            {
                // Fallback logic if region couldn't be reserved
                PerformComputationFallback();
            }
        }
        catch (InsufficientMemoryException ex)
        {
            // This occurs if we allocate more than estimatedMemoryUsage inside the region
            Console.WriteLine($"NoGC Region failed due to allocation overrun: {ex.Message}");
            // If we are still in the region (exception thrown during execution), we must end it
            if (regionEntered)
            {
                GC.EndNoGCRegion();
            }
            PerformComputationFallback();
        }
        finally
        {
            _noGcLock.Release();
        }
    }

    private void PerformComputation()
    {
        // Simulate a heavy inference task (e.g., matrix multiplication)
        // CRITICAL: Do not allocate new objects (strings, arrays) here.
        // Use stackalloc or pre-allocated buffers if necessary.
        
        double sum = 0;
        for (int i = 0; i < 1_000_000; i++)
        {
            sum += Math.Sqrt(i) * Math.Sin(i);
        }
        // Console.WriteLine($"Inference Result: {sum}");
    }

    private void PerformComputationFallback()
    {
        // Slower, allocation-friendly version if NoGC fails
        Console.WriteLine("Running fallback inference logic (GC allowed)...");
    }
}
