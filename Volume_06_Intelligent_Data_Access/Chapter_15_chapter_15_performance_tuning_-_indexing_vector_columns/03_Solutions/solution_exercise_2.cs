
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
using System.ComponentModel.DataAnnotations;

// 1. Database Setup & EF Core Mapping
public class UserInteraction
{
    [Key]
    public int Id { get; set; }
    
    // SQL Server vector mapping usually requires specific types (e.g., Vector<float> or byte[])
    // We use byte[] here as a generic representation for the "vector" column type in SQL Server
    public byte[] Embedding { get; set; } = Array.Empty<byte>();
}

public class RecommendationContext : DbContext
{
    public DbSet<UserInteraction> UserInteractions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Assuming SQL Server with vector extensions
        optionsBuilder.UseSqlServer("Server=.;Database=RecEngine;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Note: EF Core SQL Server provider might not fully support IVFFlat DDL generation yet.
        // We map the entity, but the index creation is often done via Raw SQL for specific vector extensions.
        modelBuilder.Entity<UserInteraction>(entity =>
        {
            entity.ToTable("UserInteractions");
            // Map the vector column
            entity.Property(e => e.Embedding).HasColumnName("Embedding");
        });
    }
}

public class TuningService
{
    private readonly RecommendationContext _context;

    public TuningService(RecommendationContext context)
    {
        _context = context;
    }

    // 4. Probe Parameter Adjustment
    public async Task<List<UserInteraction>> SearchWithDynamicProbeAsync(byte[] queryVector, int probeValue)
    {
        // Adjust the probe setting for the current session
        // This is specific to SQL Server's vector search implementation
        await _context.Database.ExecuteSqlRawAsync($"SET VECTOR_SEARCH_PROBE = {probeValue}");

        // Execute the search query
        // Note: The actual LINQ query depends on the SQL Server EF Core provider's support for vector distance.
        // Here we simulate the query structure.
        return await _context.UserInteractions
            .OrderBy(u => CalculateDistance(u.Embedding, queryVector)) // Hypothetical translation
            .Take(10)
            .ToListAsync();
    }

    private double CalculateDistance(byte[] v1, byte[] v2)
    {
        // Placeholder for Cosine Distance calculation
        return 0.0; 
    }

    // 2. Index Creation (Raw SQL DDL)
    // This would typically be executed via migrations or a setup script.
    public string GetIndexCreationSql()
    {
        // Calculation: sqrt(50,000,000) ≈ 7071. 
        // IVFFlat requires tuning 'lists' based on dataset size. 
        // 7071 is a theoretical starting point, but often 'lists' is set to sqrt(N) to sqrt(4*N).
        int lists = 7071; 
        
        return $@"
            CREATE VECTOR INDEX ix_user_interactions
            ON UserInteractions(Embedding)
            WITH (DISTANCE_COSINE, LISTS = {lists});
        ";
    }
}

// 5. Trade-off Analysis (Instructor's Analysis section below)
