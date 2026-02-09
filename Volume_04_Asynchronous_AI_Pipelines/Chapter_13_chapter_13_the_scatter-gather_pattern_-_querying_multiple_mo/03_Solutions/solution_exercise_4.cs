
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// 1. Distinct Result Types
public class SqlResult { public List<string> Rows { get; set; } }
public class VectorResult { public List<double> Embeddings { get; set; } }
public class GraphResult { public List<string> Nodes { get; set; } }

public class KnowledgeGraphResponse
{
    public List<string> UnifiedNodes { get; set; }
    public string Metadata { get; set; }
}

// 2. Async Methods with Simulated Delays
public class DatabaseService
{
    public async Task<SqlResult> QuerySqlAsync(string query)
    {
        await Task.Delay(300); // Fast
        return new SqlResult { Rows = new List<string> { "Row1", "Row2" } };
    }

    public async Task<VectorResult> QueryVectorAsync(string query, CancellationToken ct)
    {
        // Simulate a slow query that might timeout
        try
        {
            await Task.Delay(1500, ct); // Exceeds typical timeout
            return new VectorResult { Embeddings = new List<double> { 0.1, 0.5, 0.9 } };
        }
        catch (TaskCanceledException)
        {
            throw; // Propagate cancellation
        }
    }

    public async Task<GraphResult> QueryGraphAsync(string query)
    {
        await Task.Delay(200); // Fast
        return new GraphResult { Nodes = new List<string> { "NodeA", "NodeB" } };
    }
}

public class KnowledgeGraphAggregator
{
    private readonly DatabaseService _db = new DatabaseService();

    // 4. Asynchronous Aggregation Logic
    private async Task<KnowledgeGraphResponse> MergeDataAsync(SqlResult sql, VectorResult vec, GraphResult graph)
    {
        // Simulate complex async calculation
        await Task.Delay(100);
        
        var unified = new List<string>();
        
        // Logic: Link SQL Rows to Graph Nodes (mock logic)
        unified.AddRange(sql.Rows);
        unified.AddRange(graph.Nodes);
        
        // If vector exists, append embeddings as metadata strings
        if (vec != null)
        {
            unified.Add($"VectorCount: {vec.Embeddings.Count}");
        }

        return new KnowledgeGraphResponse
        {
            UnifiedNodes = unified,
            Metadata = "Aggregation Complete"
        };
    }

    public async Task<KnowledgeGraphResponse> BuildUnifiedGraphAsync(string userQuery)
    {
        // 5. Fallback Mechanism: Use a shorter timeout for the Vector DB specifically
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // 1 second timeout

        // Start tasks
        var sqlTask = _db.QuerySqlAsync(userQuery);
        var graphTask = _db.QueryGraphAsync(userQuery);
        
        // Vector task with specific cancellation token
        var vectorTask = _db.QueryVectorAsync(userQuery, cts.Token);

        // Wait for the "Fast" tasks (SQL and Graph) first to ensure core data
        // Or wait for all, but handle the Vector exception specifically.
        // Strategy: Wait for SQL and Graph, then check Vector.
        
        var coreTasks = new[] { sqlTask, graphTask };
        await Task.WhenAll(coreTasks);

        // Now handle the Vector task (the "Best Effort" part)
        VectorResult vectorResult = null;
        try
        {
            vectorResult = await vectorTask;
        }
        catch (TaskCanceledException)
        {
            // Fallback: Vector timed out
            vectorResult = null; // Use default empty/null
        }

        // 4. Call the asynchronous aggregator
        return await MergeDataAsync(sqlTask.Result, vectorResult, graphTask.Result);
    }
}

// Example Usage
public class Program
{
    public static async Task Main()
    {
        var aggregator = new KnowledgeGraphAggregator();
        var response = await aggregator.BuildUnifiedGraphAsync("complex query");

        Console.WriteLine($"Unified Nodes Count: {response.UnifiedNodes.Count}");
        Console.WriteLine($"Metadata: {response.Metadata}");
        // Note: Vector result will be missing due to simulated timeout
    }
}
