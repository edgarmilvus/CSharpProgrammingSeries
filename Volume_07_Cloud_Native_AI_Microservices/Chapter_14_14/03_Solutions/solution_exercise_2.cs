
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System.Collections.Concurrent;
using System.Diagnostics;

public record ModelInfo(string ModelId, int MemoryFootprint, int UsageScore, bool IsLoaded = false, DateTime LastAccessed = default);

public static class GpuMemoryTracker
{
    private const long TotalMemory = 8L * 1024 * 1024 * 1024; // 8GB
    private static long _allocated;
    private static readonly object _lock = new object();

    public static long AllocatedMemory => _allocated;
    public static long FreeMemory => TotalMemory - _allocated;

    public static bool TryAllocate(long bytes)
    {
        lock (_lock)
        {
            if (_allocated + bytes <= TotalMemory)
            {
                _allocated += bytes;
                return true;
            }
            return false;
        }
    }

    public static void Deallocate(long bytes)
    {
        lock (_lock)
        {
            _allocated -= bytes;
            if (_allocated < 0) _allocated = 0;
        }
    }
}

public class ModelManager
{
    // Key: ModelId, Value: ModelInfo
    private readonly ConcurrentDictionary<string, ModelInfo> _loadedModels = new();
    
    // LRU Logic: We track access times to determine which model is least recently used
    // For high concurrency, we might use a Priority Queue, but for this simulation 
    // we will iterate the dictionary (which is safe for reads) to find the eviction candidate.

    public async Task LoadModelAsync(ModelInfo model)
    {
        // 1. Check if already loaded
        if (_loadedModels.TryGetValue(model.ModelId, out var existing))
        {
            // Update access time
            _loadedModels[model.ModelId] = existing with { LastAccessed = DateTime.UtcNow, UsageScore = existing.UsageScore + 1 };
            return;
        }

        // 2. Check Memory
        if (GpuMemoryTracker.TryAllocate(model.MemoryFootprint))
        {
            // Simulate loading from disk/network
            await Task.Delay(50); 
            var loadedModel = model with { IsLoaded = true, LastAccessed = DateTime.UtcNow };
            _loadedModels[model.ModelId] = loadedModel;
            return;
        }

        // 3. Eviction Logic (LRU + UsageScore)
        await EvictAndLoadAsync(model);
    }

    private async Task EvictAndLoadAsync(ModelInfo newModel)
    {
        ModelInfo? candidateToRemove = null;

        // Snapshot for safe iteration
        var currentModels = _loadedModels.Values.ToList();

        if (currentModels.Count > 0)
        {
            // Strategy: Prioritize UsageScore (Cold vs Hot), then LRU (LastAccessed)
            candidateToRemove = currentModels
                .OrderBy(m => m.UsageScore) // Lowest usage first
                .ThenBy(m => m.LastAccessed) // Oldest access first
                .FirstOrDefault();
        }

        if (candidateToRemove != null)
        {
            // Unload
            if (_loadedModels.TryRemove(candidateToRemove.ModelId, out _))
            {
                GpuMemoryTracker.Deallocate(candidateToRemove.MemoryFootprint);
                
                // Simulate deallocation overhead
                await Task.Delay(10);
            }
        }

        // Retry allocation
        if (GpuMemoryTracker.TryAllocate(newModel.MemoryFootprint))
        {
            await Task.Delay(50); // Simulate load
            var loadedModel = newModel with { IsLoaded = true, LastAccessed = DateTime.UtcNow };
            _loadedModels[newModel.ModelId] = loadedModel;
        }
        else
        {
            throw new InvalidOperationException("Insufficient GPU memory even after eviction.");
        }
    }
    
    // Helper for Dependency Injection registration
    public static IServiceCollection AddModelManager(this IServiceCollection services)
    {
        return services.AddSingleton<ModelManager>();
    }
}
