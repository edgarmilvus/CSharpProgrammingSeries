
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System.Runtime.CompilerServices;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Resilience;
using Polly;
using System.Text.Json;

namespace KernelMemory.Exercises;

public class CosmosNoSqlMemoryStore : IMemoryStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<CosmosNoSqlMemoryStore> _logger;
    private const string DatabaseName = "KernelMemoryDB";
    private const string ContainerName = "MemoryVectors";

    // Configuration model for the container
    public class CosmosMemoryConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = DatabaseName;
        public string ContainerName { get; set; } = ContainerName;
        public string PartitionKeyPath { get; set; } = "/tenantId";
        public int VectorDimension { get; set; } = 1536; // e.g., for text-embedding-ada-002
    }

    public CosmosNoSqlMemoryStore(CosmosMemoryConfig config, ILogger<CosmosNoSqlMemoryStore> logger)
    {
        _logger = logger;
        
        // Initialize Cosmos Client
        _cosmosClient = new CosmosClient(config.Endpoint, config.Key);
        _database = _cosmosClient.CreateDatabaseIfNotExistsAsync(config.DatabaseName).GetAwaiter().GetResult().Database;
        
        // Define Vector Indexing Policy
        var vectorIndexingPolicy = new VectorIndexingPolicy
        {
            // Flat is exact search (good for < 500k vectors). IVF is approximate (faster for large datasets).
            // We choose Flat for accuracy in this example, but IVF is recommended for scale.
            IndexType = VectorIndexType.Flat, 
            DistanceFunction = DistanceFunction.Cosine,
            Dimensions = config.VectorDimension
        };

        var containerProperties = new ContainerProperties(id: config.ContainerName, partitionKeyPath: config.PartitionKeyPath)
        {
            // Define the vector embedding path
            VectorEmbeddingPolicy = new VectorEmbeddingPolicy
            {
                Embeddings = 
                {
                    new VectorEmbeddingDefinition
                    {
                        Path = "/embedding",
                        DataType = VectorDataType.Float32,
                        Dimensions = config.VectorDimension,
                        DistanceFunction = DistanceFunction.Cosine
                    }
                }
            },
            IndexingPolicy = new IndexingPolicy
            {
                // Ensure vector indexing is enabled
                VectorIndexes = 
                {
                    new VectorIndexPath { Path = "/embedding", IndexingStrategy = VectorIndexingStrategy.Flat }
                }
            }
        };

        // Create container with Unique Key for ID
        containerProperties.UniqueKeyPolicy.UniqueKeys.Add(new UniqueKey { Paths = { "/id" } });
        
        _container = _database.CreateContainerIfNotExistsAsync(containerProperties).GetAwaiter().GetResult().Container;

        // Initialize Resilience Pipeline (Polly) using Microsoft.Extensions.Resilience
        var resilienceOptions = new ResilienceOptionsBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RequestRateExceededException>().Handle<TimeoutException>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RequestRateExceededException>(),
                FailureRatio = 0.1,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
            
        _resiliencePipeline = resilienceOptions.Build();
    }

    public async Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        // Ensure we use the correct container (collection)
        var container = GetContainer(collectionName);
        
        // Cosmos DB Upsert acts as insert or update based on the ID
        var response = await _resiliencePipeline.ExecuteAsync(
            async token => await container.UpsertItemAsync(
                record, 
                new PartitionKey(record.GetPartitionKey()), // Assuming record.Metadata contains PartitionKey logic
                cancellationToken: token
            ),
            cancellationToken
        );

        return response.Resource.id;
    }

    public async IAsyncEnumerable<MemoryRecord> GetListAsync(
        string collectionName, 
        MemoryFilter? filter = null, 
        int limit = 1, 
        bool withEmbeddings = false, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var container = GetContainer(collectionName);
        
        // Construct the query
        var queryable = container.GetItemLinqQueryable<MemoryRecord>();
        
        // 1. Apply Metadata Filters (Hybrid Search Part 1)
        if (filter != null && filter.Filters.Count > 0)
        {
            foreach (var kvp in filter.Filters)
            {
                // Assuming metadata is stored flat in the root object for simplicity, 
                // or deeply nested. Here we assume a generic property access.
                queryable = queryable.Where(x => x.Metadata[kvp.Key] == kvp.Value);
            }
        }

        // 2. Apply Vector Similarity (Hybrid Search Part 2)
        // Note: Cosmos DB NoSQL LINQ provider does not natively support VectorDistance in C# LINQ expressions yet.
        // We must use a SQL query string for Vector functions.
        
        string sqlQuery = "SELECT * FROM c WHERE 1=1";
        
        if (filter != null && filter.Filters.Count > 0)
        {
            foreach (var kvp in filter.Filters)
            {
                sqlQuery += $" AND c.metadata['{kvp.Key}'] = '{kvp.Value}'";
            }
        }

        // If an embedding is provided in the filter (for similarity search), we add the VectorDistance function
        // This requires the container to have the Vector Index configured.
        if (filter?.Embedding != null)
        {
            // Serialize embedding to array string for SQL query
            var embeddingArray = "[" + string.Join(",", filter.Embedding) + "]";
            // We add a filter for distance, but typically vector search requires ORDER BY and TOP.
            // Since limit is provided, we will order by distance.
            sqlQuery += $" ORDER BY VectorDistance(c.embedding, {embeddingArray})";
        }

        var queryDefinition = new QueryDefinition(sqlQuery);
        var iterator = container.GetItemQueryIterator<MemoryRecord>(queryDefinition, requestOptions: new QueryRequestOptions { MaxItemCount = limit });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            foreach (var item in response.Resource)
            {
                yield return item;
            }
        }
    }

    public async Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(collectionName);
        try
        {
            var response = await _resiliencePipeline.ExecuteAsync(
                async token => await container.ReadItemAsync<MemoryRecord>(key, new PartitionKey(GetPartitionKeyFromId(key)), cancellationToken: token),
                cancellationToken
            );
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string collectionName, string key, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(collectionName);
        await _resiliencePipeline.ExecuteAsync(
            async token => await container.DeleteItemAsync<MemoryRecord>(key, new PartitionKey(GetPartitionKeyFromId(key)), cancellationToken: token),
            cancellationToken
        );
    }

    public async Task DeleteAllAsync(string collectionName, IEnumerable<string>? keys = null, CancellationToken cancellationToken = default)
    {
        var container = GetContainer(collectionName);
        
        if (keys != null)
        {
            // Batch delete (Cosmos DB doesn't support single command multi-delete)
            // We use a BulkExecutor pattern or simple loop with parallelism
            var tasks = keys.Select(key => 
                _resiliencePipeline.ExecuteAsync(
                    async token => await container.DeleteItemAsync<MemoryRecord>(key, new PartitionKey(GetPartitionKeyFromId(key)), cancellationToken: token),
                    cancellationToken
                )
            );
            await Task.WhenAll(tasks);
        }
        else
        {
            // WARNING: Deleting all items in a container is expensive.
            // Best practice is to delete the container and recreate it.
            await _database.DeleteContainerAsync(container.Id, cancellationToken: cancellationToken);
            // Recreate immediately
            await _database.CreateContainerAsync(container.Id, "/tenantId", cancellationToken: cancellationToken);
        }
    }

    // Helper to map collection name to container
    private Container GetContainer(string collectionName)
    {
        // In a real scenario, you might map collectionName to different containers or use a discriminator field.
        // Here we assume single container strategy with a discriminator or just use the collection name as container ID.
        return _database.GetContainer(collectionName);
    }

    private string GetPartitionKeyFromId(string id)
    {
        // Simple hash or logic to determine partition key from ID if not stored in record
        // Ideally, MemoryRecord should contain the PartitionKey logic.
        return "default"; 
    }
}

// Extension to support MemoryRecord serialization structure required by KM
public class MemoryRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    // Helper to get partition key
    public string GetPartitionKey() 
    {
        if (Metadata.TryGetValue("tenantId", out var tenantId)) return tenantId.ToString();
        if (Metadata.TryGetValue("collectionId", out var collectionId)) return collectionId.ToString();
        return "default";
    }
}
