
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

// 1. Hybrid DbContext
public class HybridDesignContext : DbContext
{
    // Relational Entities (Code-First)
    public DbSet<User> Users { get; set; }

    // Vector Entities (Database-First)
    // These are mapped to existing tables in the vector DB
    public DbSet<VectorDocument> VectorDocuments { get; set; }

    public HybridDesignContext(DbContextOptions<HybridDesignContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Code-First Users
        modelBuilder.Entity<User>().ToTable("Users");
        
        // Configure Database-First VectorDocuments
        // We explicitly map to the existing table and ignore auto-migration for it if possible
        modelBuilder.Entity<VectorDocument>().ToTable("VectorDocuments", schema: "vectors");
        
        // 2. Schema Validation Mechanism
        // We can hook into ModelValidating event to check capabilities
        this.GetService<IModelRuntimeValidator>().OnModelValidated = (model, context) =>
        {
            var vectorEntity = model.FindEntityType(typeof(VectorDocument));
            if (vectorEntity == null) return;

            var vectorProp = vectorEntity.FindProperty("Embedding");
            if (vectorProp != null)
            {
                // Check if the provider supports the vector type
                var storeType = vectorProp.GetColumnType();
                if (!storeType.Contains("vector")) 
                {
                    throw new InvalidOperationException("Vector property mapped to non-vector column!");
                }
            }
        };
    }
}

// 3. Custom Design-Time Factory
public class HybridContextFactory : IDesignTimeDbContextFactory<HybridDesignContext>
{
    public HybridDesignContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HybridDesignContext>();

        // Logic to determine connection string based on environment or args
        // For Code-First migrations, we might point to SQL Server
        optionsBuilder.UseSqlServer("Server=relational-db;Database=AppDb;Trusted_Connection=True;");

        // Note: For Database-First reverse engineering of the Vector DB:
        // You would run: dotnet ef dbcontext scaffold ... --context VectorDbContext
        // Then merge the contexts or use inheritance.

        return new HybridDesignContext(optionsBuilder.Options);
    }
}

// 4. Idempotent Migration Scripts
// In the DbContext, we override SaveChanges to generate raw SQL for vector indexes
// if EF Core Migrations don't support them natively.
public class MigrationManager
{
    public void ApplyMigrations(HybridDesignContext context)
    {
        // Ensure relational migrations
        context.Database.Migrate();

        // Manually apply vector index if it doesn't exist
        var exists = context.Database.SqlQueryRaw<int>(
            "SELECT 1 FROM sys.indexes WHERE name = 'IX_VectorDocuments_Embedding'").Any();
        
        if (!exists)
        {
            // Raw SQL for vector index (SQL Server Computed Column approach or PostgreSQL)
            context.Database.ExecuteSqlRaw(
                "CREATE INDEX IX_VectorDocuments_Embedding ON VectorDocuments.Embedding USING ivfflat (vector_l2_ops)");
        }
    }
}

// Mock Entities
public class User { public int Id { get; set; } }
public class VectorDocument { public int Id { get; set; } public float[] Embedding { get; set; } }
