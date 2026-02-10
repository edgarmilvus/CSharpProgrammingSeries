
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

// Source File: solution_exercise_25.cs
// Description: Solution for Exercise 25
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Abstraction Layer
public interface IVectorRepository<T> where T : class
{
    Task UpsertAsync(string id, float[] vector, Dictionary<string, object> metadata);
    Task<List<VectorResult<T>>> SearchAsync(float[] queryVector, int topK, Dictionary<string, object> filters = null);
    Task DeleteAsync(string id);
    Task CreateIndexAsync(string fieldName, string indexType); // e.g., "HNSW"
}

public class VectorResult<T>
{
    public T Entity { get; set; }
    public double Score { get; set; }
}

// 2. Plugin System (Factory)
public enum VectorProvider { Pinecone, PgVector, Milvus }

public class VectorRepositoryFactory
{
    public IVectorRepository<T> Create<T>(VectorProvider provider) where T : class
    {
        return provider switch
        {
            VectorProvider.Pinecone => new PineconeRepository<T>(),
            VectorProvider.PgVector => new PgVectorRepository<T>(),
            VectorProvider.Milvus => new MilvusRepository<T>(),
            _ => throw new NotSupportedException()
        };
    }
}

// 3. Implementation Example (Pinecone)
public class PineconeRepository<T> : IVectorRepository<T>
{
    private readonly IPineconeClient _client;
    
    public async Task UpsertAsync(string id, float[] vector, Dictionary<string, object> metadata)
    {
        // Pinecone specific logic
        await _client.Upsert(id, vector, metadata);
    }

    public async Task<List<VectorResult<T>>> SearchAsync(float[] queryVector, int topK, Dictionary<string, object> filters = null)
    {
        // Pinecone specific query
        var results = await _client.Query(queryVector, topK, filters);
        return results.Select(r => new VectorResult<T> { Entity = (T)r.Entity, Score = r.Score }).ToList();
    }

    public async Task DeleteAsync(string id) => await _client.Delete(id);
    public async Task CreateIndexAsync(string fieldName, string indexType) => await _client.CreateIndex(fieldName, indexType);
}

// 4. Migration Path (Simulated)
public class VectorMigrationManager
{
    private readonly IVectorRepository<Document> _oldRepo;
    private readonly IVectorRepository<Document> _newRepo;

    public async Task MigrateRepository()
    {
        // Read from old, Write to new
        // Since interfaces are the same, the migration logic is generic
    }
}
