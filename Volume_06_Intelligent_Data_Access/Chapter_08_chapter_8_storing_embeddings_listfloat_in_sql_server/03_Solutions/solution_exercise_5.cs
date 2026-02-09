
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class BenchmarkService
{
    private readonly VectorContext _context;

    public BenchmarkService(VectorContext context)
    {
        _context = context;
    }

    public async Task BenchmarkVectorIndex()
    {
        // 1. Generate Data
        var random = new Random();
        var records = Enumerable.Range(1, 50000).Select(i => new VectorRecord
        {
            Id = Guid.NewGuid(),
            Content = $"Document {i}",
            Embedding = Enumerable.Range(0, 768).Select(_ => (float)random.NextDouble()).ToList()
        }).ToList();

        // 2. Bulk Insert
        Console.WriteLine("Inserting 50,000 records...");
        var repo = new VectorRepository(_context);
        await repo.BulkInsertAsync(records);
        Console.WriteLine("Insertion complete.");

        // 3. Query Execution (Pre-Index)
        var queryVector = Enumerable.Range(0, 768).Select(_ => (float)random.NextDouble()).ToList();
        var sw = Stopwatch.StartNew();
        
        // Execute the search (Simulated)
        await repo.SearchSimilarAsync(queryVector);
        
        sw.Stop();
        Console.WriteLine($"Time without Index: {sw.ElapsedMilliseconds} ms");

        // 4. Create Index (SQL Execution)
        // Note: This requires dynamic SQL execution or manual execution in SSMS.
        // We simulate the SQL command here.
        string createIndexSql = @"
            -- Assuming SQL Server 2022+ with Vector Support
            CREATE VECTOR INDEX IX_Vector_Embedding 
            ON VectorRecords(NormalizedEmbedding) 
            WITH (DISTANCE_METRIC = 'Euclidean', INDEX_TYPE = 'IVF');
        ";
        Console.WriteLine("Creating Index... (Execute manually if not supported in EF)");
        // await _context.Database.ExecuteSqlRaw(createIndexSql); 

        // 5. Query Execution (Post-Index)
        sw.Restart();
        await repo.SearchSimilarAsync(queryVector);
        sw.Stop();
        Console.WriteLine($"Time with Index: {sw.ElapsedMilliseconds} ms");
    }
}
