
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

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InferenceService.ModelManagement
{
    // Interface for DI
    public interface IModelManager
    {
        Task<ModelMetadata> GetModelAsync();
    }

    public class ModelMetadata
    {
        public string Name { get; set; } = "BERT-Sentiment";
        public byte[] Weights { get; set; } = Array.Empty<byte>();
    }

    public sealed class ModelManager : IModelManager
    {
        // Singleton instance managed by Lazy<T> for thread-safe initialization
        private static readonly Lazy<ModelManager> _instance = new(
            () => new ModelManager(), 
            LazyThreadSafetyMode.ExecutionAndPublication
        );

        private const string CacheDir = "/models/cache";
        private const string ModelUri = "https://example.com/models/bert-weights.json";
        
        // Private constructor to enforce Singleton pattern
        private ModelManager() { }

        public static ModelManager Instance => _instance.Value;

        public async Task<ModelMetadata> GetModelAsync()
        {
            // 1. Check Cache
            if (TryGetCachedModel(out var cachedModel))
            {
                Console.WriteLine("Model loaded from cache.");
                return cachedModel;
            }

            // 2. Download and Cache (Simulated)
            Console.WriteLine("Model not in cache. Downloading...");
            var model = await DownloadModelAsync();
            
            // 3. Save to Cache
            await SaveToCacheAsync(model);

            return model;
        }

        private bool TryGetCachedModel(out ModelMetadata model)
        {
            model = null!;
            try
            {
                if (!Directory.Exists(CacheDir)) return false;

                // In a real scenario, verify hash/version here
                var cacheFile = Path.Combine(CacheDir, "model.json");
                if (!File.Exists(cacheFile)) return false;

                var json = File.ReadAllText(cacheFile);
                model = JsonSerializer.Deserialize<ModelMetadata>(json)!;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<ModelMetadata> DownloadModelAsync()
        {
            using var client = new HttpClient();
            // Simulate network latency and deserialization
            await Task.Delay(2000); 
            
            // Return dummy model data
            return new ModelMetadata 
            { 
                Weights = new byte[1024] // Simulated weight data
            };
        }

        private async Task SaveToCacheAsync(ModelMetadata model)
        {
            Directory.CreateDirectory(CacheDir);
            var json = JsonSerializer.Serialize(model);
            await File.WriteAllTextAsync(Path.Combine(CacheDir, "model.json"), json);
        }
    }
}
