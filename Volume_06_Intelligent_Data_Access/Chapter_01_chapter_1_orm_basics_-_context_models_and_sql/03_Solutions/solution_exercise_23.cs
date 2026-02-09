
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

# Source File: solution_exercise_23.cs
# Description: Solution for Exercise 23
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Lightweight Edge Vector Store
public class EdgeVectorStore
{
    // In-memory store for edge devices
    private readonly List<(int id, float[] vector)> _store = new();

    public void Add(int id, float[] vector)
    {
        _store.Add((id, vector));
    }

    // 2. On-Device Search (No Network)
    public List<(int id, double score)> Search(float[] queryVector, int topK)
    {
        return _store
            .Select(x => (x.id, Score: CalculateSimilarity(x.vector, queryVector)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    }

    private double CalculateSimilarity(float[] a, float[] b) => 0.9; // Mock
}

// 3. Synchronization Mechanism
public class EdgeSyncService
{
    private readonly EdgeVectorStore _localStore;
    private readonly ICloudVectorClient _cloudClient;

    public async Task SyncAsync()
    {
        // 1. Pull updates from Cloud
        var lastSync = GetLastSyncTimestamp();
        var updates = await _cloudClient.GetUpdatesSince(lastSync);

        foreach (var update in updates)
        {
            _localStore.Add(update.Id, update.Vector);
        }

        // 2. Push local changes (if device generates data)
        // ...
    }

    // 4. Compression Strategy
    public byte[] CompressVector(float[] vector)
    {
        // Use Quantization (from Exercise 17) to reduce size for transmission
        // float[] (400 bytes) -> byte[] (100 bytes)
        return vector.Select(v => (byte)(v * 127)).ToArray();
    }

    private DateTime GetLastSyncTimestamp() => DateTime.MinValue; // Mock
}

// 5. Offline Search
public class OfflineFirstSearch
{
    private readonly EdgeVectorStore _store;

    public async Task<List<int>> FindSimilarAsync(float[] vector)
    {
        // Check connectivity
        if (IsOnline())
        {
            // Online: Hybrid search (Local + Cloud)
            var localResults = _store.Search(vector, 5);
            var cloudResults = await _cloudClient.Search(vector, 5);
            return MergeResults(localResults, cloudResults);
        }
        else
        {
            // Offline: Local only
            return _store.Search(vector, 5).Select(x => x.id).ToList();
        }
    }

    private bool IsOnline() => true; // Mock
    private List<int> MergeResults(List<(int id, double score)> local, List<(int id, double score)> cloud) => new();
}

// Mocks
public interface ICloudVectorClient 
{
    Task<List<VectorRecord>> GetUpdatesSince(DateTime dt);
    Task<List<(int id, double score)>> Search(float[] vector, int topK);
}
