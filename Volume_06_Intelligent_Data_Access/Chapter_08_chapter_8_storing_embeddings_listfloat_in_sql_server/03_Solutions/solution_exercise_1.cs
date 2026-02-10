
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

// 1. Entity Definition
public class VectorRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    
    // Raw embedding storage
    // Note: In a real scenario, consider using 'float[]' or SQL 'VECTOR' type for better performance.
    // For this exercise, we map List<float> to a string or JSON column, or a varbinary if using specific SQL types.
    // Assuming we store raw embeddings in a JSON string for flexibility in EF Core, 
    // but for SQL Server Vector types, we might need a ValueConverter.
    public List<float> Embedding { get; set; } 

    // Computed Column: Normalized version of Embedding for fast Cosine Similarity (dot product)
    // We store this as a byte[] (varbinary) representing the normalized vector.
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[] NormalizedEmbedding { get; set; }
}

// 2. DbContext Configuration
public class VectorContext : DbContext
{
    public DbSet<VectorRecord> VectorRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Replace with your actual connection string
        optionsBuilder.UseSqlServer("Server=.;Database=VectorDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VectorRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Configure Embedding as a JSON string for storage (EF Core 8+ supports this natively)
            entity.Property(e => e.Embedding)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<float>>(v, null));

            // Configure Computed Column for Normalized Embedding
            // This SQL expression calculates the L2-normalized vector.
            // Note: This requires SQL Server 2022+ or a scalar function definition in older versions.
            // We assume a user-defined function 'dbo.NormalizeVector' exists or use built-in SQL math.
            // For this example, we simulate the SQL generation.
            entity.Property(e => e.NormalizedEmbedding)
                .HasColumnType("varbinary(max)")
                .HasComputedColumnSql("dbo.NormalizeVector(Embedding) PERSISTED"); 
        });
    }
}

// 3. Repository Implementation
public class VectorRepository
{
    private readonly VectorContext _context;

    public VectorRepository(VectorContext context)
    {
        _context = context;
    }

    // Bulk Insert using SqlBulkCopy (Optimized for high volume)
    public async Task BulkInsertAsync(IEnumerable<VectorRecord> records)
    {
        // We must handle the conversion of List<float> to a format SqlBulkCopy understands (e.g., JSON string)
        // or use a DataTable.
        var table = new System.Data.DataTable();
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("Content", typeof(string));
        table.Columns.Add("Embedding", typeof(string)); // Storing as JSON string

        foreach (var record in records)
        {
            table.Rows.Add(
                record.Id,
                record.Content,
                System.Text.Json.JsonSerializer.Serialize(record.Embedding)
            );
        }

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            await connection.OpenAsync();
            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "VectorRecords";
                
                // Map columns
                bulkCopy.ColumnMappings.Add("Id", "Id");
                bulkCopy.ColumnMappings.Add("Content", "Content");
                bulkCopy.ColumnMappings.Add("Embedding", "Embedding");
                
                // The NormalizedEmbedding is computed by SQL, so we don't map it here.

                try
                {
                    await bulkCopy.WriteToServerAsync(table);
                }
                catch (Exception ex)
                {
                    // Handle logging/exception
                    throw new InvalidOperationException("Bulk insert failed", ex);
                }
            }
        }
    }

    // Search Similarity
    public async Task<List<VectorRecord>> SearchSimilarAsync(List<float> queryVector, int topN = 5)
    {
        // 1. Validate Dimensions (Edge Case)
        // We need to check against the dimensionality of stored vectors. 
        // For this example, we assume a fixed dimension (e.g., 384) or check the first record.
        // In a real app, store dimensionality in metadata.
        var expectedDimensions = 384; // Example
        if (queryVector.Count != expectedDimensions)
        {
            throw new ArgumentOutOfRangeException(nameof(queryVector), 
                $"Query vector dimension {queryVector.Count} does not match expected dimension {expectedDimensions}.");
        }

        // 2. Normalize the query vector (Client-side calculation for dot product equivalence)
        // Euclidean distance on normalized vectors is proportional to Cosine Similarity.
        // d(a,b) = sqrt(2 - 2*cos(a,b)) for normalized vectors.
        // However, SQL Server's VECTOR_DISTANCE function handles this internally.
        // We will pass the raw vector and let SQL calculate distance against the normalized column.
        
        // Convert List<float> to byte[] for passing to SQL (if using varbinary parameter)
        // Or serialize to JSON string if the SQL function accepts string.
        // Here we assume we use a SQL function that accepts a JSON string.
        var queryVectorJson = System.Text.Json.JsonSerializer.Serialize(queryVector);

        // 3. Execute Query
        // We use Euclidean distance on the NormalizedEmbedding column.
        // Since NormalizedEmbedding is normalized, Euclidean distance correlates with Cosine Similarity.
        // Distance formula: sqrt(sum((a_i - b_i)^2))
        
        // Note: EF Core 8+ supports raw SQL in queries. 
        // We assume a SQL User Defined Function (UDF) mapped to `EF.Functions.VectorDistance` or similar.
        // Since standard EF doesn't have built-in Vector support yet, we use FromSqlRaw or a UDF.
        
        // Hypothetical SQL: 
        // SELECT TOP 5 * FROM VectorRecords 
        // ORDER BY VECTOR_DISTANCE('cosine', NormalizedEmbedding, @queryVector) ASC
        
        // For this solution, we will simulate the EF Core implementation using a computed distance.
        // We will project the result to include the distance.
        
        // Using FromSqlInterpolated for safety
        var results = await _context.VectorRecords
            .FromSqlInterpolated($@"
                SELECT TOP({topN}) Id, Content, Embedding, NormalizedEmbedding, 
                       dbo.CalculateEuclideanDistance(NormalizedEmbedding, {queryVectorJson}) AS Distance
                FROM VectorRecords
                ORDER BY Distance ASC
            ")
            .ToListAsync();

        return results;
    }
}
