
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

// Source File: solution_exercise_15.cs
// Description: Solution for Exercise 15
// ==========================================

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Feature Flag Service
public interface IFeatureFlagService
{
    bool IsEnabled(string feature);
    string GetVariant(string experiment); // For A/B testing (e.g., "A" or "B")
}

public class ConfigurationFeatureFlag : IFeatureFlagService
{
    private readonly IConfiguration _config;
    public ConfigurationFeatureFlag(IConfiguration config) => _config = config;
    
    public bool IsEnabled(string feature) => _config.GetValue<bool>($"Features:{feature}");
    public string GetVariant(string experiment) => _config.GetValue<string>($"Experiments:{experiment}") ?? "A";
}

// 2. Similarity Strategy Interface
public interface ISimilarityStrategy
{
    string Name { get; }
    double Calculate(float[] a, float[] b);
}

public class CosineStrategy : ISimilarityStrategy
{
    public string Name => "Cosine";
    public double Calculate(float[] a, float[] b)
    {
        // Implementation
        return 0.9; 
    }
}

public class DotProductStrategy : ISimilarityStrategy
{
    public string Name => "DotProduct";
    public double Calculate(float[] a, float[] b)
    {
        // Implementation
        return 1.5;
    }
}

// 3. Strategy Factory
public class SimilarityStrategyFactory
{
    private readonly IFeatureFlagService _flags;

    public SimilarityStrategyFactory(IFeatureFlagService flags) => _flags = flags;

    public ISimilarityStrategy GetStrategy()
    {
        // A/B Test Logic
        var variant = _flags.GetVariant("VectorSimilarityAlgo");
        
        return variant switch
        {
            "B" => new DotProductStrategy(),
            _ => new CosineStrategy() // Default A
        };
    }
}

// 4. Logging System
public class QueryLogger
{
    public void LogQuery(string algorithm, double score, long latencyMs)
    {
        // Store in DB or Analytics platform
        Console.WriteLine($"[Experiment] Algo: {algorithm}, Score: {score}, Latency: {latencyMs}ms");
    }
}

// 5. Hybrid Search (Combining Metrics)
public class HybridSearchService
{
    private readonly SimilarityStrategyFactory _factory;
    private readonly QueryLogger _logger;

    public async Task<List<DocumentResult>> Search(float[] vector)
    {
        var strategy = _factory.GetStrategy();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Fetch candidates (e.g., from Vector DB)
        var candidates = await GetCandidatesAsync(vector); 

        // Apply the chosen strategy
        var results = candidates
            .Select(d => new { Doc = d, Score = strategy.Calculate(d.Vector, vector) })
            .OrderByDescending(x => x.Score)
            .Take(10)
            .ToList();

        sw.Stop();

        // Log for analysis
        _logger.LogQuery(strategy.Name, results.First().Score, sw.ElapsedMilliseconds);

        return results.Select(r => new DocumentResult { Document = r.Doc, Similarity = r.Score }).ToList();
    }

    private async Task<List<Document>> GetCandidatesAsync(float[] vector) => new(); // Mock
}
