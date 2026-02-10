
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class RagContext : DbContext
{
    public DbSet<Document> Documents { get; set; }

    // 1. Define a DbFunction (Static Method Mapping)
    // This maps a C# method to a SQL function (or specific SQL fragment)
    [DbFunction("CalculateCosineSimilarity", "dbo")] // Assumes SQL Server UDF
    public static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        => throw new NotImplementedException("This method is only for LINQ translation.");

    // Alternative: EF.Functions.TransactSql for raw SQL if UDF isn't available
    public static double CalculateCosineSimilaritySql(float[] source, float[] target)
        => EF.Functions.TransactSql($"VECTOR_COSINE({source}, {target})");
}

public class RagSearchService
{
    private readonly RagContext _context;

    public RagSearchService(RagContext context) => _context = context;

    public async Task<List<DocumentResult>> EfficientSearch(float[] queryVector, int pageSize, int? lastId = null)
    {
        // 2. Rewritten Query
        var query = _context.Documents
            .AsSplitQuery()       // 4. Split queries for related data (Chunks)
            .AsNoTracking()       // 4. Read-only performance boost
            .Where(d => d.Content.Contains("AI")); // If indexed, this is efficient. If not, avoid.

        // 3. Keyset Pagination (Seek Method)
        // Instead of Skip(10).Take(10), we seek the next page based on the last seen ID.
        // This is much faster on large datasets.
        if (lastId.HasValue)
        {
            query = query.Where(d => d.Id > lastId.Value);
        }

        // 4. Server-side Vector Similarity
        // We project the result to include the calculated similarity.
        // Note: We pass the 'queryVector' as a parameter. EF Core translates this to a SQL parameter.
        var results = await query
            .Select(d => new 
            {
                Document = d,
                // Using the DbFunction defined in the context
                Similarity = RagContext.CalculateCosineSimilarity(d.Vector, queryVector)
            })
            .Where(x => x.Similarity > 0.7) // Filter on the server
            .OrderBy(x => x.Similarity)     // Sort on the server
            .Take(pageSize)                 // Limit results
            .ToListAsync();

        // Transform to DTO
        return results.Select(r => new DocumentResult 
        { 
            Document = r.Document, 
            Similarity = r.Similarity 
        }).ToList();
    }
}

public class DocumentResult
{
    public Document Document { get; set; }
    public double Similarity { get; set; }
}
