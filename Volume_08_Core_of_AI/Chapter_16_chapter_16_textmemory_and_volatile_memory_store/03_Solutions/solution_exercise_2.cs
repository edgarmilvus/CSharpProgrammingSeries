
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

using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentMemoryApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup
            var memoryStore = new VolatileMemoryStore();
            string collectionName = "ConcurrentCollection";
            int taskCount = 10;
            int iterationsPerTask = 5;
            int expectedTotal = taskCount * iterationsPerTask;

            // 2. Unsafe Execution (Demonstration - Optional, might throw or lose data)
            // await RunUnsafeTest(memoryStore, collectionName, taskCount, iterationsPerTask);

            // 3. Safe Execution
            var safeStore = new ThreadSafeMemoryStoreWrapper(memoryStore);
            await RunSafeTest(safeStore, collectionName, taskCount, iterationsPerTask);

            // 4. Verification
            var count = await safeStore.GetCollectionSize(collectionName);
            Console.WriteLine($"Expected: {expectedTotal}, Actual: {count}");
            Console.WriteLine(count == expectedTotal ? "SUCCESS: No data loss." : "FAILURE: Data loss detected.");
        }

        static async Task RunUnsafeTest(IMemoryStore store, string collection, int tasks, int iterations)
        {
            Console.WriteLine("\n--- Running Unsafe Test ---");
            var taskList = new List<Task>();
            for (int i = 0; i < tasks; i++)
            {
                int taskId = i;
                taskList.Add(Task.Run(async () =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        string key = $"key-{taskId}-{j}";
                        string text = $"Text {taskId}-{j}";
                        var embedding = new float[] { 1.0f }; // Dummy embedding
                        
                        // VolatileMemoryStore is not thread-safe for writes in all versions
                        await store.UpsertAsync(collection, key, text, null, "description", embedding);
                    }
                }));
            }
            await Task.WhenAll(taskList);
            var count = store.GetCollectionSize(collection).Result;
            Console.WriteLine($"Unsafe Result: {count} items (Expected 50)");
        }

        static async Task RunSafeTest(IMemoryStore store, string collection, int tasks, int iterations)
        {
            Console.WriteLine("\n--- Running Safe Test ---");
            var taskList = new List<Task>();
            for (int i = 0; i < tasks; i++)
            {
                int taskId = i;
                taskList.Add(Task.Run(async () =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        string key = $"key-{taskId}-{j}";
                        string text = $"Text {taskId}-{j}";
                        var embedding = new float[] { 1.0f };
                        
                        // Using the thread-safe wrapper
                        await store.UpsertAsync(collection, key, text, null, "description", embedding);
                    }
                }));
            }
            await Task.WhenAll(taskList);
        }
    }

    // Wrapper to ensure thread safety
    public class ThreadSafeMemoryStoreWrapper : IMemoryStore
    {
        private readonly IMemoryStore _innerStore;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ThreadSafeMemoryStoreWrapper(IMemoryStore innerStore)
        {
            _innerStore = innerStore;
        }

        public async Task<string> UpsertAsync(string collection, string key, string text, string? description = null, string? additionalMetadata = null, float[]? embedding = null)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _innerStore.UpsertAsync(collection, key, text, description, additionalMetadata, embedding);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Delegate other methods to inner store (omitted for brevity, but required for full implementation)
        public IAsyncEnumerable<MemoryQueryResult> SearchAsync(string collection, string query, int limit = 1, double minRelevanceScore = 0, CancellationToken cancellationToken = default)
            => _innerStore.SearchAsync(collection, query, limit, minRelevanceScore, cancellationToken);

        public Task<bool> RemoveAsync(string collection, string key, CancellationToken cancellationToken = default)
            => _innerStore.RemoveAsync(collection, key, cancellationToken);

        public Task RemoveCollectionAsync(string collection, CancellationToken cancellationToken = default)
            => _innerStore.RemoveCollectionAsync(collection, cancellationToken);

        public Task<IEnumerable<string>> GetCollectionsAsync(CancellationToken cancellationToken = default)
            => _innerStore.GetCollectionsAsync(cancellationToken);

        public Task<MemoryRecord?> GetAsync(string collection, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
            => _innerStore.GetAsync(collection, key, withEmbedding, cancellationToken);

        // Helper for verification
        public async Task<int> GetCollectionSize(string collection)
        {
            // Note: VolatileMemoryStore doesn't expose count directly, 
            // so we simulate counting by listing keys or accessing internal state if possible.
            // For this exercise, we assume we can inspect the store or count the search results.
            var items = await _innerStore.SearchAsync(collection, "", limit: 1000).ToListAsync();
            return items.Count;
        }
    }
}
