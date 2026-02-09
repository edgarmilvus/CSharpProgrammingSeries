
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System.Buffers;
using System.Collections.Concurrent;

namespace GpuResourceManagement
{
    public interface IGpuMemoryManager
    {
        Memory<byte>? TryAllocate(int sizeInBytes);
        void Release(Memory<byte> handle);
    }

    public class GpuMemoryManager : IGpuMemoryManager
    {
        private readonly long _totalMemory;
        private long _allocatedMemory;
        private readonly ConcurrentBag<Memory<byte>> _pool = new();

        public GpuMemoryManager(long totalMemoryBytes)
        {
            _totalMemory = totalMemoryBytes;
        }

        public Memory<byte>? TryAllocate(int sizeInBytes)
        {
            // Simple check for available memory (thread-safe atomic operation)
            long currentAllocated = Interlocked.Read(ref _allocatedMemory);
            if (currentAllocated + sizeInBytes > _totalMemory)
            {
                return null; // GPU OOM
            }

            // Atomically add the size
            Interlocked.Add(ref _allocatedMemory, sizeInBytes);

            // Allocate a managed array to simulate VRAM allocation
            // In a real scenario, this would be a pointer to native GPU memory
            var buffer = new byte[sizeInBytes];
            var memory = new Memory<byte>(buffer);
            
            return memory;
        }

        public void Release(Memory<byte> handle)
        {
            // Calculate size (simplified)
            long size = handle.Length; 
            
            // Return memory to pool (optional optimization) or just GC
            // _pool.Add(handle); 
            
            // Decrement allocated counter
            Interlocked.Add(ref _allocatedMemory, -size);
        }
    }

    public class PredictionService
    {
        private readonly IGpuMemoryManager _gpuManager;

        public PredictionService(IGpuMemoryManager gpuManager)
        {
            _gpuManager = gpuManager;
        }

        public async Task<string> PredictAsync(int inputSize)
        {
            var memoryHandle = _gpuManager.TryAllocate(inputSize);
            
            if (memoryHandle == null)
            {
                // Return 503 Service Unavailable
                throw new HttpRequestException("GPU Out of Memory", null, System.Net.HttpStatusCode.ServiceUnavailable);
            }

            try
            {
                // Simulate processing
                await Task.Delay(100);
                return "Inference Complete";
            }
            finally
            {
                _gpuManager.Release(memoryHandle.Value);
            }
        }
    }
}
