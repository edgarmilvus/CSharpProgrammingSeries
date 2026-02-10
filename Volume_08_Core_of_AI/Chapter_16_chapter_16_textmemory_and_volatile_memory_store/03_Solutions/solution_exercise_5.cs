
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HybridMemoryArch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup Stores
            var volatileStore = new VolatileStore();
            var fileStore = new FileStore("hybrid_db.json");
            
            // Initialize Manager
            var manager = new HybridMemoryManager(volatileStore, fileStore);

            // Scenario A: Add persistent data
            await manager.AddToPersistent("sys_1", "System Setup: Initialize the server configuration.");
            
            // Scenario B: Add volatile session data
            await manager.AddToVolatile("sess_1", "Current Session: User reporting login issues.");

            // Scenario C: Query "Setup" (Should hit FileStore)
            Console.WriteLine("--- Query: 'Setup' ---");
            var result1 = await manager.SearchHybrid("Setup");
            Console.WriteLine(result1 != null ? $"Found: {result1.Text}" : "Not found");

            // Scenario D: Query "Session" (Should hit VolatileStore)
            Console.WriteLine("\n--- Query: 'Session' ---");
            var result2 = await manager.SearchHybrid("Session");
            Console.WriteLine(result2 != null ? $"Found: {result2.Text}" : "Not found");

            // Challenge: Cache Miss Scenario
            // Clear volatile to simulate a restart or empty cache
            await volatileStore.RemoveCollectionAsync("SessionCollection"); 
            Console.WriteLine("\n--- Cache Miss Test: Query 'Setup' again ---");
            
            // First call: Should find in FileStore, cache to Volatile
            var result3 = await manager.SearchHybrid("Setup");
            Console.WriteLine($"First Call: {result3?.Text}");

            // Second call: Should find in VolatileStore (Fast)
            var result4 = await manager.SearchHybrid("Setup");
            Console.WriteLine($"Second Call (Cached): {result4?.Text}");
        }
    }

    // --- Interfaces ---
    public interface IMemoryRecord
    {
        string Key { get; set; }
        string Text { get; set; }
        float[] Embedding { get; set; }
    }

    public interface IMemoryStore
    {
        Task AddAsync(IMemoryRecord record);
        Task<IMemoryRecord?> SearchAsync(string query);
    }

    public interface IPersistentStore : IMemoryStore
    {
        Task SaveToFileAsync();
        Task LoadFromFileAsync();
    }

    // --- Implementations ---

    public class SimpleRecord : IMemoryRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    public class VolatileStore : IMemoryStore
    {
        // Thread-safe collection for session memory
        private readonly ConcurrentDictionary<string, IMemoryRecord> _store = new();

        public Task AddAsync(IMemoryRecord record)
        {
            _store.AddOrUpdate(record.Key, record, (k, v) => record);
            return Task.CompletedTask;
        }

        public Task<IMemoryRecord?> SearchAsync(string query)
        {
            // Simple mock search logic
            var queryEmbed = MockEmbeddingGenerator.GenerateEmbedding(query);
            var best = _store.Values.OrderByDescending(r => CalculateScore(r.Embedding, queryEmbed)).FirstOrDefault();
            return Task.FromResult(best);
        }

        // Helper to clear for testing
        public Task RemoveCollectionAsync()
        {
            _store.Clear();
            return Task.CompletedTask;
        }

        private float CalculateScore(float[] a, float[] b)
        {
            // Simplified dot product for normalized vectors
            return a.Zip(b, (x, y) => x * y).Sum();
        }
    }

    public class FileStore : IPersistentStore
    {
        private readonly string _filePath;
        private List<IMemoryRecord> _records = new();

        public FileStore(string filePath) => _filePath = filePath;

        public async Task AddAsync(IMemoryRecord record)
        {
            _records.Add(record);
            await SaveToFileAsync();
        }

        public async Task<IMemoryRecord?> SearchAsync(string query)
        {
            // Load from file if empty (simulating persistence)
            if (_records.Count == 0) await LoadFromFileAsync();

            var queryEmbed = MockEmbeddingGenerator.GenerateEmbedding(query);
            return _records.OrderByDescending(r => CalculateScore(r.Embedding, queryEmbed)).FirstOrDefault();
        }

        public async Task SaveToFileAsync()
        {
            var json = JsonSerializer.Serialize(_records);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task LoadFromFileAsync()
        {
            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath);
                _records = JsonSerializer.Deserialize<List<SimpleRecord>>(json)?.Cast<IMemoryRecord>().ToList() ?? new List<IMemoryRecord>();
            }
        }

        private float CalculateScore(float[] a, float[] b) => a.Zip(b, (x, y) => x * y).Sum();
    }

    // --- Hybrid Manager ---
    public class HybridMemoryManager
    {
        private readonly VolatileStore _volatile;
        private readonly FileStore _persistent;
        private const double Threshold = 0.5f; // Mock threshold

        public HybridMemoryManager(VolatileStore volatileStore, FileStore persistentStore)
        {
            _volatile = volatileStore;
            _persistent = persistentStore;
        }

        public async Task AddToVolatile(string key, string text)
        {
            var record = new SimpleRecord { Key = key, Text = text, Embedding = MockEmbeddingGenerator.GenerateEmbedding(text) };
            await _volatile.AddAsync(record);
        }

        public async Task AddToPersistent(string key, string text)
        {
            var record = new SimpleRecord { Key = key, Text = text, Embedding = MockEmbeddingGenerator.GenerateEmbedding(text) };
            await _persistent.AddAsync(record);
        }

        public async Task<IMemoryRecord?> SearchHybrid(string query)
        {
            // 1. Search Volatile (Session Memory)
            var volatileResult = await _volatile.SearchAsync(query);
            
            // Check threshold (Mock calculation)
            float score = CalculateScore(volatileResult?.Embedding ?? Array.Empty<float>(), MockEmbeddingGenerator.GenerateEmbedding(query));
            
            if (volatileResult != null && score > Threshold)
            {
                Console.WriteLine("Source: Volatile Store (High Relevance)");
                return volatileResult;
            }

            // 2. Search Persistent (Long-term Memory)
            var persistentResult = await _persistent.SearchAsync(query);
            
            if (persistentResult != null)
            {
                Console.WriteLine("Source: Persistent Store");
                
                // 3. Cache Miss Logic: If found in persistent, cache it in volatile
                await _volatile.AddAsync(persistentResult);
                Console.WriteLine("Action: Cached result to Volatile Store.");
                
                return persistentResult;
            }

            return null;
        }

        private float CalculateScore(float[] a, float[] b)
        {
            if (a.Length == 0 || b.Length == 0) return 0;
            return a.Zip(b, (x, y) => x * y).Sum();
        }
    }

    // Mock Generator (Reused from Ex 1)
    public static class MockEmbeddingGenerator
    {
        public static float[] GenerateEmbedding(string text)
        {
            if (string.IsNullOrEmpty(text)) return new float[0];
            double sum = 0;
            foreach (char c in text) sum += (int)c;
            return new float[] { (float)(sum / 10000.0) };
        }
    }
}
