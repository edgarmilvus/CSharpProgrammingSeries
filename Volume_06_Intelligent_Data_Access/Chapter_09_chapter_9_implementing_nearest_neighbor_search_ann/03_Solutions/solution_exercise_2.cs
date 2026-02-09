
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Chapter9.Exercise2
{
    // 1. Entity Definition (Reused from Ex 1)
    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    public class DocumentContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 3. DbContext Integration: Ensure Npgsql is used
            // Note: In a real app, connection string goes here.
            optionsBuilder.UseNpgsql("Host=localhost;Database=vector_db;Username=postgres;Password=password", 
                o => o.UseVector()); // Hypothetical extension method for pgvector support
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the vector type hint if needed, though migrations handle the DDL.
            modelBuilder.Entity<Document>()
                .Property(d => d.Embedding)
                .HasColumnType("vector(1536)");
        }
    }

    // 2. Migration Customization
    [DbContext(typeof(DocumentContext))]
    [Migration("20231027000000_AddDocumentVectors")]
    public class AddDocumentVectors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conditional check for the extension
            // Note: Creating extensions usually requires superuser privileges.
            // In production, DBAs often pre-install extensions.
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'vector') THEN
                        -- Attempt to create extension. 
                        -- WARNING: Requires superuser privileges. 
                        -- If this fails, the migration will abort, alerting the admin.
                        CREATE EXTENSION IF NOT EXISTS vector; 
                    END IF;
                END $$;
            ");

            // Create Table with raw SQL
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS Documents (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""Title"" text NULL,
                    ""Content"" text NULL,
                    ""Embedding"" vector(1536)
                );
            ");

            // 4. Create HNSW Index
            // Explanation: 'vector_cosine_ops' is an operator class for Cosine Distance (<=>).
            // If you use Euclidean distance (<->), you should use 'vector_l2_ops'.
            // Using the wrong operator class will result in a sequential scan (slow) or incorrect results.
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_documents_embedding_hnsw 
                ON Documents 
                USING hnsw (Embedding vector_cosine_ops);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Documents");
            // Note: We generally do not drop extensions in migrations as they might be used by other tables.
        }
    }

    // 5. Querying with FromSqlRaw
    public static class DocumentSearchService
    {
        public static async Task<List<Document>> SearchAnnAsync(DocumentContext context, float[] queryVector, int topK)
        {
            // Convert float array to Postgres vector literal format for SQL
            // Format: '[1,2,3]' or '1,2,3' depending on pgvector version. 
            // Recent versions accept '1,2,3' for input, but SQL operators usually require explicit casting or specific syntax.
            // The '<=>' operator is the cosine distance operator in pgvector.
            
            var vectorString = string.Join(",", queryVector);
            
            // Using raw SQL. Note: EF Core usually maps parameters, but pgvector operators 
            // might require specific syntax. We format the string directly here for the operator.
            var sql = $@"
                SELECT * FROM Documents 
                ORDER BY Embedding <=> '{vectorString}'::vector 
                LIMIT {topK};
            ";

            // In a real scenario, we'd use parameters to prevent SQL injection, 
            // but pgvector syntax inside ORDER BY can be tricky with parameters.
            // A safer approach often involves a user-defined function (STABLE) wrapper.
            
            return await context.Documents
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
