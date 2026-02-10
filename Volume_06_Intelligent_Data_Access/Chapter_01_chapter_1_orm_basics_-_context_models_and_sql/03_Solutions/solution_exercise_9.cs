
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

// Source File: solution_exercise_9.cs
// Description: Solution for Exercise 9
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

// 1. Benchmark Suite
public class VectorBenchmark
{
    private readonly AppDbContext _context;
    private readonly float[] _queryVector;

    public VectorBenchmark(AppDbContext context)
    {
        _context = context;
        _queryVector = new float[128]; // Example dimension
        new Random().NextBytes(_queryVector.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public async Task RunAll()
    {
        Console.WriteLine("Starting Benchmarks...");
        
        await BenchmarkInMemoryVsDbSide();
        await BenchmarkTrackedVsNoTracking();
        await BenchmarkBatchVsSingle();
        
        Console.WriteLine("Benchmarks Complete.");
    }

    private async Task BenchmarkInMemoryVsDbSide()
    {
        // Scenario 1: Client-side calculation
        var sw = Stopwatch.StartNew();
        var docs = await _context.Documents.ToListAsync();
        var clientResults = docs
            .Select(d => new { Doc = d, Score = CalculateSim(d.Vector, _queryVector) })
            .Where(x => x.Score > 0.7)
            .ToList();
        sw.Stop();
        Console.WriteLine($"Client-Side Calc: {sw.ElapsedMilliseconds}ms");

        // Scenario 2: DB-side (Simulated via raw SQL or DbFunction)
        // Assuming a DbFunction exists that maps to SQL
        sw.Restart();
        var dbResults = await _context.Documents
            .Where(d => AppDbContext.CalculateCosineSimilarity(d.Vector, _queryVector) > 0.7)
            .ToListAsync();
        sw.Stop();
        Console.WriteLine($"DB-Side Calc: {sw.ElapsedMilliseconds}ms");
    }

    private async Task BenchmarkTrackedVsNoTracking()
    {
        // Tracked (Change Tracking overhead)
        var sw = Stopwatch.StartNew();
        var tracked = await _context.Documents.ToListAsync();
        sw.Stop();
        Console.WriteLine($"Tracked: {sw.ElapsedMilliseconds}ms");

        // No Tracking
        sw.Restart();
        var untracked = await _context.Documents.AsNoTracking().ToListAsync();
        sw.Stop();
        Console.WriteLine($"No Tracking: {sw.ElapsedMilliseconds}ms");
    }

    private async Task BenchmarkBatchVsSingle()
    {
        // Single Query
        var sw = Stopwatch.StartNew();
        foreach (var id in Enumerable.Range(1, 100))
        {
            await _context.Documents.FindAsync(id);
        }
        sw.Stop();
        Console.WriteLine($"Single Queries (100): {sw.ElapsedMilliseconds}ms");

        // Batch Query
        sw.Restart();
        var ids = Enumerable.Range(1, 100).ToList();
        await _context.Documents.Where(d => ids.Contains(d.Id)).ToListAsync();
        sw.Stop();
        Console.WriteLine($"Batch Query: {sw.ElapsedMilliseconds}ms");
    }

    private double CalculateSim(float[] a, float[] b) => 0.9; // Mock
}

// 2. Custom Interceptor for Measurement
public class PerformanceInterceptor : DbCommandInterceptor
{
    private readonly Stopwatch _stopwatch = new();

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken token = default)
    {
        _stopwatch.Restart();
        return base.ReaderExecutingAsync(command, eventData, result, token);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken token = default)
    {
        _stopwatch.Stop();
        if (_stopwatch.ElapsedMilliseconds > 100) // Threshold
        {
            // Log to telemetry
            Console.WriteLine($"[SLOW QUERY] {command.CommandText} took {_stopwatch.ElapsedMilliseconds}ms");
        }
        return base.ReaderExecutedAsync(command, eventData, result, token);
    }
}

// 3. Index Analysis (Conceptual)
public class IndexRecommendationEngine
{
    public string AnalyzeQuery(string sql)
    {
        if (sql.Contains("VectorSimilarity"))
        {
            return "Recommendation: Create IVFFLAT index on vector column for PostgreSQL, or Computed Column + B-Tree for SQL Server.";
        }
        return "No specific recommendations.";
    }
}
