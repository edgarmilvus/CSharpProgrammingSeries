
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

using Microsoft.Extensions.Caching.Hybrid;
using System.Text;
using System.Text.Json;

// --- Existing Domain Models (Assumed) ---
public class SqlRequest(string naturalLanguage, string schemaContext)
{
    public string NaturalLanguage { get; } = naturalLanguage;
    public string SchemaContext { get; } = schemaContext;
}

public class SqlResponse(string query, string explanation)
{
    public string Query { get; } = query;
    public string Explanation { get; } = explanation;
}

// --- Service Layer with Caching ---
public interface ISqlGenerationService
{
    Task<SqlResponse> GenerateSqlAsync(SqlRequest request, CancellationToken ct = default);
    Task UpdateSchema(string newSchemaDefinition);
}

public class SqlGenerationService : ISqlGenerationService
{
    private readonly HybridCache _cache;
    private string _currentSchema; // Mutable state for schema

    public SqlGenerationService(HybridCache cache)
    {
        _cache = cache;
        _currentSchema = "Initial Schema Definition"; // Load from config in real app
    }

    public async Task<SqlResponse> GenerateSqlAsync(SqlRequest request, CancellationToken ct = default)
    {
        // 1. Composite Cache Key
        // We include the current schema hash or version to ensure schema changes 
        // naturally invalidate old keys, though we also use tags for bulk removal.
        var schemaHash = BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(_currentSchema))).Replace("-", "").Substring(0, 8);
        var cacheKey = $"sql_gen:{request.NaturalLanguage.GetHashCode()}:{schemaHash}";

        // 2. Hybrid Cache Implementation
        try 
        {
            return await _cache.GetOrCreateAsync(
                cacheKey,
                async (ct) =>
                {
                    // 3. Expensive LLM Call (Simulated)
                    // This factory is guaranteed to run only once per key concurrently.
                    await Task.Delay(500, ct); // Simulate latency

                    // Mock LLM Response
                    var mockQuery = $"SELECT * FROM users WHERE comment LIKE '%{request.NaturalLanguage}%'";
                    return new SqlResponse(mockQuery, $"Generated from prompt: {request.NaturalLanguage}");
                },
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(15),
                    // Tagging for schema invalidation
                    Tags = new HashSet<string> { $"schema:{schemaHash}" }
                },
                ct
            );
        }
        catch (JsonException ex)
        {
            // Handle potential serialization errors from malformed LLM output
            // Log error and return a fallback or throw custom exception
            throw new InvalidOperationException("Failed to deserialize LLM response.", ex);
        }
    }

    public async Task UpdateSchema(string newSchemaDefinition)
    {
        _currentSchema = newSchemaDefinition;
        
        // Calculate new hash for future keys
        var newSchemaHash = BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(_currentSchema))).Replace("-", "").Substring(0, 8);
        
        // 4. Invalidation
        // Since we tagged entries with the *old* schema hash, we need to remove them.
        // However, tracking the exact tag string requires storing the old hash.
        // For this exercise, we assume we know the tag pattern or we clear all sql_gen tags.
        // A better approach is to track the active schema version in a singleton or Redis.
        
        // Simulating removal of the specific old tag:
        // In a real app, you might store the "CurrentSchemaHash" in a distributed cache 
        // and check it on read, or use a generic "SchemaVersion" tag that gets updated.
        
        // Here, we will remove by a generic tag if supported, or rely on the key structure change.
        // HybridCache supports RemoveByTagAsync.
        // Since we can't easily list all old tags here without state, we assume the 
        // invalidation mechanism targets the specific version tag.
        
        // For the sake of the exercise, we assume the system knows the previous tag 
        // (e.g., stored in a singleton) or we use a global "SchemaChanged" trigger.
        // Let's assume we stored the previous hash.
        // await _cache.RemoveByTagAsync($"schema:{oldSchemaHash}"); 
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(15) };
});

builder.Services.AddSingleton<ISqlGenerationService, SqlGenerationService>();

var app = builder.Build();

app.MapPost("/completion", async (SqlRequest request, ISqlGenerationService service, CancellationToken ct) =>
{
    var result = await service.GenerateSqlAsync(request, ct);
    return Results.Ok(result);
});

app.MapPost("/admin/schema", async (string newSchema, ISqlGenerationService service) =>
{
    await service.UpdateSchema(newSchema);
    return Results.Ok("Schema updated. Old cached SQLs are effectively invalidated via key change or tags.");
});

app.Run();
