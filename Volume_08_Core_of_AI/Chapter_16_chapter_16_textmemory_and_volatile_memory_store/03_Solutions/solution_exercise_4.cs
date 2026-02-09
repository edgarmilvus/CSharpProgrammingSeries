
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemorySerialization
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePath = "memory_export.json";

            // 1. Populate Store A
            var storeA = new VolatileMemoryStore();
            await PopulateStore(storeA, "TestCollection");

            // 2. Export to JSON
            await ExportMemoryToJSON(storeA, "TestCollection", filePath);
            Console.WriteLine("Export completed.");

            // 3. Simulate Restart: Create Store B
            var storeB = new VolatileMemoryStore();

            // 4. Import from JSON
            await ImportMemoryFromJSON(storeB, "TestCollection", filePath);
            Console.WriteLine("Import completed.");

            // 5. Verification
            var results = storeB.SearchAsync("TestCollection", "Item 50", limit: 1);
            await foreach (var result in results)
            {
                Console.WriteLine($"Verification Success: Found '{result.Metadata.Text}' in Store B.");
            }
        }

        static async Task PopulateStore(IMemoryStore store, string collection)
        {
            for (int i = 1; i <= 100; i++)
            {
                string text = $"Memory item {i}";
                // Mock embedding: simple vector based on index
                float[] embedding = new float[] { (float)i / 100.0f }; 
                await store.UpsertAsync(collection, $"key_{i}", text, embedding: embedding);
            }
        }

        static async Task ExportMemoryToJSON(IMemoryStore store, string collection, string filePath)
        {
            var records = new List<SerializedRecord>();
            
            // Fetch all records (using a large limit or iterating keys if available)
            // Note: VolatileMemoryStore doesn't have a direct "GetAllKeys", so we search with a wildcard or empty query
            var searchResults = store.SearchAsync(collection, "", limit: 1000);
            
            await foreach (var item in searchResults)
            {
                records.Add(new SerializedRecord
                {
                    Key = item.Metadata.Id,
                    Text = item.Metadata.Text,
                    Embedding = item.Embedding.ToArray() // Convert ReadOnlyMemory to Array
                });
            }

            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        static async Task ImportMemoryFromJSON(IMemoryStore store, string collection, string filePath)
        {
            if (!File.Exists(filePath)) return;

            var json = await File.ReadAllTextAsync(filePath);
            var records = JsonSerializer.Deserialize<List<SerializedRecord>>(json);

            if (records != null)
            {
                foreach (var record in records)
                {
                    await store.UpsertAsync(collection, record.Key, record.Text, embedding: record.Embedding);
                }
            }
        }
    }

    // DTO for Serialization
    public class SerializedRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
